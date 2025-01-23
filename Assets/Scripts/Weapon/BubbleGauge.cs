using UnityEngine;
using UnityEngine.UI;

public class BubbleGauge : MonoBehaviour
{
    public Slider slider;

    public void SetMaxGauge(float gauge)
    {
        slider.maxValue = gauge;
        slider.value = gauge;
    }

    public void SetGauge(float gauge)
    {
        slider.value = gauge;
    }
}
