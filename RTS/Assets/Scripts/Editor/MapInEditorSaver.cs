using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MapInEditorSaver))]
public class MapInEditorSaver : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MapInEditorSaver myScript = (MapInEditorSaver)target;
        if (GUILayout.Button("Create objects"))
        {
            /*int mapSizeX;
            int mapSizeY;
            List<string> lines = new List<string>(new StreamReader(filePath).ReadToEnd().Split('\n'));
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                switch (words[0])
                {
                    case mapSizeFileKey:
                        mapSizeX = int.Parse(words[1]);
                        mapSizeY = int.Parse(words[2]);
                        map = new Tile[mapSizeX, mapSizeY];
                        break;
                    case tileKey:
                        SaveTileToMap(new IntVector2(int.Parse(words[1]), int.Parse(words[2])), (TileType)System.Enum.Parse(typeof(TileType), words[3]));
                        break;
                }
            }*/
        }
    }
}
