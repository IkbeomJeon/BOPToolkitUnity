using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;
using System.IO;
using System;

public class TestWindow : EditorWindow
{

    [SerializeField]

    public void OnGUI()
    {

    }

    [MenuItem("BOPHelper/Calculate_IOU")]
    public static TestWindow OpenWindow()
    {
        return Instance;
    }


    static TestWindow instance;

    public static TestWindow Instance
    {
        get
        {
            if (instance)
                return instance;
            else
            {
                instance = GetWindow<TestWindow>(false, "Calculate_IOU");


                return instance;
            }
        }
    }
}