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
    [SerializeField]
    SerializableDictionary<int, List<float>> test1 = new SerializableDictionary<int, List<float>>();
    [SerializeField]
    SerializableDictionary<int, float> test2 = new SerializableDictionary<int, float>();

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
        var rgb_path = BOPPath.get_rgb_path(dataset_params.split_path, scene_id, im_id, dataset_params.rgb_ext);
        var depth_path = BOPPath.get_depth_path(dataset_params.split_path, scene_id, im_id, dataset_params.depth_ext);

        var rgbTxeture = TextureIO.LoadTexture(rgb_path);
        var depthTexture = TextureIO.LoadTexture(depth_path);
        Assert.IsNotNull(depthTexture);
        Assert.AreEqual(depthTexture.width, 640);
        Assert.AreEqual(depthTexture.height, 480);
        //Assert.AreEqual(depthTexture.format, TextureFormat.R16);

        int resolutionX = depthTexture.width;
        int resolutionY = depthTexture.height;
        
        Vector3[] pointcloud_vertices = new Vector3[resolutionX * resolutionY];
        Color[] colorData = rgbTxeture.GetPixels();
        ComputeBuffer pointCloudBuffer = new ComputeBuffer(pointcloud_vertices.Length, 3 * sizeof(float));
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

        pointCloudBuffer.GetData(pointcloud_vertices);

        pointCloudBuffer.Release();

        //PointCloudData pointCloud = new PointCloudData(pointcloud_vertices, colorData, resolutionX, resolutionY);


        GameObject pointCloud = new GameObject();

        int vertexCount= pointcloud_vertices.Length;
        int verticesMax = 128*128;
        int meshCount = (int)Mathf.Ceil(vertexCount / (float)verticesMax);

        int meshIndex = 0;
        int vertexIndex = 0;
        int resolution = GetNearestPowerOfTwo(Mathf.Sqrt(vertexCount));
        while (meshIndex < meshCount)
        {
            int meshVertexCount = Mathf.Min(verticesMax, vertexCount - vertexIndex);
            Vector3[] vertices = new Vector3[meshVertexCount];
            Color[] color = new Color[meshVertexCount];
            int[] indices = new int[meshVertexCount];
            for (int i = 0; i < meshVertexCount; i++)
            {
                color[i] = colorData[vertexIndex];
                vertices[i] = pointcloud_vertices[vertexIndex];
                indices[i] = i;
                vertexIndex++;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.colors = color;
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            var material = new Material(Shader.Find("Unlit/PointCloud"));
            material.SetFloat("_Size", 0.001f);
            GameObject go = CreateGameObjectWithMesh(mesh, material, meshIndex.ToString(), pointCloud.transform);

            meshIndex++;
        }
        for(int i = 0; i < 10000; i++)
        {
            yield return null;
        }
      
    }
    static GameObject CreateGameObjectWithMesh(Mesh mesh, Material materialToApply, string name = "GeneratedMesh", Transform parent = null)
    {
        GameObject meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject.DestroyImmediate(meshGameObject.GetComponent<Collider>());
        meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
        meshGameObject.GetComponent<Renderer>().sharedMaterial = materialToApply;
        meshGameObject.name = name;
        meshGameObject.transform.parent = parent;
        meshGameObject.transform.localPosition = Vector3.zero;
        meshGameObject.transform.localRotation = Quaternion.identity;
        meshGameObject.transform.localScale = Vector3.one;
        return meshGameObject;
    }
    int GetNearestPowerOfTwo(float x)
    {
        return (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
    }
    [UnityTest]
    public IEnumerator PointCloudGenTest()
    {
        string name = "lm";
        string split = "test";
        int scene_id = 1;
        int im_id = 1;

        BOPDatasetParams dataset_params = new BOPDatasetParams(dataset_path, name, split);
        var rgb_path = BOPPath.get_rgb_path(dataset_params.split_path, scene_id, im_id, dataset_params.rgb_ext);
        var depth_path = BOPPath.get_depth_path(dataset_params.split_path, scene_id, im_id, dataset_params.depth_ext);

        var rgbTxeture = TextureIO.LoadTexture(rgb_path);
        var depthTexture = TextureIO.LoadTexture(depth_path);
        Assert.IsNotNull(depthTexture);
        Assert.AreEqual(depthTexture.width, 640);
        Assert.AreEqual(depthTexture.height, 480);
        //Assert.AreEqual(depthTexture.format, TextureFormat.R16);

        int resolutionX = depthTexture.width;
        int resolutionY = depthTexture.height;

        Vector3[] pointcloud_vertices = new Vector3[resolutionX * resolutionY];
        Color[] pointcloud_colors = rgbTxeture.GetPixels();

        ComputeBuffer pointCloudBuffer = new ComputeBuffer(pointcloud_vertices.Length, 3 * sizeof(float));
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
        pointCloudBuffer.GetData(pointcloud_vertices);
        pointCloudBuffer.Release();

        PointCloudData pointCloud = new PointCloudData(pointcloud_vertices, null, pointcloud_colors, null);
        Debug.LogFormat("{0}, {1}, {2}", pointCloud.sub_groups.Count, pointCloud.sub_groups[0].vertices.Length, pointCloud.sub_groups[0].colors.Length);
        pointCloud.ToGameObject();

        for (int i = 0; i < 10000; i++)
        {
            yield return null;
        }


    }
}

