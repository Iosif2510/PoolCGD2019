using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor {
    public override void OnInspectorGUI() {
        MapGenerator map = target as MapGenerator;

        if (map.currentMode != MapGenerator.GameMode.Infinite) {
            if (DrawDefaultInspector()) map.GenerateMap(map.maps[map.mapIndex]);
            if (GUILayout.Button("Generate Map")) map.GenerateMap(map.maps[map.mapIndex]);
        }
        else {
            MapGenerator.Map newMap = new MapGenerator.Map();
            if (DrawDefaultInspector()) map.GenerateMap(newMap);
            if (GUILayout.Button("Generate Map")) map.GenerateMap(newMap);
        }
        
    }
}