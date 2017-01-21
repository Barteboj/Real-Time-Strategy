using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderValueViewController : MonoBehaviour
{
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Text text;

    void OnEnable()
    {
        ViewSliderValue();
    }

    public void ViewSliderValue()
    {
        text.text = slider.value.ToString();
    }
}
