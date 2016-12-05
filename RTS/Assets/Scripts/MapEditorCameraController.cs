﻿using UnityEngine;
using System.Collections;

public class MapEditorCameraController : MonoBehaviour
{
    public float cameraMoveSpeed = 3;
    public MapEditor mapEditor;
    public int outsideMapBorderLength = 5;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        Vector2 clampedMousePosition = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height));
        if (clampedMousePosition.x == Screen.width && Camera.main.transform.position.x < mapEditor.map.GetLength(0) + outsideMapBorderLength)
        {
            Camera.main.transform.Translate(Vector2.right * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.x == 0 && Camera.main.transform.position.x > -outsideMapBorderLength)
        {
            Camera.main.transform.Translate(Vector2.left * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.y == Screen.height && Camera.main.transform.position.y < mapEditor.map.GetLength(1) + outsideMapBorderLength)
        {
            Camera.main.transform.Translate(Vector2.up * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.y == 0 && Camera.main.transform.position.y > -outsideMapBorderLength)
        {
            Camera.main.transform.Translate(Vector2.down * cameraMoveSpeed * Time.deltaTime);
        }
    }
}
