using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapListElement : MonoBehaviour
{
    private string mapFilePath;
    [SerializeField]
    private Text mapNameText;

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

    public void LoadMap()
    {
        MapEditor.Instance.LoadMap(mapFilePath);
        MapEditor.Instance.MapName = mapNameText.text;
        MapEditorMainMenuController.Instance.Hide();
        FindObjectOfType<MapEditorCameraController>().ResetCameraPosition();
    }

    public void SelectMapInLobby()
    {
        FindObjectOfType<LobbyMenu>().LobbyMenuController.MapName = mapNameText.text;
    }
}
