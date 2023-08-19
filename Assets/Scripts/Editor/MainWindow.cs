using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using System.IO;
using System;

public class MainWindow : EditorWindow
{
    string scene_path;

    Vector2 scrollPos = Vector2.zero;
    public bool isPlaying = false;
    [SerializeField] BOPDatasetParams datasetParams;

    int curr_frame_id;

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Scene path: " + scene_path, EditorStyles.boldLabel);



        if (datasetParams == null && GUILayout.Button("Select scene path"))
        {
            var path = EditorUtility.OpenFolderPanel("Select scene path", scene_path, "");

            if (!string.IsNullOrEmpty(path))
            {
                scene_path = path;
                SavePreperence();
            }
            try
            {
                datasetParams = new BOPDatasetParams(scene_path);
                EditorGUILayout.LabelField("Scene path: " + scene_path, EditorStyles.boldLabel);

                curr_frame_id = 0;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                datasetParams = null;
            }
        }
        EditorGUILayout.EndVertical();

        if (datasetParams != null)
        {
            EditorGUILayout.BeginHorizontal();
            var playButtonContent = EditorGUIUtility.IconContent("PlayButton On");
            var stopButtonContent = EditorGUIUtility.IconContent("d_PreMatQuad");
            var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");

            
            if (!isPlaying)
            {
                if (GUILayout.Button(playButtonContent))
                    isPlaying = true;
            }
            else
            {
                if (GUILayout.Button(pauseButtonContent))
                    isPlaying = !isPlaying;
            }
            if (GUILayout.Button(stopButtonContent))
            {
                DestroyAll();
                datasetParams = null;
                isPlaying = false;
            }
            EditorGUILayout.EndHorizontal();
            FrameSelectionMenu();

        }
        

        EditorGUILayout.EndScrollView();
    }
    void FrameSelectionMenu()
    {
        if (datasetParams == null)
            return;

        int total_frame = datasetParams.scene_camera.Count;
        EditorGUILayout.LabelField("Total frames", datasetParams.scene_camera.Count.ToString());
        var new_frame_id = EditorGUILayout.IntSlider(curr_frame_id, 0, total_frame);
    }
    void LoadAll()
    {

        // Check all the number of samples.

        //
    }
    void DestroyAll()
    {

    }

    public void LoadPreperence()
    {
        if (PlayerPrefs.HasKey("scene_path"))
        {
            scene_path = PlayerPrefs.GetString("scene_path");
        }
    }

    public void SavePreperence()
    {
        PlayerPrefs.SetString("scene_path", scene_path);
    }


    [MenuItem("BOPHelper/MainWindow")]
    public static MainWindow OpenWindow()
    {
        return Instance;
    }


    static MainWindow instance;

    public static MainWindow Instance
    {
        get
        {
            if (instance)
                return instance;
            else
            {
                instance = GetWindow<MainWindow>(false, "BOPHelper");
                instance.LoadPreperence();

                return instance;
            }
        }
    }
}
