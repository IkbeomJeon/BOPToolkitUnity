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
    string loaded_scene_name;
    string rgb_ext = "png";
    string gray_ext = "png";
    string depth_ext = "png";

    [SerializeField]
    public CameraInfo camera_info;
    [SerializeField] 
    public SerializableDictionary<int, ModelInfo> model_info;
    [SerializeField] 
    public SerializableDictionary<int, SceneCamera> scene_camera;
    [SerializeField] 
    public SerializableDictionary<int, List<SceneGT>> scene_gt;
    [SerializeField] 
    public SerializableDictionary<int, List<SceneGTInfo>> scene_gt_info;

    public BOPDatasetParams(string dataset_path, string dataset_name, string dataset_split)
    {
        base_path = Path.Combine(dataset_path, dataset_name);
        split_path = Path.Combine(base_path, dataset_split);
        
        if (!Directory.Exists(split_path))
            throw new DirectoryNotFoundException("The path not found: " + split_path);

        camera_info = load_camera_info();
        model_info = load_model_info();
    }
    public BOPDatasetParams(string scene_path)
    {
        loaded_scene_name = Path.GetFileNameWithoutExtension(scene_path);
        split_path = Directory.GetParent(scene_path).FullName;
        base_path = Directory.GetParent(split_path).FullName;
        
        if (!Directory.Exists(split_path))
            throw new DirectoryNotFoundException("The path not found: " + split_path);

        camera_info = load_camera_info();
        model_info = load_model_info();
        scene_camera = load_scene_camera();
        scene_gt = load_scene_gt();
        scene_gt_info = load_scene_gt_info();
    }
    bool is_scene_valid(int scene_id)
    {
        if (!File.Exists(get_scene_path(scene_id)) ||
            !File.Exists(get_scene_gt_path(scene_id)) ||
            !File.Exists(get_scene_gt_info_path(scene_id)))
            return false;
        
        else return true;
    }
    public CameraInfo load_camera_info()
    {
        string filepath = Path.Combine(base_path, "camera.json");
        string json = File.ReadAllText(filepath);

        CameraInfo cameraData = JsonUtility.FromJson<CameraInfo>(json);
        return cameraData;
    }

    public SerializableDictionary<int, ModelInfo> load_model_info()
    {
        string filepath = get_model_info_path();
        string json = File.ReadAllText(filepath);
        var dataDict = JsonConvert.DeserializeObject<SerializableDictionary<int, ModelInfo>>(json);
        return dataDict;
    }
    public List<string> get_scene_list()
    {
        //get directory names in split_path
        string[] scene_dirs = Directory.GetDirectories(split_path);
        var items = scene_dirs.Select(i => Path.GetFileName(i));
        return items.ToList();
    }
    public SerializableDictionary<int, SceneCamera> load_scene_camera(string scene_name=null)
    {
        if (scene_name == null)
            scene_name = loaded_scene_name;
        
        string filepath = Path.Combine(split_path, scene_name, "scene_camera.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<SerializableDictionary<int, SceneCamera>>(json);
        return dataDict;
    }
    public SerializableDictionary<int, List<SceneGT>> load_scene_gt(string scene_name=null)
    {
        if (scene_name == null)
            scene_name = loaded_scene_name;
        
        string filepath = Path.Combine(split_path, scene_name, "scene_gt.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<SerializableDictionary<int, List<SceneGT>>>(json);
        return dataDict;
    }
    public SerializableDictionary<int, List<SceneGTInfo>> load_scene_gt_info(string scene_name=null)
    {
        if (scene_name == null)
            scene_name = loaded_scene_name;
        
        string filepath = Path.Combine(split_path, scene_name, "scene_gt_info.json");
        string json = File.ReadAllText(filepath);

        var dataDict = JsonConvert.DeserializeObject<SerializableDictionary<int, List<SceneGTInfo>>>(json);
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
        return string.Format("{0}/{1}/obj_{1:D6}.ply", base_path, "models", obj_id);
    }
    string get_model_info_path()
    {
        return string.Format("{0}/{1}/{2}", base_path, "models", "models_info.json");
    }

}
