using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EmploymentAutomation.UI;

public static class VisualElementExtensions
{
    public static MinMaxSlider AddIntMinMaxSliderWithValueDisplay(
        this VisualElement element,
        string label,
        Vector2Int value,
        int min,
        int max,
        Action<Vector2Int> onChange)
    {
        var slider = element.AddMinMaxSlider(
            label: label,
            values: new MinMaxSliderValues(new Vector2(value.x, value.y), min, max));
        var valueLabel = slider.AddGameLabel(min.ToString());
        slider.RegisterValueChangedCallback((e) =>
        {
            var newValue = new Vector2Int((int)Math.Round(e.newValue.x), (int)Math.Round(e.newValue.y));
            valueLabel.text = newValue.x + "% - " + newValue.y + "%";
            onChange(newValue);
        });
        return slider;
    }
}