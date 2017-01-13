using UnityEngine;
using System.Collections;

public class GameCameraController : MonoBehaviour
{
    public float cameraMoveSpeed = 3;

    private bool blockLeft = false;
    private bool blockRight = false;
    private bool blockUp = false;
    private bool blockDown = false;

    public BoxCollider2D selectionArea;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void SetCameraPosition(Vector3 CameraPositionToSet)
    {
        
        if (CameraPositionToSet.x > MapGridded.Instance.mapGrid.GetLength(1) - (selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x))
        {
            CameraPositionToSet = new Vector3(MapGridded.Instance.mapGrid.GetLength(1) - (selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x), CameraPositionToSet.y, CameraPositionToSet.z);
        }
        if (CameraPositionToSet.x < (selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x))
        {
            CameraPositionToSet = new Vector3((selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x), CameraPositionToSet.y, CameraPositionToSet.z);
        }
        if (CameraPositionToSet.y > MapGridded.Instance.mapGrid.GetLength(0) - (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y))
        {
            CameraPositionToSet = new Vector3(CameraPositionToSet.x, MapGridded.Instance.mapGrid.GetLength(0) - (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y), CameraPositionToSet.z);
        }
        if (CameraPositionToSet.y < (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y))
        {
            CameraPositionToSet = new Vector3(CameraPositionToSet.x, (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y), CameraPositionToSet.z);
        }
        blockDown = false;
        blockUp = false;
        blockLeft = false;
        blockRight = false;
        Camera.main.transform.position = CameraPositionToSet;
    }

    void Update()
    {
        if (selectionArea.transform.position.x > MapGridded.Instance.mapGrid.GetLength(1) - (selectionArea.size.x / 2) - 0.5f)
        {
            Camera.main.transform.position = new Vector3(MapGridded.Instance.mapGrid.GetLength(1) - (selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x), Camera.main.transform.position.y, Camera.main.transform.position.z);
            blockRight = true;
        }
        if (selectionArea.transform.position.x < (selectionArea.size.x / 2) - 0.5f)
        {
            Camera.main.transform.position = new Vector3((selectionArea.size.x / 2) - 0.5f - (selectionArea.transform.position.x - Camera.main.transform.position.x), Camera.main.transform.position.y, Camera.main.transform.position.z);
            blockLeft = true;
        }
        if (selectionArea.transform.position.y > MapGridded.Instance.mapGrid.GetLength(0) - (selectionArea.size.y / 2) - 0.5f)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, MapGridded.Instance.mapGrid.GetLength(0) - (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y), Camera.main.transform.position.z);
            blockUp = true;
        }
        if (selectionArea.transform.position.y < (selectionArea.size.y / 2) - 0.5f)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, (selectionArea.size.y / 2) - 0.5f - (selectionArea.transform.position.y - Camera.main.transform.position.y), Camera.main.transform.position.z);
            blockDown = true;
        }
        Vector2 clampedMousePosition = new Vector2(Mathf.Clamp(Input.mousePosition.x, 0, Screen.width), Mathf.Clamp(Input.mousePosition.y, 0, Screen.height));
        if (clampedMousePosition.x >= Screen.width - 1 && selectionArea.transform.position.x <= MapGridded.Instance.mapGrid.GetLength(1) - (selectionArea.size.x / 2) - 0.5f && !blockRight)
        {
            blockLeft = false;
            Camera.main.transform.Translate(Vector2.right * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.x == 0 && selectionArea.transform.position.x > (selectionArea.size.x / 2) - 0.5f && !blockLeft)
        {
            blockRight = false;
            Camera.main.transform.Translate(Vector2.left * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.y >= Screen.height - 1 && selectionArea.transform.position.y <= MapGridded.Instance.mapGrid.GetLength(0) - (selectionArea.size.y / 2) - 0.5f && !blockUp)
        {
            blockDown = false;
            Camera.main.transform.Translate(Vector2.up * cameraMoveSpeed * Time.deltaTime);
        }
        else if (clampedMousePosition.y == 0 && selectionArea.transform.position.y >= (selectionArea.size.y / 2) - 0.5f && !blockDown)
        {
            blockUp = false;
            Camera.main.transform.Translate(Vector2.down * cameraMoveSpeed * Time.deltaTime);
        }
    }
}
