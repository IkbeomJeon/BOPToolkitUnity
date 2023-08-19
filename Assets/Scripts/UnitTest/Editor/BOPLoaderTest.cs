using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.TestTools;

public class BOPLoaderTest
{
    string dataset_path = "Assets/Dataset";

    [SetUp]
    public void SetUp()
    {
    }

    // A Test behaves as an ordinary method
    [Test]
    public void ParseCameraInfoTest()
    {
        string name = "lm";
        string split = "test";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var cameraData = dataset_params.get_camera_info();

        Assert.AreEqual(cameraData.cx, 325.2611f);
        Assert.AreEqual(cameraData.cy, 242.04899f);
        Assert.AreEqual(cameraData.depth_scale, 1.0f);
        Assert.AreEqual(cameraData.fx, 572.4114f);
        Assert.AreEqual(cameraData.fy, 573.57043f);
        Assert.AreEqual(cameraData.height, 480);
        Assert.AreEqual(cameraData.width, 640);
    }
    [Test]
    public void get_model_info_test()
    {
        string name = "lm";
        string split = "test";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.get_model_info_list();
        Assert.AreEqual(15, dataDict.Count);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectData objectData = dataDict[1];
        Assert.AreEqual(objectData.diameter, 102.099f);
        Assert.AreEqual(objectData.min_x, -37.9343f);
        Assert.AreEqual(objectData.min_y, -38.7996f);
        Assert.AreEqual(objectData.min_z, -45.8845f);
        Assert.AreEqual(objectData.size_x, 75.8686f);
        Assert.AreEqual(objectData.size_y, 77.5992f);
        Assert.AreEqual(objectData.size_z, 91.769f);
    }
    
    [Test]
    public void get_scene_camera_list_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.get_scene_camera_list(scene_id);
        Assert.IsNotNull(dataDict);

        Assert.AreEqual(dataDict.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        CameraData cameraData = dataDict[1];
        Assert.AreEqual(9, cameraData.cam_K.Count);
        Assert.AreEqual(1.0f, cameraData.depth_scale);
    }
    [Test]
    public void get_scene_gt_list_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.get_scene_gt_list(scene_id);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGT object_gt = dataDict[1][0];
        Assert.AreEqual(object_gt.cam_R_m2c.Count, 9);
        Assert.AreEqual(object_gt.cam_t_m2c.Count, 3);
        Assert.AreEqual(object_gt.obj_id, 1);
    }
    [Test]
    public void get_scene_gt_info_list_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.get_scene_gt_info_list(scene_id);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGTInfo object_gt_info = dataDict[1][0];
        Assert.AreEqual(object_gt_info.bbox_obj.Count, 4);
        Assert.AreEqual(object_gt_info.bbox_visib.Count, 4);
        Assert.AreEqual(object_gt_info.px_count_all, 1598);

    }
}

