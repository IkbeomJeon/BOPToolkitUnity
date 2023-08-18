using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MainWindow : EditorWindow
{
    string scene_path;

    Vector2 scrollPos = Vector2.zero;
    public bool isLoaded = false;
    public bool isPlaying = false;

    
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        if (GUILayout.Button("Select scene path"))
        {
            var path = EditorUtility.OpenFolderPanel("Select scene path", scene_path, "");

            if (!string.IsNullOrEmpty(path))
            {
                scene_path = path;
                SavePreperence();
            }

        }
        EditorGUILayout.LabelField(scene_path, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();

        if (!string.IsNullOrEmpty(scene_path))
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
        
    }
    void DestroyAll()
    {
        
    }

    public void LoadPreperence()
    {
        if (PlayerPrefs.HasKey("scene_path"))
        {
            scene_path = PlayerPrefs.GetString("scene_path");
            Debug.Log(scene_path);
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
