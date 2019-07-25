using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor {
    public override void OnInspectorGUI() {
        MapGenerator map = target as MapGenerator;

        if (map.transform.Find("Map Generator")) {
            DestroyImmediate(map.transform.Find("Map Generator").gameObject);
        }
        if (DrawDefaultInspector()) map.GenerateMap(map.maps[map.mapIndex]);
        if (GUILayout.Button("Generate Map")) map.GenerateMap(map.maps[map.mapIndex]);
        
    }
}