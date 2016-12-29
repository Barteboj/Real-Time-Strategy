using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Minimap : MonoBehaviour
{
    private static Minimap instance;

    public static Minimap Instance
    {
        get
        {
            if (instance == null)
            {
                Minimap newInstance = FindObjectOfType<Minimap>();
                if (newInstance != null)
                {
                    instance = newInstance;
                    return instance;
                }
                else
                {
                    Debug.LogError("There is not Minimap attached to scene and is tried to be obtained");
                    return null;
                }
            }
            else
            {
                return instance;
            }
        }
    }

    public int size;
    public int mapSize;
    public CanvasScaler canvasScaler;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("More than one instances of Minimap on scene");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void SetMapSize(int size)
    {
        mapSize = size;
    }

    void Update()
    {
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Minimap"));
        if (hitInfo.collider != null && Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 positionRelativeToMinimap = mouseWorldPosition - (Vector2)gameObject.transform.position;

            float unitMinimapSize = size / canvasScaler.referencePixelsPerUnit * canvasScaler.matchWidthOrHeight;
            Camera.main.transform.position = new Vector3((positionRelativeToMinimap * (mapSize / 5)).x - 0.5f, (positionRelativeToMinimap * (mapSize / 5)).y - 0.5f, Camera.main.transform.position.z);
        }
    }

    public Vector2 WorldToMinimapPosition(Vector2 worldPosition, int mapSize)
    {
        return worldPosition * ((float)size / mapSize);
    }

    public void SetMinimapElement(Image image, Vector2 positionInWorld, int width, int height, int mapSize)
    {
        image.transform.parent = gameObject.transform;
        image.rectTransform.sizeDelta = new Vector2((float)size / mapSize * width, (float)size / mapSize * height);
        image.rectTransform.anchoredPosition = WorldToMinimapPosition(positionInWorld, mapSize);
    }

    public void SetMinimapElementPosition(Image image, Vector2 positionInWorld, int mapSize)
    {
        image.rectTransform.anchoredPosition = WorldToMinimapPosition(positionInWorld, mapSize);
    }
}
