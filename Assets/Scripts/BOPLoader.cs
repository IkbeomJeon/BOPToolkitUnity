using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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



public class BOPDatasetParams
{
    string base_path;        
    string split_path;
    string split_type;
    string model_path;
    string model_into_path;
    string rgb_ext = "png";
    string gray_ext = "png";
    string depth_ext = "png";
    
    public BOPDatasetParams(string dataset_path, string dataset_name, string dataset_split)
    {
        base_path = Path.Combine(dataset_path, dataset_name);
        split_path = Path.Combine(base_path, dataset_split);
        model_path = Path.Combine(base_path, "models");
        //나중에 : dataset name과 split에 따라서 달라져야 하는것
            //확장자
            //split_type
    }
    public string get_split_path()
    {
        return split_path;
    }
    public string get_scene_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_camera.json");
    }
    public string get_scene_gt_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_gt.json");
    }
    public string get_scene_gt_info_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_gt_info.json");
    }
    public string get_rgb_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "rgb", im_id, rgb_ext);
    }
    public string get_depth_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "depth", im_id, depth_ext);
    }
    public string get_gray_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "gray", im_id, depth_ext);
    }
    public string get_mask_path(int scene_id, int im_id, int gt_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}_{4:D6}.png", split_path, scene_id, "mask", im_id, gt_id);
    }
    public string get_mask_visible_path(int scene_id, int im_id, int gt_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}_{4:D6}.png", split_path, scene_id, "mask_visib", im_id, gt_id);
    }

    public string get_model_path(int obj_id)
    {
        return string.Format("{0}/obj_{1:D6}.ply", model_path, obj_id);
    }
    public string get_model_info_path()
    {
        return string.Format("{0}/{1}", model_path, "models_info.json");
    }


}
