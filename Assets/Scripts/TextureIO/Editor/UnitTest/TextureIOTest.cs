using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TextureIOTest
{
    string sample_dir = "Assets/Dataset/lm/test/000001";
    // A Test behaves as an ordinary method
    [Test]
    public void LoadRGBA32Image()
    {
        var filepath = sample_dir + "/rgb/000000.png";
        var texutre = TextureIO.LoadTexture(filepath);
        Assert.AreEqual(texutre.width, 640);
        Assert.AreEqual(texutre.height, 480);
        Assert.AreEqual(texutre.format, TextureFormat.RGB24);
        //TextureIO.WriteTexture(texutre, "result_rgb.png");
        //System.IO.FileInfo fi1 = new System.IO.FileInfo(filepath);
        ////get asset path
        //System.IO.FileInfo fi2 = new System.IO.FileInfo(Application.dataPath + "/result_rgb.png");
        //Assert.AreEqual(fi1.Length, fi2.Length);
    }
    [Test]
    public void LoadUSHORTDepthImage()
    {
        var filepath = sample_dir + "/depth/000000.png";
        var texutre = TextureIO.LoadTexture(filepath);
        Assert.AreEqual(texutre.width, 640);
        Assert.AreEqual(texutre.height, 480);
        Assert.AreEqual(texutre.format, TextureFormat.R16);
        //TextureIO.WriteTexture(texutre, "result_depth.png");

        ////print file size from filepath
        //System.IO.FileInfo fi1= new System.IO.FileInfo(filepath);
        ////get asset path
        //System.IO.FileInfo fi2= new System.IO.FileInfo(Application.dataPath+"/result_depth.png");
        //Assert.AreEqual(fi1.Length, fi2.Length);

    }

}
