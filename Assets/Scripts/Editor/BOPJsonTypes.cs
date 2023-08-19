using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraInfo
{
    public float cx;
    public float cy;
    public float depth_scale;
    public float fx;
    public float fy;
    public int height;
    public int width;
}
[System.Serializable]
public class SymmetryData
{
    public List<float> axis = new List<float>();
    public List<float> offset = new List<float>();
}

[System.Serializable]
public class ObjectData
{
    public float diameter;
    public float min_x;
    public float min_y;
    public float min_z;
    public float size_x;
    public float size_y;
    public float size_z;
    public List<SymmetryData> symmetries_continuous = new List<SymmetryData>();
    public List<List<float>> symmetries_discrete = new List<List<float>>();
}
[System.Serializable]
public class CameraData
{
    public List<float> cam_K;
    public float depth_scale;
}
[Serializable]
public class ObjectGT
{
    public List<float> cam_R_m2c;
    public List<float> cam_t_m2c;
    public int obj_id;
}


[Serializable]
public class ObjectGTInfo
{
    public List<int> bbox_obj;
    public List<int> bbox_visib;
    public int px_count_all;
    public int px_count_valid;
    public int px_count_visib;
    public float visib_fract;
}
