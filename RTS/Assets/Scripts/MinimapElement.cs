using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MinimapElement : MonoBehaviour
{
    public int width;
    public int height;
    public Image image;
    public bool isStatic = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Map Editor")
        {
            Destroy(this);
        }
        else
        {
            image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x * width, image.rectTransform.sizeDelta.y * height);
            Minimap.Instance.SetMinimapElement(image, gameObject.transform.position, width, height, MapGridded.Instance.mapGrid.GetLength(0));
            if (isStatic)
            {
                enabled = false;
            }
        }
    }

    void Update()
    {
        Minimap.Instance.SetMinimapElementPosition(image, gameObject.transform.position, MapGridded.Instance.mapGrid.GetLength(0));
    }

    void OnDestroy()
    {
        if (image != null)
        {
            Destroy(image.gameObject);
        }
    }

    public void Hide()
    {
        image.enabled = false;
    }

    public void Show()
    {
        image.enabled = true;
    }
}
