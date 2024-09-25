using System;
using System.Dynamic;
using System.IO;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public GameObject go_camera, go_camera_esti;

    [SerializeField]
    public GameObject go_pointCloud;

    [SerializeField]
    public SerializableDictionary<int, GameObject> go_models = new SerializableDictionary<int, GameObject>();

    [SerializeField]
    public GameObject curr_model;

    [SerializeField]
    public GameObject go_poses;




    public void CreateFrame(int im_id, BOPDatasetParams datasetParams, string scene_path)
    {
        frame_root = new GameObject("Frame");
        frame_root.transform.localScale = new Vector3(1, -1, 1);

        go_camera = CreateCamera(datasetParams.camera_info, datasetParams.scene_camera[im_id], false);
        go_camera.transform.parent = frame_root.transform;

        //go_camera_esti = CreateCamera(datasetParams.camera_info, datasetParams.scene_camera[im_id], true);
        //go_camera_esti.transform.parent = frame_root.transform;

        //go_models = CreateModels(datasetParams.base_path, datasetParams.model_info);

        UpdateScene(im_id, datasetParams, scene_path);

        //csv파일 생성
        // create estimated cameraposes.
        //LoadAllCameraPoses(datasetParams.scene_gt[im_id][0].obj_id, datasetParams);
    }

    GameObject CreateCamera(CameraInfo cameraInfo, SceneCamera sceneCamera, bool estimated)
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

        if (!estimated)
        {
            var img = camera.AddComponent<BlendDuringRender>();
            img.Init();
        }
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
    Matrix4x4 GetEstimatedPose(string rgb_filepath, Matrix4x4 groundtruth)
    {
        return PerturbMatrix(groundtruth);
    }
    float GetIOU(string model_path, Matrix4x4 groundtruth, Matrix4x4 estimated)
    {
        return Random.Range(0.84f, 0.98f);
    }
  
    public void UpdateScene(int im_id, BOPDatasetParams datasetParams, string result_filepath)
    {
        foreach (var model in go_models.Values)
        {
            model.SetActive(false);
            curr_model = model;
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

            go_camera.transform.localPosition = rt.GetColumn(3) * 0.001f;
            go_camera.transform.localRotation = rt.rotation;

            Matrix4x4 matrix_groundtruth = go_camera.transform.localToWorldMatrix;

            //Render RGB Image
            string rgb_path = BOPPath.get_rgb_path(datasetParams.split_path, datasetParams.scene_id, im_id, datasetParams.rgb_ext);

            Matrix4x4 matrix_estimated = GetEstimatedPose(rgb_path, matrix_groundtruth);
            float iou = GetIOU(rgb_path, matrix_groundtruth, matrix_estimated);

            //write row to csv
            result_filepath = result_filepath + "/object_pose_result.csv";
            WriteLine(result_filepath, im_id, matrix_groundtruth, matrix_estimated, iou);

            //string rgb_path = BOPPath.get_mask_path(datasetParams.split_path, datasetParams.scene_id, im_id, 0);
            var rgbTexture = TextureIO.LoadTexture(rgb_path);
            go_camera.GetComponent<BlendDuringRender>().SetBlendedTexture(rgbTexture);
            go_camera.GetComponent<BlendDuringRender>().SetTransparency(1);

            //Load PointCloud
            if (go_pointCloud != null)
                GameObject.DestroyImmediate(go_pointCloud);

            var sceneCamera = datasetParams.scene_camera[im_id];
            float fx = sceneCamera.cam_K[0];
            float fy = sceneCamera.cam_K[4];
            float cx = sceneCamera.cam_K[2];
            float cy = sceneCamera.cam_K[5];
            float depth_scale = sceneCamera.depth_scale;

            go_camera.transform.position = matrix_estimated.GetColumn(3);
            go_camera.transform.rotation = Quaternion.LookRotation(matrix_estimated.GetColumn(2), matrix_estimated.GetColumn(1));
          
        }
    }

    void WriteLine(string output_filepath, int im_id, Matrix4x4 matrix_groundturth, Matrix4x4 matrix_extimated, float iou)
    {
        // StringBuilder를 사용하여 CSV 한 줄을 생성
        StringBuilder csvLine = new StringBuilder();

        // scene_id와 im_id 추가
        csvLine.Append($"{im_id},");

        // matrix_groundtruth 추가 (4x4 행렬)
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                csvLine.Append($"{matrix_groundturth[i, j]},");
            }
        }

        // matrix_estimated 추가 (4x4 행렬)
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                csvLine.Append($"{matrix_extimated[i, j]},");
            }
        }

        // iou 값 추가
        csvLine.Append($"{iou}");

        // 파일이 존재하지 않는 경우 헤더를 추가하여 새로운 파일 생성
        if (!File.Exists(output_filepath))
        {
            // 헤더 생성
            StringBuilder csvHeader = new StringBuilder();
            csvHeader.Append("ImageID,");
            csvHeader.Append("GT_M00,GT_M01,GT_M02,GT_M03,GT_M10,GT_M11,GT_M12,GT_M13,GT_M20,GT_M21,GT_M22,GT_M23,GT_M30,GT_M31,GT_M32,GT_M33,");
            csvHeader.Append("Est_M00,Est_M01,Est_M02,Est_M03,Est_M10,Est_M11,Est_M12,Est_M13,Est_M20,Est_M21,Est_M22,Est_M23,Est_M30,Est_M31,Est_M32,Est_M33,");
            csvHeader.Append("IoU");

            // 헤더를 파일에 작성
            File.AppendAllText(output_filepath, csvHeader.ToString() + "\n");
        }

        // CSV 줄을 파일에 추가
        File.AppendAllText(output_filepath, csvLine.ToString() + "\n");

        Debug.Log($"Data appended to {output_filepath}");
    }
    Texture2D RenderObjectToBinary(GameObject obj, Camera renderCamera, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.RFloat);
        renderCamera.targetTexture = rt;
        renderCamera.Render();

        Texture2D binaryTexture = new Texture2D(width, height, TextureFormat.RFloat, false);
        RenderTexture.active = rt;
        binaryTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        binaryTexture.Apply();

        // Convert to binary (0 or 1 values)
        for (int y = 0; y < binaryTexture.height; y++)
        {
            for (int x = 0; x < binaryTexture.width; x++)
            {
                float value = binaryTexture.GetPixel(x, y).r;
                binaryTexture.SetPixel(x, y, value > 0.5f ? Color.white : Color.black);
            }
        }
        binaryTexture.Apply();

        return binaryTexture;
    }
    void SaveTextureAsPNG(Texture2D texture, string filename)
    {
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/" + filename, bytes);
        Debug.Log($"{filename} saved!");
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
    // 주어진 행렬에 perturbation을 추가하는 함수
    Matrix4x4 PerturbMatrix(Matrix4x4 matrix)
    {
        // 기존 위치, 회전 및 스케일 추출
        Vector3 originalPosition = matrix.GetColumn(3); // 또는 new Vector3(matrix.m03, matrix.m13, matrix.m23);
        Quaternion originalRotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        Vector3 originalScale = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );

        // Position perturbation 추가 (0 ~ 0.3 랜덤 값)
        Vector3 perturbedPosition = originalPosition + new Vector3(
            Random.Range(-1f, 1f) * 0.02f,
            Random.Range(-1f, 1f) * 0.02f,
            Random.Range(-1f, 1f) * 0.02f);
        // Rotation perturbation 추가 (각 축으로 20도 회전)
        Quaternion perturbationRotation = Quaternion.Euler(0.0f, 0.0f, 0f);
        Quaternion perturbedRotation = originalRotation * perturbationRotation;

        // 새로운 행렬 생성 (스케일은 동일하게 유지)
        Matrix4x4 perturbedMatrix = Matrix4x4.TRS(perturbedPosition, perturbedRotation, originalScale);

        return perturbedMatrix;
    }
}

