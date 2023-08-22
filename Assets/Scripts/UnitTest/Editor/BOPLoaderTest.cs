using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PointCloudExporter;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.TestTools;
using static Codice.CM.Common.Serialization.PacketFileReader;

public class BOPLoaderTest
{
    string dataset_path = "Assets/Dataset";

    [SetUp]
    public void SetUp()
    {
    }

    // A Test behaves as an ordinary method
    [Test]
    public void load_camera_info_test()
    {
        string name = "lm";
        string split = "test";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var cameraData = dataset_params.load_camera_info();

        Assert.AreEqual(cameraData.cx, 325.2611f);
        Assert.AreEqual(cameraData.cy, 242.04899f);
        Assert.AreEqual(cameraData.depth_scale, 1.0f);
        Assert.AreEqual(cameraData.fx, 572.4114f);
        Assert.AreEqual(cameraData.fy, 573.57043f);
        Assert.AreEqual(cameraData.height, 480);
        Assert.AreEqual(cameraData.width, 640);
    }
    [Test]
    public void load_model_info_test()
    {
        string name = "lm";
        string split = "test";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.load_model_info();
        Assert.AreEqual(15, dataDict.Count);
        Assert.IsTrue(dataDict.ContainsKey(1));
        ModelInfo objectData = dataDict[1];
        Assert.AreEqual(objectData.diameter, 102.099f);
        Assert.AreEqual(objectData.min_x, -37.9343f);
        Assert.AreEqual(objectData.min_y, -38.7996f);
        Assert.AreEqual(objectData.min_z, -45.8845f);
        Assert.AreEqual(objectData.size_x, 75.8686f);
        Assert.AreEqual(objectData.size_y, 77.5992f);
        Assert.AreEqual(objectData.size_z, 91.769f);
    }
    
    [Test]
    public void load_scene_camera_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.load_scene_camera(scene_id);
        Assert.IsNotNull(dataDict);

        Assert.AreEqual(dataDict.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        SceneCamera cameraData = dataDict[1];
        Assert.AreEqual(9, cameraData.cam_K.Count);
        Assert.AreEqual(1.0f, cameraData.depth_scale);
    }
    [Test]
    public void load_scene_gt_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.load_scene_gt(scene_id);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        SceneGT object_gt = dataDict[1][0];
        Assert.AreEqual(object_gt.cam_R_m2c.Count, 9);
        Assert.AreEqual(object_gt.cam_t_m2c.Count, 3);
        Assert.AreEqual(object_gt.obj_id, 1);
    }
    [Test]
    public void load_scene_gt_info_test()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var dataDict = dataset_params.load_scene_gt_info(scene_id);
        Assert.IsNotNull(dataDict);
        Assert.AreEqual(dataDict.Keys.Count, 3);
        Assert.IsTrue(dataDict.ContainsKey(1));
        SceneGTInfo object_gt_info = dataDict[1][0];
        Assert.AreEqual(object_gt_info.bbox_obj.Count, 4);
        Assert.AreEqual(object_gt_info.bbox_visib.Count, 4);
        Assert.AreEqual(object_gt_info.px_count_all, 1598);

    }
    [Test]
    public void load_ply()
    {
        string name = "lm";
        string split = "test";
        string scene_id = "000001";
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var base_path = dataset_params.base_path;
        var model_path = BOPPath.get_model_path(base_path, 1);
        var go = PointCloudGenerator.LoadPly(model_path);
        
    }
    [Test]
    public void MatTest()
    {
        GameObject test = new GameObject();

        //test.transform.Rotate(-90, 0, 0);
        //test.transform.localScale = new Vector3(1, -1, 1);

        Matrix4x4 conversionMatrix = new Matrix4x4();
        conversionMatrix.SetRow(0, new Vector4(1, 0, 0, 0));
        conversionMatrix.SetRow(1, new Vector4(0, 0, 1, 0));
        conversionMatrix.SetRow(2, new Vector4(0, 1, 0, 0));
        conversionMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
        conversionMatrix = conversionMatrix * test.transform.localToWorldMatrix;

        ////Debug.Log(resultMatrix);

        //ApplyTransform(test, conversionMatrix);
        ////test.transform.Rotate(270, 0, 0);
        test.transform.position = conversionMatrix.GetColumn(3);
        test.transform.rotation = conversionMatrix.rotation;


        Debug.Log(test.transform.rotation.eulerAngles);
        Debug.Log(test.transform.localScale);
        Debug.Log(test.transform.worldToLocalMatrix);
        Debug.Log(test.transform.localToWorldMatrix);

    }

    [Test]
    public void MatTest2()
    {
        var cam1 = GameObject.Find("Camera1");
        var cam2 = GameObject.Find("Camera2");
      

        Debug.Log(cam1.transform.localToWorldMatrix);
        Debug.Log(cam2.transform.localToWorldMatrix);

        //Debug.Log(cam2.transform.localToWorldMatrix * cam1.transform.localToWorldMatrix.inverse);

        //var env = GameObject.Find("Frame");
        //Debug.Log(env.transform.localToWorldMatrix * cam1.transform.localToWorldMatrix);

        Matrix4x4 conversionMatrix = new Matrix4x4();
        conversionMatrix.SetRow(0, new Vector4(1, 0, 0, 0));
        conversionMatrix.SetRow(1, new Vector4(0, -1, 0, 0));
        conversionMatrix.SetRow(2, new Vector4(0, 0, -1, 0));
        conversionMatrix.SetRow(3, new Vector4(0, 0, 0, 1));
        conversionMatrix = cam1.transform.localToWorldMatrix * conversionMatrix;
        Debug.Log(conversionMatrix);

    }
    public void ApplyTransform(GameObject target, Matrix4x4 matrix)
    {
        // 위치 추출
        Vector3 position = matrix.GetColumn(3);  // 4th column

        // 스케일 추출
        Vector3 scale = new Vector3(
            matrix.GetColumn(0).magnitude,
            matrix.GetColumn(1).magnitude,
            matrix.GetColumn(2).magnitude
        );
        //Debug.Log(scale);

        // 회전 추출
        Vector3 forward = -matrix.GetColumn(1);
        Vector3 up = matrix.GetColumn(2);
        Quaternion rotation = Quaternion.LookRotation(forward, up);

        // Transform에 적용
        target.transform.localPosition = position;
        target.transform.localRotation = rotation;
        target.transform.localScale = scale;


    }
    [UnityTest]
    public IEnumerator DepthImage2PointCloudTest()
    {
        string name = "lm";
        string split = "test";
        int scene_id = 1;
        int im_id = 1;
        
        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var depth_path = BOPPath.get_depth_path(dataset_params.split_path, scene_id, im_id, dataset_params.depth_ext);

        var depthTexture = TextureIO.LoadTexture(depth_path);
        Assert.IsNotNull(depthTexture);
        Assert.AreEqual(depthTexture.width, 640);
        Assert.AreEqual(depthTexture.height, 480);
        //Assert.AreEqual(depthTexture.format, TextureFormat.R16);

        int resolutionX = depthTexture.width;
        int resolutionY = depthTexture.height;
        
        Vector3[] pointCloudData = new Vector3[resolutionX * resolutionY];
        ComputeBuffer pointCloudBuffer = new ComputeBuffer(pointCloudData.Length, 3 * sizeof(float));
#if UNITY_EDITOR
        ComputeShader computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Scripts/DepthImage2PointCloud.compute");
#endif
        // load compute shader in project folder.

        var cam_info = dataset_params.load_camera_info();
        
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernel, "_DepthTexture", depthTexture);
        computeShader.SetBuffer(kernel, "_PointCloud", pointCloudBuffer);
        computeShader.SetFloat("_fx", cam_info.fx);
        computeShader.SetFloat("_fy", cam_info.fx);
        computeShader.SetFloat("_cx", cam_info.cx);
        computeShader.SetFloat("_cy", cam_info.cy);

        computeShader.Dispatch(kernel, resolutionX / 8, resolutionY / 8, 1);

        pointCloudBuffer.GetData(pointCloudData);
        //foreach(var pointCloud in pointCloudData )
        //{
        //    if(pointCloud.x != 0)
        //    {
        //        int a = 3;
        //    }    
        //}
        pointCloudBuffer.Release();

        GameObject pointCloud = new GameObject();
        Mesh mesh = new Mesh();
        mesh.vertices = pointCloudData;
        mesh.SetIndices(System.Linq.Enumerable.Range(0, pointCloudData.Length).ToArray(), MeshTopology.Points, 0);

        var meshfilter = pointCloud.AddComponent<MeshFilter>();
        meshfilter.sharedMesh = mesh;

        var meshRenderer = pointCloud.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Custom/PointCloud"));
        for(int i = 0; i < 100000; i++)
        {
            yield return null;
        }

    }

}

