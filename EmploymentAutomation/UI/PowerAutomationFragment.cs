using System;
using EmploymentAutomation.Logic;
using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.Localization;
using TimberUi.CommonUi;
using UnityEngine;
using UnityEngine.UIElements;

namespace EmploymentAutomation.UI;

public class PowerAutomationFragment(
    ILoc loc,
    VisualElementInitializer initializer) : BaseEntityPanelFragment<PowerComponent>
{
    private Toggle toggle;
    private MinMaxSlider slider;

    protected override void InitializePanel()
    {
        toggle = panel.AddToggle(text: loc.T("Ximsa.EmploymentAutomation.PowerToggle"), onValueChanged: OnToggle);
        slider = panel.AddIntMinMaxSliderWithValueDisplay(
            label: "",
            value: new Vector2Int(10, 50),
            min: 0,
            max: 100,
            onChange: OnIngredientSliderChanged);
        panel.Initialize(initializer);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component == null) return;
        UpdateValues(component);
    }

    private void UpdateValues(IEmploymentBoundsProvider component)
    {
        panel.ToggleDisplayStyle(component.Available);
        toggle.ToggleDisplayStyle(component.Available);
        slider.ToggleDisplayStyle(component.Available);
        toggle.text = ToggleText(component.Fillrate);
        toggle.value = component.Active;
        slider.value = new Vector2(component.Low*100f, component.High*100f);
    }
    
    public override void UpdateFragment()
    {
        base.UpdateFragment();
        if (component == null) return;
        UpdateReadonlyValues(component);
    }

    private void UpdateReadonlyValues(IEmploymentBoundsProvider component)
    {
        toggle.text = ToggleText(component.Fillrate);
    }

    private string ToggleText(float fillrate) =>
        loc.T("Ximsa.EmploymentAutomation.PowerToggle") + (int)Math.Round(fillrate*100) + "%";

    private void OnIngredientSliderChanged(Vector2Int value)
    {
        if (component == null)
            return;
        component.Low = value.x / 100f;
        component.High = value.y / 100f;
    }

    private void OnToggle(bool toggleState)
    {
        if (component == null)
            return;
        component.Active = toggleState;
    }
}