using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;

public class NewTestScript
{
    public Camera renderCamera;
    public int resolution = 256;
    [Test]
    public void NewTestScriptSimplePasses()
    {
        renderCamera = Camera.main;
        // Create cylinder objects
        GameObject cylinder1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder1.transform.position = new Vector3(0, 0, 0);
        Texture2D binaryImage1 = RenderObjectToBinary(cylinder1);
        cylinder1.SetActive(false);

        GameObject cylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder2.transform.position = new Vector3(0.5f, 0.0f, 0);
        Texture2D binaryImage2 = RenderObjectToBinary(cylinder2);

        float iou = ComputeIOU(binaryImage1, binaryImage2 );

        // Save images
        SaveTextureAsPNG(binaryImage1, "BinaryImage1.png");
        SaveTextureAsPNG(binaryImage2, "BinaryImage2.png");

        // Cleanup
        //
        //Destroy(cylinder2);
    }


    // Converts a Texture2D to a RenderTexture
    RenderTexture ConvertToRenderTexture(Texture2D texture)
    {
        RenderTexture rt = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.RFloat);
        rt.enableRandomWrite = true;
        rt.Create();

        Graphics.Blit(texture, rt);

        return rt;
    }
    Texture2D RenderObjectToBinary(GameObject obj)
    {
        RenderTexture rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.RFloat);
        renderCamera.targetTexture = rt;
        renderCamera.Render();

        Texture2D binaryTexture = new Texture2D(resolution, resolution, TextureFormat.RFloat, false);
        RenderTexture.active = rt;
        binaryTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
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

    float ComputeIOU(Texture2D inputTexture1, Texture2D inputTexture2)
    {
        ComputeShader iouComputeShader = Resources.Load<ComputeShader>("IoUComputeShader");
        ComputeBuffer resultBuffer = new ComputeBuffer(2, sizeof(int));
        int[] resultArray = new int[2];

        // Initialize RenderTextures with binary input textures
        RenderTexture binaryTexture1 = ConvertToRenderTexture(inputTexture1);
        RenderTexture binaryTexture2 = ConvertToRenderTexture(inputTexture2);

        // Allocate result buffer

        resultBuffer.SetData(resultArray);

        // 커널 이름으로 인덱스 가져오기
        int kernelHandle = iouComputeShader.FindKernel("CSMain");

        // Set compute shader parameters
        iouComputeShader.SetTexture(kernelHandle, "_InputTexture1", binaryTexture1);
        iouComputeShader.SetTexture(kernelHandle, "_InputTexture2", binaryTexture2);
        iouComputeShader.SetBuffer(kernelHandle, "_Result", resultBuffer);
        iouComputeShader.SetInt("_TextureWidth", inputTexture1.width);
        iouComputeShader.SetInt("_TextureHeight", inputTexture1.height);

        // Execute compute shader
        int threadGroupsX = Mathf.CeilToInt(inputTexture1.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(inputTexture1.height / 8.0f);
        iouComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY, 1);

        // Get result from buffer
        resultBuffer.GetData(resultArray);

        // Calculate IoU
        float intersection = resultArray[0];
        float unionValue = resultArray[1];
        float iou = intersection / unionValue;

        Debug.Log($"Intersection: {intersection}, Union: {unionValue}, IoU: {iou}");

        // Clean up
        resultBuffer.Release();
        binaryTexture1.Release();
        binaryTexture2.Release();

        return iou;
    }

}
