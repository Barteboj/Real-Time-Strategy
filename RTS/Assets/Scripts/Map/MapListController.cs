using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MapListController : MonoBehaviour
{
    [SerializeField]
    private List<MapListElement> maps;
    [SerializeField]
    private GameObject mapListElementPrefab;
    [SerializeField]
    private Transform gridTransform;

    void OnEnable()
    {
        LoadMapsToList();
    }

    public void LoadMapsToList()
    {
        foreach (MapListElement mapListElement in FindObjectsOfType<MapListElement>())
        {
            Destroy(mapListElement.gameObject);
        }
        string searchedFolder = Application.dataPath + "\\" + MapEditor.mapsFolderName;
        if (Directory.Exists(searchedFolder))
        {
            string[] mapsfilesPaths = Directory.GetFiles(searchedFolder, "*.map", SearchOption.TopDirectoryOnly);
            foreach (string mapFilePath in mapsfilesPaths)
            {
                GameObject instantiatedMapListElement = Instantiate(mapListElementPrefab, Vector2.zero, Quaternion.identity, gridTransform);
                instantiatedMapListElement.transform.localScale = Vector2.one;
                instantiatedMapListElement.GetComponent<MapListElement>().MapFilePath = mapFilePath;
                maps.Add(instantiatedMapListElement.GetComponent<MapListElement>());
            }
        }
    }
}
