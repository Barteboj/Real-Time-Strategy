﻿using UnityEngine;
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
                instance = FindObjectOfType<Minimap>();
            }
            return instance;
        }
    }
    [SerializeField]
    private int size;
    public int MapSize { get; set; }
    [SerializeField]
    private CanvasScaler canvasScaler;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Update()
    {
        RaycastHit2D hitInfo = Physics2D.GetRayIntersection(new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward), Mathf.Infinity, 1 << LayerMask.NameToLayer("Minimap"));
        if (hitInfo.collider != null && Input.GetMouseButton(0))
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 positionRelativeToMinimap = mouseWorldPosition - (Vector2)gameObject.transform.position;

            float unitMinimapSize = size / canvasScaler.referencePixelsPerUnit * canvasScaler.matchWidthOrHeight;
            FindObjectOfType<GameCameraController>().SetCameraPosition(new Vector3((positionRelativeToMinimap * (MapSize / 5)).x - 0.5f, (positionRelativeToMinimap * (MapSize / 5)).y - 0.5f, Camera.main.transform.position.z));
        }
    }

    public Vector2 WorldToMinimapPosition(Vector2 worldPosition, int mapSize)
    {
        return worldPosition * ((float)size / mapSize);
    }

    public void SetMinimapElement(Image image, Vector2 positionInWorld, float width, float height, int mapSize)
    {
        image.transform.SetParent(gameObject.transform);
        image.rectTransform.sizeDelta = new Vector2((float)size / mapSize * width, (float)size / mapSize * height);
        image.rectTransform.anchoredPosition = WorldToMinimapPosition(positionInWorld, mapSize);
    }

    public void SetMinimapElementPosition(Image image, Vector2 positionInWorld, int mapSize)
    {
        image.rectTransform.anchoredPosition = WorldToMinimapPosition(positionInWorld, mapSize);
    }
}
