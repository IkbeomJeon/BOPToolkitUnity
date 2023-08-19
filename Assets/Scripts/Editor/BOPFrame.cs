using PointCloudExporter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BOPFrame
{
    [SerializeField]
    public GameObject frame_root;

    [SerializeField]
    public List<GameObject> models;
    
    public void CreateFrame(int im_id, BOPDatasetParams datasetParams)
    {
        frame_root = new GameObject("Frame");
        var cam_obj = CreateCamera(datasetParams.camera_info, datasetParams.scene_camera[im_id]);
        cam_obj.transform.parent = frame_root.transform;

        models = CreateModels(datasetParams.get_base_path(),  datasetParams.model_info);
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
        
        return camera;
    }
    List<GameObject> CreateModels(string base_path, SerializableDictionary<int, ModelInfo> model_info)
    {
        List<GameObject> models = new List<GameObject>();
        foreach (var model in model_info)
        {
            //var go_model = new GameObject(model.Key.ToString());
            string model_path = BOPPath.get_model_path(base_path, model.Key);
            //var mesh_info = PointCloudGenerator.LoadPointCloud(model_path);
            //var go_model = PointCloudGenerator.ToGameObject(mesh_info, model.Key.ToString(), 0.1f);
            var go_model = PointCloudGenerator.LoadPly(model_path);
            go_model.name = model.Key.ToString();
            go_model.transform.parent = frame_root.transform;
            models.Add(go_model);
        }

        return models;
    }
    
    public void Destroy()
    {
        GameObject.DestroyImmediate(frame_root);

    }
    //GameObject CreteteObjects(GameObject frame, SceneGT sceneGT, SerializableDictionary<int, ModelInfo> modelInfo)
    //{
    //    GameObject objects = new GameObject("Objects");
    //    objects.transform.parent = frame.transform;

    //}
}
