using System;
using System.Dynamic;
using System.IO;
using UnityEngine;

static class CoordinateUtil
{
    /// <summary>
    /// HoloLens2 ��ǥ�迡�� Unity ��ǥ��� ��ȯ.
    /// ���ν��� ����ȭ�� �ʿ��ϴ�.
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static Matrix4x4 ToUnityCoordinateSystem(this Matrix4x4 src)
    {
        Vector3 pos = src.GetColumn(3);
        Vector3 eulerAngle = src.rotation.eulerAngles;

        eulerAngle.y *= -1.0f;
        eulerAngle.z *= -1.0f;

        Vector3 eulerAngleInRad;
        eulerAngleInRad.x = Mathf.PI * eulerAngle.x / 180.0f;
        eulerAngleInRad.y = Mathf.PI * eulerAngle.y / 180.0f;
        eulerAngleInRad.z = Mathf.PI * eulerAngle.z / 180.0f;

        Matrix4x4 vX;
        vX.m00 = 1.0f; vX.m01 = 0.0f; vX.m02 = 0.0f; vX.m03 = 0.0f;
        vX.m10 = 0.0f; vX.m11 = Mathf.Cos(eulerAngleInRad.x); vX.m12 = -Mathf.Sin(eulerAngleInRad.x); vX.m13 = 0.0f;
        vX.m20 = 0.0f; vX.m21 = Mathf.Sin(eulerAngleInRad.x); vX.m22 = Mathf.Cos(eulerAngleInRad.x); vX.m23 = 0.0f;
        vX.m30 = 0.0f; vX.m31 = 0.0f; vX.m32 = 0.0f; vX.m33 = 1.0f;

        Matrix4x4 vY;
        vY.m00 = Mathf.Cos(eulerAngleInRad.y); vY.m01 = 0.0f; vY.m02 = Mathf.Sin(eulerAngleInRad.y); vY.m03 = 0.0f;
        vY.m10 = 0.0f; vY.m11 = 1.0f; vY.m12 = 0.0f; vY.m13 = 0.0f;
        vY.m20 = -Mathf.Sin(eulerAngleInRad.y); vY.m21 = 0.0f; vY.m22 = Mathf.Cos(eulerAngleInRad.y); vY.m23 = 0.0f;
        vY.m30 = 0.0f; vY.m31 = 0.0f; vY.m32 = 0.0f; vY.m33 = 1.0f;

        Matrix4x4 vZ;
        vZ.m00 = Mathf.Cos(eulerAngleInRad.z); vZ.m01 = -Mathf.Sin(eulerAngleInRad.z); vZ.m02 = 0.0f; vZ.m03 = 0.0f;
        vZ.m10 = Mathf.Sin(eulerAngleInRad.z); vZ.m11 = Mathf.Cos(eulerAngleInRad.z); vZ.m12 = 0.0f; vZ.m13 = 0.0f;
        vZ.m20 = 0.0f; vZ.m21 = 0.0f; vZ.m22 = 1.0f; vZ.m23 = 0.0f;
        vZ.m30 = 0.0f; vZ.m31 = 0.0f; vZ.m32 = 0.0f; vZ.m33 = 1.0f;

        Matrix4x4 vR3 = vY * vX * vZ;
        Vector4 vP = new Vector4(-pos.x, pos.y, pos.z, 1);

        Matrix4x4 dst = vR3;
        dst.SetColumn(3, vP);

        return dst;
    }


    public static Vector3 Ros2Unity(this Vector3 vector3)
    {
        return new Vector3(-vector3.y, vector3.z, vector3.x);
    }
    public static Quaternion Ros2Unity(this Quaternion quaternion)
    {
        return new Quaternion(quaternion.y, -quaternion.z, -quaternion.x, quaternion.w);
    }
}

[Serializable]
public class BOPFrame
{
    [SerializeField]
    public GameObject frame_root;

    [SerializeField]
    public GameObject go_camera;

    [SerializeField]
    public GameObject go_pointCloud;

    [SerializeField]
    public SerializableDictionary<int, GameObject> go_models = new SerializableDictionary<int, GameObject>();

    [SerializeField]
    public GameObject go_poses;
    
    public void CreateFrame(int im_id, BOPDatasetParams datasetParams)
    {
        frame_root = new GameObject("Frame");
        frame_root.transform.localScale = new Vector3(1, -1, 1);
        go_camera = CreateCamera(datasetParams.camera_info, datasetParams.scene_camera[im_id]);
        go_camera.transform.parent = frame_root.transform;
        //go_models = CreateModels(datasetParams.base_path, datasetParams.model_info);

        UpdateScene(im_id, datasetParams);
        LoadAllCameraPoses(datasetParams.scene_gt[im_id][0].obj_id, datasetParams);
    }

    GameObject CreateCamera(CameraInfo cameraInfo, SceneCamera sceneCamera)
    {
        GameObject camera = new GameObject("Camera");
        var cam = camera.AddComponent<Camera>();
        cam.usePhysicalProperties = true;

        float fx = sceneCamera.cam_K[0];
        float fy = sceneCamera.cam_K[4];
        float cx = sceneCamera.cam_K[2];
        float cy = sceneCamera.cam_K[5];
        float width = cameraInfo.width;
        float height = cameraInfo.height;


        // How to  adjust intrinsic parameter to unity camera.
        // note, https://answers.unity.com/questions/814701/how-to-simulate-unity-pinhole-camera-from-its-intr.html
        float f = fx * 0.1f; // f can be arbitrary, as long as sensor_size is resized to to make ax,ay consistient
        float sizeX = f * width / fx;
        float sizeY = f * height / fy;

        float shiftX = -(cx - width / 2.0f) / width;
        float shiftY = (cy - height / 2.0f) / height;

        cam.focalLength = f;
        cam.sensorSize = new Vector2(sizeX, sizeY);
        cam.lensShift = new Vector2(shiftX, shiftY);

        cam.clearFlags = CameraClearFlags.Depth;
        cam.nearClipPlane = 0.03f;
        cam.farClipPlane = 100f;

        var img = camera.AddComponent<BlendDuringRender>();
        img.Init();

        return camera;
    }
    void LoadModel(string base_path, int obj_id, SerializableDictionary<int, ModelInfo> model_info)
    {
        //var output  = new SerializableDictionary<int, GameObject><int, GameObject>();

        //var go_model = new GameObject(model.Key.ToString());
        string model_path = BOPPath.get_model_path(base_path, obj_id);
        //var mesh_info = PointCloudGenerator.LoadPointCloud(model_path);
        //var go_model = PointCloudGenerator.ToGameObject(mesh_info, model.Key.ToString(), 0.1f);
        var go_model = PointCloudLoader.LoadPly(model_path);
        go_model.transform.parent = frame_root.transform;
        go_model.name = obj_id.ToString();

        //go_model.transform.localScale = new Vector3(1, -1, 1);
        //go_model.transform.Rotate(-90, 0, 0);
        //Debug.Log(go_model.transform.localToWorldMatrix);


        //Matrix4x4 conversionMatrix = new Matrix4x4();
        //conversionMatrix.SetRow(0, new Vector4(1, 0, 0, 0));
        //conversionMatrix.SetRow(1, new Vector4(0, -1, 0, 0));
        //conversionMatrix.SetRow(2, new Vector4(0, 0, 1, 0));
        //conversionMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
        //conversionMatrix = conversionMatrix * go_model.transform.localToWorldMatrix;
        //ApplyTransform(go_model, conversionMatrix);
        //Debug.Log(go_model.transform.localToWorldMatrix);

        go_models.Add(obj_id, go_model);
        //go_model.transform.localScale = new Vector3(1, 1, 1);
        //return output;
    }

    void LoadAllCameraPoses(int obj_id, BOPDatasetParams datasetParams)
    {
        go_poses = new GameObject("Poses");


        var sceneGT = datasetParams.scene_gt;
        //load first models
        //
        Matrix4x4 rotx_270 = Matrix4x4.identity;
        rotx_270.SetRow(0, new Vector4(1, 0, 0, 0));
        rotx_270.SetRow(1, new Vector4(0, 0, 1, 0));
        rotx_270.SetRow(2, new Vector4(0, -1, 0, 0));
        rotx_270.SetRow(3, new Vector4(0, 0, 0, 1));
        
        foreach (var im_id in sceneGT.Keys)
        {
            foreach (var gt in sceneGT[im_id])
            {
                Matrix4x4 rt = Matrix4x4.identity;

                if (gt.obj_id == obj_id)
                {
                    rt.m00 = gt.cam_R_m2c[0]; rt.m01 = gt.cam_R_m2c[1]; rt.m02 = gt.cam_R_m2c[2]; rt.m03 = gt.cam_t_m2c[0];
                    rt.m10 = gt.cam_R_m2c[3]; rt.m11 = gt.cam_R_m2c[4]; rt.m12 = gt.cam_R_m2c[5]; rt.m13 = gt.cam_t_m2c[1];
                    rt.m20 = gt.cam_R_m2c[6]; rt.m21 = gt.cam_R_m2c[7]; rt.m22 = gt.cam_R_m2c[8]; rt.m23 = gt.cam_t_m2c[2];

                    rt = rt * rotx_270;
                    rt = rt.inverse;

                    GameObject go_camera_pose = new GameObject("Cam_" + im_id.ToString());
                    go_camera_pose.AddComponent<ImageControllerMono>();
                    go_camera_pose.transform.localPosition = rt.GetColumn(3) * 0.001f;
                    go_camera_pose.transform.localRotation = rt.rotation;

                    go_camera_pose.transform.parent = go_poses.transform;
                }
            }
        }

        go_poses.transform.parent = frame_root.transform;
        go_poses.transform.localPosition = Vector3.zero;
        go_poses.transform.localRotation = Quaternion.identity;
        go_poses.transform.localScale = Vector3.one;
    }
    
    public void ApplyTransform(GameObject target, Matrix4x4 conversionMatrix, float scalef)
    {
        Vector3 position = conversionMatrix.GetColumn(3) * scalef;
        Vector3 scale = new Vector3(
            conversionMatrix.GetColumn(0).magnitude,
            conversionMatrix.GetColumn(1).magnitude,
            conversionMatrix.GetColumn(2).magnitude
        );
        Vector3 forward = conversionMatrix.GetColumn(2);
        Vector3 up = conversionMatrix.GetColumn(1);
        Quaternion rotation = Quaternion.LookRotation(forward, up);

        // 추출한 위치, 회전, 스케일로 Transform 업데이트
        target.transform.localPosition = position;
        target.transform.localRotation = rotation;
        target.transform.localScale = scale;
    }


    // option 1: model-centric, centric-model-id
    // option 2: camera-centric
    public void UpdateScene(int im_id, BOPDatasetParams datasetParams)
    {
        foreach (var model in go_models.Values)
        {
            model.SetActive(false);
        }
        var sceneGT = datasetParams.scene_gt[im_id];

        Matrix4x4 rotx_270 = Matrix4x4.identity;
        rotx_270.SetRow(0, new Vector4(1, 0, 0, 0));
        rotx_270.SetRow(1, new Vector4(0, 0, 1, 0));
        rotx_270.SetRow(2, new Vector4(0, -1, 0, 0));
        rotx_270.SetRow(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 inv_z = Matrix4x4.identity;
        inv_z.SetRow(0, new Vector4(1, 0, 0, 0));
        inv_z.SetRow(1, new Vector4(0, 0, 1, 0));
        inv_z.SetRow(2, new Vector4(0, 1, 0, 0));
        inv_z.SetRow(3, new Vector4(0, 0, 0, 1));

        Matrix4x4 ros2gl = Matrix4x4.identity;
        inv_z.SetRow(0, new Vector4(0, -1, 0, 0));
        inv_z.SetRow(1, new Vector4(0, 0, 1, 0));
        inv_z.SetRow(2, new Vector4(1, 0, 0, 0));
        inv_z.SetRow(3, new Vector4(0, 0, 0, 1));
        
        //load first models        
        foreach (var gt in sceneGT)
        {
            if (!go_models.ContainsKey(gt.obj_id))
            {
                LoadModel(datasetParams.base_path, gt.obj_id, datasetParams.model_info);
            }
            else
            {
                var go_model = go_models[gt.obj_id];
                go_model.SetActive(true);
            }

            Matrix4x4 rt = Matrix4x4.identity;

            rt.m00 = gt.cam_R_m2c[0]; rt.m01 = gt.cam_R_m2c[1]; rt.m02 = gt.cam_R_m2c[2]; rt.m03 = gt.cam_t_m2c[0];
            rt.m10 = gt.cam_R_m2c[3]; rt.m11 = gt.cam_R_m2c[4]; rt.m12 = gt.cam_R_m2c[5]; rt.m13 = gt.cam_t_m2c[1];
            rt.m20 = gt.cam_R_m2c[6]; rt.m21 = gt.cam_R_m2c[7]; rt.m22 = gt.cam_R_m2c[8]; rt.m23 = gt.cam_t_m2c[2];

            rt = rt * rotx_270;
            rt = rt.inverse;

            //Matrix4x4 conversionMatrix = rt;
            //conversionMatrix.SetColumn(1, rt.GetColumn(2));
            //conversionMatrix.SetColumn(2, rt.GetColumn(1));

            //Matrix4x4 conversionMatrix2 = conversionMatrix;
            //conversionMatrix2.SetRow(1, conversionMatrix.GetRow(2));
            //conversionMatrix2.SetRow(2, conversionMatrix.GetRow(1));
            //conversionMatrix2 = conversionMatrix2.inverse;
            //rt = rt * rotx_270;
            //rt = inv_z * rt * inv_z;

            //rt = inv_z * rt * inv_z;
            //rt = inv_z * rt ;
            go_camera.transform.localPosition = rt.GetColumn(3) * 0.001f;
            go_camera.transform.localRotation = rt.rotation;
            //ApplyTransform(go_camera, rt, 0.01f);

            //rt = go_camera.transform.localToWorldMatrix;
            //rt = rt * conversionMatrix.inverse;
            //go_camera.transform.position = rt.GetColumn(3);
            //go_camera.transform.rotation = rt.rotation;
            //ApplyTransform(go_camera, rt);
            //var mat_env = GameObject.Find("env").transform.worldToLocalMatrix;
            //Debug.LogFormat("env: {0}", mat_env);

            //var newMat = mat_env * go_camera.transform.localToWorldMatrix;
            //go_camera.transform.position = newMat.GetColumn(3);
            //go_camera.transform.rotation = newMat.rotation;

            //Render RGB Image
            string rgb_path = BOPPath.get_rgb_path(datasetParams.split_path, datasetParams.scene_id, im_id, datasetParams.rgb_ext);
            var rgbTexture = TextureIO.LoadTexture(rgb_path);
            go_camera.GetComponent<BlendDuringRender>().SetBlendedTexture(rgbTexture);
            go_camera.GetComponent<BlendDuringRender>().SetTransparency(1);
            
            //Load PointCloud
            if(go_pointCloud != null)
                GameObject.DestroyImmediate(go_pointCloud);

            string depth_path = BOPPath.get_depth_path(datasetParams.split_path, datasetParams.scene_id, im_id, datasetParams.depth_ext);
            var depthTexture = TextureIO.LoadTexture(depth_path);
            
            var sceneCamera = datasetParams.scene_camera[im_id];
            float fx = sceneCamera.cam_K[0];
            float fy = sceneCamera.cam_K[4];
            float cx = sceneCamera.cam_K[2];
            float cy = sceneCamera.cam_K[5];

            PointCloudData pointCloud = PointCloudLoader.LoadFromDepthImage(depthTexture, rgbTexture, fx, fy, cx, cy);
            go_pointCloud = pointCloud.ToGameObject();
            go_pointCloud.transform.parent = go_camera.transform;
            go_pointCloud.transform.localPosition = Vector3.zero;
            go_pointCloud.transform.localRotation = Quaternion.identity;
            go_pointCloud.transform.localScale = Vector3.one;

        }
    }



    // set transfrom of go_camera using rt

    //model.transform.position = gt.cam_t;
    //model.transform.rotation = gt.cam_R;

    public void Destroy()
    {
        foreach (var model in go_models.Values)
        {
            GameObject.DestroyImmediate(model);
        }
        go_models.Clear();
        GameObject.DestroyImmediate(go_camera);
        GameObject.DestroyImmediate(frame_root);
        GameObject.DestroyImmediate(go_poses);
    }
    //GameObject CreteteObjects(GameObject frame, SceneGT sceneGT, SerializableDictionary<int, GameObject><int, ModelInfo> modelInfo)
    //{
    //    GameObject objects = new GameObject("Objects");
    //    objects.transform.parent = frame.transform;

    //}
}

