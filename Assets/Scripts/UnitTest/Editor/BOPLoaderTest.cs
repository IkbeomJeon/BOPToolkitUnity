using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

public class BOPLoaderTest
{
    string dataset_path;
    [SetUp]
    public void SetUp()
    {
        // Create a test file
        dataset_path = "Assets/Dataset";
    }

    // A Test behaves as an ordinary method
    [Test]
    public void ParseCameraInfoTest()
    {
        string name = "lm";
        string filepath = Path.Combine(dataset_path, name, "camera.json");
        string json = File.ReadAllText(filepath);

        CameraInfo cameraData = JsonUtility.FromJson<CameraInfo>(json);

        Assert.AreEqual(cameraData.cx, 325.2611f);
        Assert.AreEqual(cameraData.cy, 242.04899f);
        Assert.AreEqual(cameraData.depth_scale, 1.0f);
        Assert.AreEqual(cameraData.fx, 572.4114f);
        Assert.AreEqual(cameraData.fy, 573.57043f);
        Assert.AreEqual(cameraData.height, 480);
        Assert.AreEqual(cameraData.width, 640);
    }
    [Test]
    public void ParseModelInfoJsonTest()
    {
        string name = "lm";
        string filepath = Path.Combine(dataset_path, name, "models", "models_info.json");
        string json = File.ReadAllText(filepath);
        Dictionary<string, ObjectData> dataDict = JsonConvert.DeserializeObject<Dictionary<string, ObjectData>>(json);

        Assert.AreEqual(15, dataDict.Count);
        Assert.IsTrue(dataDict.ContainsKey("1"));
        ObjectData objectData = dataDict["1"];
        Assert.AreEqual(objectData.diameter, 102.099f);
        Assert.AreEqual(objectData.min_x, -37.9343f);
        Assert.AreEqual(objectData.min_y, -38.7996f);
        Assert.AreEqual(objectData.min_z, -45.8845f);
        Assert.AreEqual(objectData.size_x, 75.8686f);
        Assert.AreEqual(objectData.size_y, 77.5992f);
        Assert.AreEqual(objectData.size_z, 91.769f);
    }
    
    [Test]
    public void ParseSceneCameraJsonTest()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        string filepath = Path.Combine(dataset_path, name, split, scene_id, "scene_camera.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<string, CameraData>>(json);
        Assert.IsNotNull(dataDict);

        Assert.AreEqual(dataDict.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey("1"));
        CameraData cameraData = dataDict["1"];
        Assert.AreEqual(9, cameraData.cam_K.Count);
        Assert.AreEqual(1.0f, cameraData.depth_scale);
    }
    [Test]
    public void ParseSceneGTJsonTest()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        string filepath = Path.Combine(dataset_path, name, split, scene_id, "scene_gt.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGT>>>(json);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGT object_gt = dataDict[1][0];
        Assert.AreEqual(object_gt.cam_R_m2c.Count, 9);
        Assert.AreEqual(object_gt.cam_t_m2c.Count, 3);
        Assert.AreEqual(object_gt.obj_id, 1);
    }
    [Test]
    public void ParseSceneGTInfoJsonTest()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        string filepath = Path.Combine(dataset_path, name, split, scene_id, "scene_gt_info.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGTInfo>>>(json);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGTInfo object_gt_info = dataDict[1][0];
        Assert.AreEqual(object_gt_info.bbox_obj.Count, 4);
        Assert.AreEqual(object_gt_info.bbox_visib.Count, 4);
        Assert.AreEqual(object_gt_info.px_count_all, 1598);

    }

    [Test]
    public void GetBOPPathTest()
    {
        string name = "lm";
        string split = "test";
        int scene_id = 1;
        int im_id = 1;
        int gt_id = 0;
        int obj_id = 1;

        BOPDatasetParams bopLoader = new BOPDatasetParams(dataset_path, name, split);
        Assert.IsTrue(Directory.Exists(bopLoader.get_split_path()));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_gt_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_gt_info_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_rgb_path(scene_id, im_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_depth_path(scene_id, im_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_mask_path(scene_id, im_id, gt_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_mask_visible_path(scene_id, im_id, gt_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_model_info_path()));
        Assert.IsTrue(File.Exists(bopLoader.get_model_path(obj_id)));

        name = "lmo";
        split = "test";
        scene_id = 2;
        im_id = 1;
        gt_id = 0;
        obj_id = 9;
        bopLoader = new BOPDatasetParams(dataset_path, name, split);
        Assert.IsTrue(Directory.Exists(bopLoader.get_split_path()));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_gt_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_scene_gt_info_path(scene_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_rgb_path(scene_id, im_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_depth_path(scene_id, im_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_mask_path(scene_id, im_id, gt_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_mask_visible_path(scene_id, im_id, gt_id)));
        Assert.IsTrue(File.Exists(bopLoader.get_model_info_path()));
        Assert.IsTrue(File.Exists(bopLoader.get_model_path(obj_id)));
    }
}

