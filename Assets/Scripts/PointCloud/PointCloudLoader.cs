using System.IO;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Dynamic;

public static class PointCloudLoader
{
    
    static string compute_shader_path = "Assets/Scripts/PointCloud/Shaders/DepthImage2PointCloud.compute";
    enum DataType { __Float, __Double };

    static public GameObject LoadPly(string filePath)
    {
        // Load the .ply file
        string[] lines = File.ReadAllLines(filePath);

        string textureFileName = "";
        foreach (string line in lines)
        {
            if (line.StartsWith("comment TextureFile"))
            {
                textureFileName = line.Split(' ')[2];
                break;
            }
        }

        //// Create a new mesh
        //Mesh mesh = new Mesh();

        // Parse the .ply file
        int vertexCount = 0;
        int faceCount = 0;
        bool header = true;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        List<Vector2> uv = new List<Vector2>();

        foreach (string line in lines)
        {
            if (header)
            {
                if (line.StartsWith("element vertex"))
                {
                    vertexCount = int.Parse(line.Split(' ')[2]);
                }
                else if (line.StartsWith("element face"))
                {
                    faceCount = int.Parse(line.Split(' ')[2]);
                }
                else if (line.StartsWith("end_header"))
                {
                    header = false;
                }
            }
            else
            {
                string[] parts = line.Split(' ');
                if (vertexCount > 0)
                {
                    //vertices.Add(conversionMatrix.MultiplyPoint(vert));
                    vertices.Add(new Vector3(float.Parse(parts[0]), float.Parse(parts[2]), float.Parse(parts[1])) * 0.001f);
                    //vertices.Add(new Vector3(float.Parse(parts[0]), float.Parse(parts[2]), float.Parse(parts[1])) * 0.01f);

                    if (textureFileName == "")
                        colors.Add(new Color(float.Parse(parts[6]) / 255f, float.Parse(parts[7]) / 255f, float.Parse(parts[8]) / 255f, float.Parse(parts[9]) / 255f));
                    else
                        uv.Add(new Vector2(float.Parse(parts[6]), float.Parse(parts[7])));

                    vertexCount--;
                }
                else if (faceCount > 0)
                {
                    triangles.Add(int.Parse(parts[1]));
                    triangles.Add(int.Parse(parts[3]));
                    triangles.Add(int.Parse(parts[2]));
                    faceCount--;
                }
            }
        }
        string texturePath = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);

        PointCloudData pointCloudData = new PointCloudData(vertices.ToArray(), triangles.ToArray(), null, colors.ToArray(), uv.ToArray(), texturePath);
        return pointCloudData.ToGameObject();
    }

    static public PointCloudData LoadFromDepthImage(Texture2D depth_texture, float fx, float fy, float cx, float cy, float depth_scale)
    { 
        var pointcloud_vertices = ConvertToPoint(depth_texture, fx, fy, cx, cy, depth_scale);
        return new PointCloudData(pointcloud_vertices, null, null, null, null, null);
    }
    static public PointCloudData LoadFromDepthImage(Texture2D depth_texture, Texture2D color_texture, float fx, float fy, float cx, float cy, float depth_scale)
    {
        var pointcloud_vertices = ConvertToPoint(depth_texture, fx, fy, cx, cy, depth_scale);
        Color[] pointcloud_colors = color_texture.GetPixels();

        return new PointCloudData(pointcloud_vertices, null, null, pointcloud_colors, null, null);
    }

    static Vector3[] ConvertToPoint(Texture2D depth_texture, float fx, float fy, float cx, float cy, float depth_scale)
    {
        int resolutionX = depth_texture.width;
        int resolutionY = depth_texture.height;

        Vector3[] pointcloud_vertices = new Vector3[resolutionX * resolutionY];

        ComputeBuffer pointCloudBuffer = new ComputeBuffer(pointcloud_vertices.Length, 3 * sizeof(float));
#if UNITY_EDITOR
        ComputeShader computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(compute_shader_path);

        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernel, "_DepthTexture", depth_texture);
        computeShader.SetBuffer(kernel, "_PointCloud", pointCloudBuffer);
        computeShader.SetFloat("_fx", fx);
        computeShader.SetFloat("_fy", fx);
        computeShader.SetFloat("_cx", cx);
        computeShader.SetFloat("_cy", cy);
        computeShader.SetFloat("_cy", cy);
        computeShader.SetFloat("_depthScale", depth_scale * 0.001f);

        computeShader.Dispatch(kernel, resolutionX / 8, resolutionY / 8, 1);
        pointCloudBuffer.GetData(pointcloud_vertices);
        pointCloudBuffer.Release();

        return pointcloud_vertices;
#endif
    }

    public static PointCloudSubData LoadPointCloud(string filePath, int maximumVertex = 6000000, float fScale = 1.0f)
    {
        PointCloudSubData data = new PointCloudSubData();
        int levelOfDetails = 1;
        if (File.Exists(filePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int cursor = 0;
                int length = (int)reader.BaseStream.Length;
                string lineText = "";
                bool header = true;
                int vertexCount = 0;
                int colorDataCount = 3;
                int index = 0;
                int step = 0;
                int normalDataCount = 0;
                DataType dataType = DataType.__Float;

                //문제 1.
                //Data Type에 따른 파씽 처리가 제대로되어 있지 않다.
                //예를 들어 {x,y,z}, {nx, ny, nz}, 에 대한 type이 파일마다 다를 수 있는데,
                //현재 헤더에 명시된 property [type] x을 기준으로 x,y,z, nx, ny,nz의 read type을 결정하기 때문에
                //에러가 발생할 수 있다. 20201008 IbJeon.

                //문제 2.
                //PC의 좌표계 정립이 되어 있지 않다. 현재 x y,z를 읽을 때 z에 -1을 곱하도록 되어있다.(홀로렌즈 좌표계 기준인데 이것도 확실치않음.)
                while (cursor + step < length)
                {
                    if (header)
                    {
                        char v = reader.ReadChar();
                        if (v == '\n')
                        {
                            if (lineText.Contains("end_header"))
                            {
                                header = false;
                            }
                            else if (lineText.Contains("element vertex"))
                            {
                                string[] array = lineText.Split(' ');
                                if (array.Length > 0)
                                {
                                    int subtractor = array.Length - 2;
                                    vertexCount = Convert.ToInt32(array[array.Length - subtractor]);

                                    if (vertexCount > maximumVertex)
                                    {
                                        levelOfDetails = 1 + (int)Mathf.Floor(vertexCount / maximumVertex);
                                        vertexCount = maximumVertex;
                                    }
                                    data.vertices = new Vector3[vertexCount];
                                    data.normals = new Vector3[vertexCount];
                                    data.colors = new Color[vertexCount];
                                }
                            }
                            else if (lineText.Contains("property uchar alpha"))
                            {
                                colorDataCount = 4;
                            }
                            else if (lineText.Contains("property float n") || lineText.Contains("property double n"))
                            {
                                normalDataCount += 1;
                            }
                            else if (lineText.Contains("property double x"))
                            { //property double x 만을 보고 버텍스 및 노말 타입을 정하기 때문에 문제의 소지가 있다.
                                dataType = DataType.__Double;
                            }

                            lineText = "";
                        }
                        else
                        {
                            lineText += v;
                        }
                        step = sizeof(char);
                        cursor += step;
                    }
                    else
                    {
                        if (index < vertexCount)
                        {
                            if (dataType == DataType.__Float)
                            {
                                float px = -reader.ReadSingle();
                                float py = reader.ReadSingle();
                                float pz = reader.ReadSingle();

                                data.vertices[index] = new Vector3(px, py, pz) * fScale;
                                if (normalDataCount == 3)
                                {
                                    float nx = -reader.ReadSingle();
                                    float ny = reader.ReadSingle();
                                    float nz = reader.ReadSingle();

                                    data.normals[index] = new Vector3(nx, ny, nz);
                                }
                                else
                                {
                                    data.normals[index] = new Vector3(1f, 1f, 1f);
                                }
                                data.colors[index] = new Color(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f, 1f);

                                step = sizeof(float) * 6 * levelOfDetails + sizeof(byte) * colorDataCount * levelOfDetails;
                            }
                            else if (dataType == DataType.__Double)
                            {
                                double px = -reader.ReadDouble();
                                double py = reader.ReadDouble();
                                double pz = reader.ReadDouble();

                                data.vertices[index] = new Vector3((float)px, (float)py, (float)pz) * fScale;

                                if (normalDataCount == 3)
                                {
                                    double nx = -reader.ReadDouble();
                                    double ny = reader.ReadDouble();
                                    double nz = reader.ReadDouble();

                                    data.normals[index] = new Vector3((float)nx, (float)ny, (float)nz);
                                }
                                else
                                {
                                    data.normals[index] = new Vector3(1f, 1f, 1f);
                                }
                                data.colors[index] = new Color(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f, 1f);

                                step = sizeof(double) * 6 * levelOfDetails + sizeof(byte) * colorDataCount * levelOfDetails;
                            }

                            cursor += step;

                            //아래의 경우 무시한다.
                            if (colorDataCount > 3)
                            {
                                reader.ReadByte();
                            }

                            if (levelOfDetails > 1)
                            {
                                for (int l = 1; l < levelOfDetails; ++l)
                                {
                                    for (int f = 0; f < 3 + normalDataCount; ++f)
                                    {
                                        reader.ReadSingle();
                                    }
                                    for (int b = 0; b < colorDataCount; ++b)
                                    {
                                        reader.ReadByte();
                                    }
                                }
                            }

                            ++index;
                        }
                    }
                }
            }
        }
        return data;
    }
}

[System.Serializable]
public class PointCloudData
{
    const int verticesMax = 256 * 256;

    [SerializeField]
    public List<PointCloudSubData> sub_groups = new List<PointCloudSubData>();

    public PointCloudData(Vector3[] vertices, int[] triangles, Vector3[] normals, Color[] colors, Vector2[] uv, string texture_filepath)
    {
        //Assert.IsTrue(vertices.Length == normals.Length && normals.Length == colors.Length && colors.Length == uvs.Length);
        List<Vector3[]> sub_vertices;
        List<Vector3[]> sub_normals = null;
        List<Color[]> sub_colors = null;
        List<Vector2[]> sub_uv = null;

        if (vertices.Length > verticesMax)
        {
            if(triangles != null)
            {
                //vertex수가 max보다 많고, face정보까지 있는데 모델을 분할해야 하는 어려운 문제이므로 일단 에러를 던진다.
                throw new System.Exception("Too many vertices and triangles. Please reduce the number of vertices and triangles.");
            }
            
            sub_vertices = vertices.SplitIntoChunks(verticesMax).ToList();

            if (normals != null)
                sub_normals = normals.SplitIntoChunks(verticesMax).ToList();

            if (colors != null)
                sub_colors = colors.SplitIntoChunks(verticesMax).ToList();

            if (uv != null)
            {
                sub_uv = uv.SplitIntoChunks(verticesMax).ToList();
            }
        }
        else
        {
            sub_vertices = new List<Vector3[]>();
            sub_vertices.Add(vertices);
            if (normals != null)
            {
                sub_normals = new List<Vector3[]>();
                sub_normals.Add(normals);
            }
            if (colors != null)
            {
                sub_colors = new List<Color[]>();
                sub_colors.Add(colors);
            }
            if (uv != null)
            {
                sub_uv = new List<Vector2[]>();
                sub_uv.Add(uv);
            }
        }

        for (int i = 0; i < sub_vertices.Count; i++)
        {
            PointCloudSubData sub_data = new PointCloudSubData();
            sub_data.vertices = sub_vertices[i];
            if (normals != null && normals.Length > 0)
                sub_data.normals = sub_normals[i];
            if (colors != null && colors.Length > 0)
                sub_data.colors = sub_colors[i];
            if (uv != null && uv.Length > 0)
            {
                sub_data.uv = sub_uv[i];
                sub_data.texture_filepath = texture_filepath;
            }
            if (triangles != null && triangles.Length > 0)
                sub_data.triangles = triangles;

            sub_groups.Add(sub_data);
        }
    }

    public GameObject ToGameObject(float point_size = 0.01f)
    {
        if (sub_groups.Count > 1)
        {
            var root_obj = new GameObject("PointCloud");
            for (int i = 0; i < sub_groups.Count; i++)
            {
                var sub_obj = sub_groups[i].ToGameObject(i.ToString(), point_size);
                sub_obj.transform.parent = root_obj.transform;
            }
            return root_obj;
        }
        else
        {

            var go = sub_groups[0].ToGameObject("PointCloud", point_size);
            return go;
        }

    }
}

[System.Serializable]
public class PointCloudSubData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Color[] colors;
    public Vector2[] uv;
    public Bounds bounds;
    public string texture_filepath;

    public GameObject ToGameObject(string name, float point_size)
    {
        GameObject go = new GameObject(name);
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;

        Material material;

        if (colors != null)
            mesh.colors = colors;

        //face의 triangle 정보가 있는 경우
        if (triangles != null)
        {
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            material = new Material(Shader.Find("PointCloud/VertexColor"));
        }
        else
        {
            material = new Material(Shader.Find("PointCloud/UnlitParticle"));
            material.SetFloat("_Size", point_size);

            var indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; ++i)
                indices[i] = i;

            mesh.SetIndices(indices, MeshTopology.Points, 0);
        }
        //texture 정보가 있는 경우
        if (uv != null && uv.Length > 0 && texture_filepath != null)
        {
            mesh.uv = uv;
            material = new Material(Shader.Find("Standard"));
            material.mainTexture = LoadTexture(texture_filepath);
        }

        if (normals != null && normals.Length > 0)
            mesh.normals = normals;


        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
        go.AddComponent<MeshFilter>().mesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        return go;
    }
    static private Texture2D LoadTexture(string texturePath)
    {
        Texture2D texture = new Texture2D(1, 1);
        if (File.Exists(texturePath))
        {
            byte[] textureData = File.ReadAllBytes(texturePath);
            texture.LoadImage(textureData);
        }
        else
        {
            throw new Exception("Texture file not found: " + texturePath);
        }
        return texture;
    }
}

public static class ArrayExtensions
{
    public static IEnumerable<T[]> SplitIntoChunks<T>(this T[] source, int chunkSize)
    {
        for (int i = 0; i < source.Length; i += chunkSize)
        {
            T[] chunk;
            if (i + chunkSize > source.Length)
            {
                chunk = new T[source.Length - i];
                System.Array.Copy(source, i, chunk, 0, chunk.Length);
            }
            else
            {
                chunk = new T[chunkSize];
                System.Array.Copy(source, i, chunk, 0, chunkSize);
            }
            yield return chunk;
        }
    }
}
