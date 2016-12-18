using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateMapMenuController : MonoBehaviour
{
    public Text mapNameText;
    public Slider mapWidthSlider;
    public Slider mapHeightSlider;

    public void CreateMap()
    {
        MapEditor.Instance.CreateMap(mapNameText.text, (int)mapWidthSlider.value, (int)mapHeightSlider.value);
    }
}
