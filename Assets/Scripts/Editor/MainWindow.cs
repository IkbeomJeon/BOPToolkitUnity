using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MainWindow : EditorWindow
{
    string dataset_path;

    Vector2 scrollPos = Vector2.zero;
    public bool isLoaded = false;
    public bool isPlaying = false;

    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        if (GUILayout.Button("Select dataset path"))
        {
            var path = EditorUtility.OpenFolderPanel("Select dataset path", dataset_path, "");

            if (!string.IsNullOrEmpty(path))
            {
                dataset_path = path;
                SavePreperence();
            }

        }
        EditorGUILayout.LabelField(dataset_path, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        if (!string.IsNullOrEmpty(dataset_path))
        {
            EditorGUILayout.BeginHorizontal();
            var playButtonContent = EditorGUIUtility.IconContent("PlayButton On");
            var stopButtonContent = EditorGUIUtility.IconContent("d_PreMatQuad");
            var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            if (!isLoaded && GUILayout.Button(playButtonContent))
            {

                LoadAll();
                isLoaded = true;
                isPlaying = false;
            }

            else if (isLoaded && GUILayout.Button(stopButtonContent))
            {
                DestroyAll();
                isLoaded = false;
                isPlaying = false;
            }

            if (isPlaying)
            {
                if (GUILayout.Button(pauseButtonContent))
                {
                    //just stop
                    isPlaying = !isPlaying;
                }
            }
            else
            {
                var backup = GUI.backgroundColor;
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button(pauseButtonContent))
                {
                    //just resume
                    isPlaying = !isPlaying;
                }
                GUI.backgroundColor = backup;
            }
            EditorGUILayout.EndHorizontal();
        }


        EditorGUILayout.EndScrollView();
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
            dataset_path = PlayerPrefs.GetString("dataset_path");
            Debug.Log(dataset_path);
        }
    }

    public void SavePreperence()
    {
        PlayerPrefs.SetString("dataset_path", dataset_path);
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
