using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MainWindow : EditorWindow
{
    string dataset_path;

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

                return instance;
            }
        }
    }
}
