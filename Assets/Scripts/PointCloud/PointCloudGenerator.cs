using System.IO;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using static PointCloudGenerator;
using UnityEngine.Assertions;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public static class PointCloudGenerator
{
    const int verticesMax = 128 * 128;

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

        // Create a new mesh
        Mesh mesh = new Mesh();

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

        // Set the mesh data
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();

        // Create a new GameObject and add the mesh
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>().mesh = mesh;

        if (textureFileName != "")
        {
            Material new_mat = new Material(Shader.Find("Standard"));
            string texturePath = Path.Combine(Path.GetDirectoryName(filePath), textureFileName);
            new_mat.mainTexture = LoadTexture(texturePath);
            obj.AddComponent<MeshRenderer>().sharedMaterial = new_mat;

        }

        else
            obj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Custom/VertexColor"));

        return obj;
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

    public static GameObject ToGameObject(this PointCloudSubData meshInfo, string name, float point_size)
    {
        GameObject pointCloudObj = new GameObject(name);
        Material ptMaterial = new Material(Shader.Find("Unlit/PointCloud"));
        ptMaterial.SetFloat("_Size", point_size);

        Texture2D sprite = (Texture2D)Resources.Load("Textures/Circle");
        //ptMaterial.mainTexture = sprite as Texture;
        ptMaterial.SetTexture("_MainTex", sprite);

        Generate(meshInfo, ptMaterial, MeshTopology.Points, pointCloudObj);

        return pointCloudObj;
    }

    private static void Generate(PointCloudSubData meshInfos, Material materialToApply, MeshTopology topology, GameObject parentObj)
    {
        int vertexCount = meshInfos.vertices.Length;
        int meshCount = (int)Mathf.Ceil(vertexCount / (float)verticesMax);
        int index = 0;
        int meshIndex = 0;
        int vertexIndex = 0;

        int resolution = GetNearestPowerOfTwo(Mathf.Sqrt(vertexCount));

        while (meshIndex < meshCount)
        {

            int count = verticesMax;
            if (vertexCount <= verticesMax)
            {
                count = vertexCount;
            }
            else if (vertexCount > verticesMax && meshCount == meshIndex + 1)
            {
                count = vertexCount % verticesMax;
            }

            Vector3[] subVertices = meshInfos.vertices.Skip(meshIndex * verticesMax).Take(count).ToArray();
            Vector3[] subNormals = meshInfos.normals.Skip(meshIndex * verticesMax).Take(count).ToArray();
            Color[] subColors = meshInfos.colors.Skip(meshIndex * verticesMax).Take(count).ToArray();
            int[] subIndices = new int[count];
            for (int i = 0; i < count; ++i)
            {
                subIndices[i] = i;
            }

            Mesh mesh = new Mesh();
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            mesh.vertices = subVertices;
            mesh.normals = subNormals;
            mesh.colors = subColors;
            mesh.SetIndices(subIndices, topology, 0);

            Vector2[] uvs2 = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < uvs2.Length; ++i)
            {
                float x = vertexIndex % resolution;
                float y = Mathf.Floor(vertexIndex / (float)resolution);
                uvs2[i] = new Vector2(x, y) / (float)resolution;
                ++vertexIndex;
            }
            mesh.uv2 = uvs2;

            GameObject go = CreateGameObjectWithMesh(mesh, materialToApply, parentObj.name + "_" + meshIndex, parentObj.transform);

            go.transform.parent = parentObj.transform;

            index += count;
            ++meshIndex;
        }
    }

    

    private static GameObject CreateGameObjectWithMesh(Mesh mesh, Material materialToApply, string name = "GeneratedMesh", Transform parent = null)
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
    /// <summary>
    /// refer. : http://stackoverflow.com/questions/466204/rounding-up-to-nearest-power-of-2
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    /// 
    static int GetNearestPowerOfTwo(float x)
    {
        return (int)Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
    }
}

[System.Serializable]
public class PointCloudData
{
    const int verticesMax = 128 * 128;
    public List<PointCloudSubData> sub_groups = new List<PointCloudSubData>();

    Material ptMaterial;

    public PointCloudData(Vector3[] vertices, Vector3[] normals, Color[] colors, Vector2[] uv)
    {

        //Assert.IsTrue(vertices.Length == normals.Length && normals.Length == colors.Length && colors.Length == uvs.Length);

        List<Vector3[]> sub_normals=null;
        List<Color[]> sub_colors=null;
        List<Vector2[]> sub_uvs=null;

        var sub_vertices = vertices.SplitIntoChunks(verticesMax).ToList();

        if (normals != null)
            sub_normals = normals.SplitIntoChunks(verticesMax).ToList();

        if(colors != null)
            sub_colors = colors.SplitIntoChunks(verticesMax).ToList();

        if(uv != null)
            sub_uvs = uv.SplitIntoChunks(verticesMax).ToList();

        for(int i =0; i<sub_vertices.Count; i++)
        {
            PointCloudSubData sub_data = new PointCloudSubData();
            sub_data.vertices = sub_vertices[i];
            if (sub_normals != null)
                sub_data.normals = sub_normals[i];
            if (colors != null)
                sub_data.colors = sub_colors[i];
            if (uv != null)
                sub_data.uv = sub_uvs[i];

            sub_groups.Add(sub_data);
        }
    }

    public GameObject ToGameObject(float point_size=0.01f)
    {
        var root_obj = new GameObject("PointCloud");
        for(int i=0; i<sub_groups.Count; i++)
        {

            ptMaterial = new Material(Shader.Find("Unlit/PointCloud"));
            Texture2D sprite = (Texture2D)Resources.Load("Textures/Circle");
            //ptMaterial.mainTexture = sprite as Texture;
            ptMaterial.SetTexture("_MainTex", sprite);

            var sub_obj = sub_groups[i].ToGameObject(i.ToString(), ptMaterial, MeshTopology.Points, point_size);
            sub_obj.transform.parent = root_obj.transform;
        }
        return root_obj;
    }
}
public class PointCloudSubData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public Color[] colors;
    public Vector2[] uv;
    public Bounds bounds;

    public GameObject ToGameObject(string name, Material materialToApply, MeshTopology topology, float point_size)
    {
        GameObject go = new GameObject(name);
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;

        if(colors != null)
            mesh.colors = colors;

        if (triangles!=null)
            mesh.triangles = triangles;
        else
        {
            var indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; ++i)
                indices[i] = i;

            mesh.SetIndices(indices, topology, 0);
        }

        if(normals != null)
            mesh.normals = normals;

        if(uv !=null)
            mesh.uv = uv;

        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100f);

        go.AddComponent<MeshFilter>().mesh = mesh;
        materialToApply.SetFloat("_Size", point_size);
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = materialToApply;

        return go;
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
