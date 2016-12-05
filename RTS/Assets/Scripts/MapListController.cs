using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MapListController : MonoBehaviour
{
    public List<MapListElement> maps;
    public GameObject mapListElementPrefab;
    public Transform gridTransform;

    void OnEnable()
    {
        LoadMapsToList();
    }

    public void LoadMapsToList()
    {
        string searchedFolder = Application.dataPath + "\\" + MapEditor.mapsFolderName;
        string[] mapsfilesPaths = Directory.GetFiles(searchedFolder, "*.map", SearchOption.TopDirectoryOnly);
        foreach (string mapFilePath in mapsfilesPaths)
        {
            GameObject instantiatedMapListElement = (GameObject)Instantiate(mapListElementPrefab, Vector2.zero, Quaternion.identity, gridTransform);
            instantiatedMapListElement.transform.localScale = Vector2.one;
            instantiatedMapListElement.GetComponent<MapListElement>().MapFilePath = mapFilePath;
            maps.Add(instantiatedMapListElement.GetComponent<MapListElement>());
        }
    }
}
