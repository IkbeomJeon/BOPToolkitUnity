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
    // A Test behaves as an ordinary method
    [Test]
    public void ParseCameraInfoTest()
    {
        string json = "{\"cx\": 325.2611, \"cy\": 242.04899, \"depth_scale\": 1.0, \"fx\": 572.4114, \"fy\": 573.57043, \"height\": 480, \"width\": 640}";
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
        // Load jsonfile from 'assets/test.json'
        string json = File.ReadAllText("Assets\\Scripts\\UnitTest\\Data\\models_info.json");
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
        string json = File.ReadAllText("Assets\\Scripts\\UnitTest\\Data\\scene_camera.json");

        var dataDict = JsonConvert.DeserializeObject<Dictionary<string, CameraData>>(json);
        Assert.IsNotNull(dataDict);

        Assert.AreEqual(dataDict.Count, 1236);
        Assert.IsTrue(dataDict.ContainsKey("1"));
        CameraData cameraData = dataDict["1"];
        Assert.AreEqual(9, cameraData.cam_K.Count);
        Assert.AreEqual(1.0f, cameraData.depth_scale);
    }
    [Test]
    public void ParseSceneGTJsonTest()
    {
        string json = File.ReadAllText("Assets\\Scripts\\UnitTest\\Data\\scene_gt.json");

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGT>>>(json);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 1236);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGT object_gt = dataDict[1][0];
        Assert.AreEqual(object_gt.cam_R_m2c.Count, 9);
        Assert.AreEqual(object_gt.cam_t_m2c.Count, 3);
        Assert.AreEqual(object_gt.obj_id, 1);
    }
    [Test]
    public void ParseSceneGTInfoJsonTest()
    {
        string json = File.ReadAllText("Assets\\Scripts\\UnitTest\\Data\\scene_gt_info.json");

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGTInfo>>>(json);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 1236);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ObjectGTInfo object_gt_info = dataDict[1][0];
        Assert.AreEqual(object_gt_info.bbox_obj.Count, 4);
        Assert.AreEqual(object_gt_info.bbox_visib.Count, 4);
        Assert.AreEqual(object_gt_info.px_count_all, 1598);

    }
}

