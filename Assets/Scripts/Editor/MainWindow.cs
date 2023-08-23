using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class MainWindow : EditorWindow
{
    string scene_path;

    Vector2 scrollPos = Vector2.zero;
    public bool isPlaying = false;
    [SerializeField] 
    public BOPDatasetParams datasetParams;

    int curr_frame_id;
    [SerializeField]
    public BOPFrame bop_frame = new BOPFrame();

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
                datasetParams.load_scene();
                int curr_frame_id = datasetParams.scene_camera.Keys.First();
                bop_frame.CreateFrame(curr_frame_id, datasetParams);
                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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
                datasetParams = null;
                isPlaying = false;
                curr_frame_id = 0;
                bop_frame.Destroy();


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
        var new_frame_id = EditorGUILayout.IntSlider(curr_frame_id, 0, total_frame-1);

        if (new_frame_id != curr_frame_id)
        {
            curr_frame_id = new_frame_id;
            UpdateFrame();
        }

        EditorGUILayout.BeginHorizontal();
        //Debug.Log(frame_interval_tick);
        if ((Event.current.keyCode == KeyCode.A || GUILayout.Button("< (A)")) && curr_frame_id > 0)
        {
            curr_frame_id--;
            //Repaint();
            UpdateFrame();
        }

        if ((Event.current.keyCode == KeyCode.D || GUILayout.Button("(D) >")) && curr_frame_id < total_frame-1)
        {
            curr_frame_id++;
            //Repaint();
            UpdateFrame();
        }
        EditorGUILayout.EndHorizontal();
    }
   
    
    void UpdateFrame()
    {
        bop_frame.UpdateScene(curr_frame_id, datasetParams);
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
