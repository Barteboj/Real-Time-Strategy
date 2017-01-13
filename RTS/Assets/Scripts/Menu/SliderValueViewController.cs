using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderValueViewController : MonoBehaviour
{
    public Slider slider;
    public Text text;

    void OnEnable()
    {
        ViewSliderValue();
    }

    public void ViewSliderValue()
    {
        text.text = slider.value.ToString();
    }
}
