using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BOPDatasetParams
{
    string base_path;
    string split_path;
    string scene_name;
    string model_path;
    string model_into_path;
    string rgb_ext = "png";
    string gray_ext = "png";
    string depth_ext = "png";

    public BOPDatasetParams(string dataset_path, string dataset_name, string dataset_split)
    {
        base_path = Path.Combine(dataset_path, dataset_name);
        split_path = Path.Combine(base_path, dataset_split);
        if (!Directory.Exists(split_path))
            throw new DirectoryNotFoundException("The path not found: " + split_path);
        model_path = Path.Combine(base_path, "models");
    }
    public BOPDatasetParams(string scene_path)
    {
        scene_name = Path.GetFileNameWithoutExtension(scene_path);
        split_path = Directory.GetParent(scene_path).FullName;
        base_path = Directory.GetParent(split_path).FullName;
        model_path = Path.Combine(base_path, "models");
    }
    public string get_scene_name()
    {
        return scene_name;
    }
    bool is_scene_valid(int scene_id)
    {
        if (!File.Exists(get_scene_path(scene_id)) ||
            !File.Exists(get_scene_gt_path(scene_id)) ||
            !File.Exists(get_scene_gt_info_path(scene_id)))
            return false;
        
        else return true;
    }
    public CameraInfo get_camera_info()
    {
        string filepath = Path.Combine(base_path, "camera.json");
        string json = File.ReadAllText(filepath);

        CameraInfo cameraData = JsonUtility.FromJson<CameraInfo>(json);
        return cameraData;
    }

    public Dictionary<int, ObjectData> get_model_info_list()
    {
        string filepath = Path.Combine(base_path, "models", "models_info.json");
        string json = File.ReadAllText(filepath);
        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, ObjectData>>(json);
        return dataDict;
    }
    public List<string> get_scene_list()
    {
        //get directory names in split_path
        string[] scene_dirs = Directory.GetDirectories(split_path);
        var items = scene_dirs.Select(i => Path.GetFileName(i));
        return items.ToList();
    }
    public Dictionary<int, CameraData> get_scene_camera_list(string scene_name)
    {
        string filepath = Path.Combine(split_path, scene_name, "scene_camera.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, CameraData>>(json);
        return dataDict;
    }
    public Dictionary<int, List<ObjectGT>> get_scene_gt_list(string scene_name)
    {
        string filepath = Path.Combine(split_path, scene_name, "scene_gt.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGT>>>(json);
        return dataDict;
    }
    public Dictionary<int, List<ObjectGTInfo>> get_scene_gt_info_list(string scene_name)
    {
        string filepath = Path.Combine(split_path, scene_name, "scene_gt_info.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<Dictionary<int, List<ObjectGTInfo>>>(json);
        return dataDict;
    }

    string get_scene_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_camera.json");
    }
    string get_scene_gt_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_gt.json");
    }
    string get_scene_gt_info_path(int scene_id)
    {
        return string.Format("{0}/{1:D6}/{2}", split_path, scene_id, "scene_gt_info.json");
    }
    string get_rgb_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "rgb", im_id, rgb_ext);
    }
    string get_depth_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "depth", im_id, depth_ext);
    }
    string get_gray_path(int scene_id, int im_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}.{4}", split_path, scene_id, "gray", im_id, depth_ext);
    }
    string get_mask_path(int scene_id, int im_id, int gt_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}_{4:D6}.png", split_path, scene_id, "mask", im_id, gt_id);
    }
    string get_mask_visible_path(int scene_id, int im_id, int gt_id)
    {
        return string.Format("{0}/{1:D6}/{2}/{3:D6}_{4:D6}.png", split_path, scene_id, "mask_visib", im_id, gt_id);
    }

    string get_model_path(int obj_id)
    {
        return string.Format("{0}/obj_{1:D6}.ply", model_path, obj_id);
    }
    string get_model_info_path()
    {
        return string.Format("{0}/{1}", model_path, "models_info.json");
    }

}
