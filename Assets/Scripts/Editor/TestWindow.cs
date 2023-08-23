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
    public bool init = false;

    //[SerializeField]
    //public SerializableDictionary<int, List<float>> test1 = new SerializableDictionary<int, List<float>>();
    //public SerializableDictionary<int,float> test2 = new SerializableDictionary<int, float>();

    [SerializeField]
    public SerializableList<SerializableList<float>> test3 = new SerializableList<SerializableList<float>>();
    //[SerializeField]
    //public SerializableList<float> test4 = new List<float>();

    public void OnGUI()
    {
        if(!init)
        {
            //var gos = new List<float>();
            //gos.Add(123);
            //gos.Add(321);
            //test1.Add(1, gos);
            //init = true;

            //test2.Add(1, 123);
            //test2.Add(2, 321);
            //init = true;

            var gos = new SerializableList<float>();
            gos.Add(123);
            gos.Add(321);

            test3.Add(gos);
            init = true;

        }
        else
        {
            foreach(var gos in test3)
            {
                foreach(var go in gos)
                {
                    EditorGUILayout.LabelField(go.ToString());
                }
            }


        }
    }
    [MenuItem("BOPHelper/TestWindow")]
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
                instance = GetWindow<TestWindow>(false, "Test");


                return instance;
            }
        }
    }
}