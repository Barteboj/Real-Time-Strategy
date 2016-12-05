using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class ActionButton : MonoBehaviour
{
    public Sprite ButtonImage;

    public UnityEvent OnClick;

    void Awake()
    {
        OnClick.AddListener(Awake);
        OnClick.RemoveAllListeners();
    }
}
