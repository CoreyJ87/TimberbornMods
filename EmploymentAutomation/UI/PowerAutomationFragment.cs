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
    private CollapsiblePanel collapsible = null!;
    private Toggle toggle = null!;
    private MinMaxSlider slider = null!;

    protected override void InitializePanel()
    {
        collapsible = new CollapsiblePanel()
            .SetTitle(loc.T("Ximsa.EmploymentAutomation.PowerToggle"));
        collapsible.SetExpandWithoutNotify(false);
        panel.Add(collapsible);

        toggle = collapsible.Container.AddToggle(
            text: loc.T("Ximsa.EmploymentAutomation.PowerToggle"),
            onValueChanged: OnToggle);
        slider = collapsible.Container.AddIntMinMaxSliderWithValueDisplay(
            label: "",
            value: new Vector2Int(10, 50),
            min: 0,
            max: 100,
            onChange: OnSliderChanged);
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
        collapsible.ToggleDisplayStyle(component.Available);
        toggle.text = ToggleText(component.Fillrate);
        toggle.value = component.Active;
        slider.value = new Vector2(component.Low * 100f, component.High * 100f);
        collapsible.SetTitle(HeaderText(component));
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
        collapsible.SetTitle(HeaderText(component));
    }

    private string HeaderText(IEmploymentBoundsProvider component) =>
        loc.T("Ximsa.EmploymentAutomation.PowerToggle")
        + " " + (component.Active ? "ON" : "OFF")
        + " (" + (int)Math.Round(component.Fillrate * 100) + "%)";

    private string ToggleText(float fillrate) =>
        loc.T("Ximsa.EmploymentAutomation.PowerToggle") + (int)Math.Round(fillrate * 100) + "%";

    private void OnSliderChanged(Vector2Int value)
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