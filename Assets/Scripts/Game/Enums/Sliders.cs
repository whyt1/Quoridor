using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Sliders
{
    Error,
    Music,
    SFX, 
    NumberOfPlayers, 
    TurnTimer, 
    BarriersCount, 
    Players, 
    Password,
};

public static class SlidersExtensions  {
    
    /// <summary>
    /// Gets the sliders current value as float. on error returns -1.
    /// </summary>
    public static float GetSliderValue(this Sliders slider)
    {
        if (!SC_GameData.Instance.TryGetSlider(slider, out GameObject gameObject)) 
        {    
            Debug.LogError($"Failed to get slider value for {slider} !"); 
            return -1;
        }
        Slider _slider = gameObject.GetComponent<Slider>();
        if (_slider == null) 
        { 
            Debug.LogError($"Failed to get slider value for {slider} !"); 
            return -1; 
        }

        return _slider.value;
    }

    /// <summary>
    /// Gets the sliders text component as TextMeshProUGUI. on error returns null.
    /// </summary>
    public static TextMeshProUGUI GetSliderText(this Sliders slider)
    {
        if (!SC_GameData.Instance.TryGetSlider(slider, out GameObject gameObject)) 
        { 
            Debug.LogError($"Failed to get slider text for {slider} !"); 
            return null; 
        }
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        return text;
    }

    public static void ToggleSlider(this Sliders _slider, bool active)
    {
        if (SC_GameData.Instance.TryGetSlider(_slider, out GameObject slider))
        {
            slider.SetActive(active);
        }
    }

    public static void ToggleSlider(this Sliders _slider)
    {
        if (SC_GameData.Instance.TryGetSlider(_slider, out GameObject slider))
        {
            slider.SetActive(!slider.activeSelf);
        }
    }

    public static void ToggleSliderInteractive(this Sliders sliderName, bool active)
    {
        if (!SC_GameData.Instance.TryGetSlider(sliderName, out GameObject slider))
        {
            return; // log error
        }
        Slider _slider = slider.GetComponent<Slider>();
        if (_slider == null) 
        { 
            Debug.LogError($"Failed to get slider value for {sliderName} !"); 
            return; 
        }
        _slider.interactable = active;
    }
}