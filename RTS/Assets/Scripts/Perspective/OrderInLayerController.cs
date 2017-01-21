using UnityEngine;
using System.Collections;

public class OrderInLayerController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool isStatic = false;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-gameObject.transform.position.y);
    }

    void Update()
    {
        if (!isStatic)
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-gameObject.transform.position.y);
        }
    }
}
