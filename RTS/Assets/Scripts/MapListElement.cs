using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapListElement : MonoBehaviour
{
    private string mapFilePath;

    public Text mapNameText;

    public string MapFilePath
    {
        get
        {
            return mapFilePath;
        }
        set
        {
            mapFilePath = value;
            string mapName = mapFilePath.Remove(0, mapFilePath.LastIndexOf("\\") + 1);
            mapName = mapName.Remove(mapName.LastIndexOf("."));
            mapNameText.text = mapName;
        }
    }

    public MapListElement(string mapFilePath)
    {
        this.MapFilePath = mapFilePath;
    }

    public void LoadMap()
    {
        MapEditor.Instance.LoadMap(mapFilePath);
        MapEditor.Instance.mapName = mapNameText.text;
        MapEditorMainMenuController.Instance.Hide();
    }
}
