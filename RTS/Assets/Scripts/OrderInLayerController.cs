using UnityEngine;
using System.Collections;

public class OrderInLayerController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-gameObject.transform.position.y);
    }
}
