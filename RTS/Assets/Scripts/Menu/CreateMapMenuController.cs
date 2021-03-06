﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateMapMenuController : MonoBehaviour
{
    [SerializeField]
    private Text mapNameText;
    [SerializeField]
    private Slider mapWidthSlider;

    public void CreateMap()
    {
        MapEditor.Instance.CreateMap(mapNameText.text, (int)mapWidthSlider.value, (int)mapWidthSlider.value);
    }
}
