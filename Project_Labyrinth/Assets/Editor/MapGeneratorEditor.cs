using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateMap))]
public class MapGeneratorEditor : Editor
{
    public  override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GenerateMap myTarget = (GenerateMap)target;

        if(GUILayout.Button("Generate Map"))
        {
            myTarget.CreateMap();
        }
        else if (GUILayout.Button("Clear Map"))
        {
            myTarget.ClearMap();
        }
        else if(GUILayout.Button("Enable Triangulation"))
        {
            myTarget.EnableTriangulation();
        }

    }
}
