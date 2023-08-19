using PointCloudExporter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[Serializable]
public class BOPFrame
{
    [SerializeField]
    public GameObject frame_root;

    [SerializeField]
    public GameObject go_camera;

    [SerializeField]
    public SerializableDictionary<int, GameObject> go_models;
    
    public void CreateFrame(int im_id, BOPDatasetParams datasetParams)
    {
        frame_root = new GameObject("Frame");
        go_camera = CreateCamera(datasetParams.camera_info, datasetParams.scene_camera[im_id]);
        go_camera.transform.parent = frame_root.transform;

        go_models = CreateModels(datasetParams.base_path, datasetParams.model_info);
        UpdateModels(im_id, datasetParams);
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
    SerializableDictionary<int, GameObject> CreateModels(string base_path, SerializableDictionary<int, ModelInfo> model_info)
    {
        var output  = new SerializableDictionary<int, GameObject>();
        foreach (var model in model_info)
        {
            //var go_model = new GameObject(model.Key.ToString());
            string model_path = BOPPath.get_model_path(base_path, model.Key);
            //var mesh_info = PointCloudGenerator.LoadPointCloud(model_path);
            //var go_model = PointCloudGenerator.ToGameObject(mesh_info, model.Key.ToString(), 0.1f);
            var go_model = PointCloudGenerator.LoadPly(model_path);
            //go_model.transform.localScale = new Vector3(1, -1, 1);
            go_model.name = model.Key.ToString();
            go_model.transform.parent = frame_root.transform;
            output.Add(model.Key, go_model);
        }

        return output;
    }

    public void UpdateModels(int im_id, BOPDatasetParams datasetParams)
    {
        foreach (var model in go_models.Values)
        {
            model.SetActive(false);
        }
        var sceneGT = datasetParams.scene_gt[im_id];
        
        foreach (var gt in sceneGT)
        {
            if (go_models.ContainsKey(gt.obj_id))
            {
                var model = go_models[gt.obj_id];
                model.SetActive(true);
                
                Matrix4x4 rt = Matrix4x4.identity;
                rt.m00 = gt.cam_R_m2c[0]; rt.m01 = gt.cam_R_m2c[1]; rt.m02 = gt.cam_R_m2c[2];
                rt.m10 = gt.cam_R_m2c[3]; rt.m11 = gt.cam_R_m2c[4]; rt.m12 = gt.cam_R_m2c[5]; 
                rt.m20 = gt.cam_R_m2c[6]; rt.m21 = gt.cam_R_m2c[7]; rt.m22 = gt.cam_R_m2c[8];
                rt.m03 = gt.cam_t_m2c[0]; rt.m13 = gt.cam_t_m2c[1]; rt.m23 = gt.cam_t_m2c[2];
                
                rt = rt.inverse;
                //rt.m10 *= 1;
                //rt.m11 *= 1;
                //rt.m12 *= 1;


                Vector3 newPosition = rt.GetColumn(3) * 0.001f;
                Quaternion newRotation = Quaternion.LookRotation(rt.GetColumn(2), rt.GetColumn(1));
                //Vector3 newScale = new Vector3(rt.GetColumn(0).magnitude, rt.GetColumn(1).magnitude, rt.GetColumn(2).magnitude);

                go_camera.transform.localPosition = newPosition;
                go_camera.transform.localRotation = newRotation;
                //go_camera.transform.localScale = newScale;
                //go_camera.transform.Rotate(Vector3.forward, 180);
                go_camera.SetActive(true);

                string rgb_path = BOPPath.get_rgb_path(datasetParams.split_path, datasetParams.scene_id, im_id, datasetParams.rgb_ext);
                // Read the image file as a byte array
                byte[] imageData = File.ReadAllBytes(rgb_path);

                // Create a new Texture2D and load the image data
                Texture2D texture = new Texture2D(2,2);
                texture.LoadImage(imageData);
                //// Get the pixel data of the texture
                // Color[] pixels = texture.GetPixels();

                // // Flip the pixel data vertically
                // Color[] flippedPixels = new Color[pixels.Length];
                // int width = texture.width;
                // int height = texture.height;
                // for (int y = 0; y < height; y++)
                // {
                //     for (int x = 0; x < width; x++)
                //     {
                //         flippedPixels[y * width + x] = pixels[(height - y - 1) * width + x];
                //     }
                // }
                // // Set the flipped pixel data back to the texture
                // texture.SetPixels(flippedPixels);
                // texture.Apply();
                //go_camera.transform.localScale = new Vector3(-1, 1, 1);
                go_camera.GetComponent<BlendDuringRender>().SetBlendedTexture(texture);
                go_camera.GetComponent<BlendDuringRender>().SetTransparency(1);
            }
        }
    }

    // set transfrom of go_camera using rt

    //model.transform.position = gt.cam_t;
    //model.transform.rotation = gt.cam_R;
    
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
