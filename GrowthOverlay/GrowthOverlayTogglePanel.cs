namespace GrowthOverlay;

internal class GrowthOverlayTogglePanel : ILoadableSingleton
{
    static readonly string ToggleCssClass = "stockpile-overlay-toggle-panel";
    static readonly string ShowOverlayLocKey = "Inventory.StockpileOverlay.Show";
    static readonly string HideOverlayLocKey = "Inventory.StockpileOverlay.Hide";

    readonly GrowthOverlayService _growthOverlay;
    readonly VisualElementLoader _visualElementLoader;
    readonly UILayout _uiLayout;
    readonly ITooltipRegistrar _tooltipRegistrar;
    GrowthOverlayToggle _growthOverlayToggle = null!;
    Toggle _toggle = null!;
    bool _enabled;

    string TooltipLocKey => _enabled ? HideOverlayLocKey : ShowOverlayLocKey;

    public GrowthOverlayTogglePanel(
        GrowthOverlayService growthOverlay,
        VisualElementLoader visualElementLoader,
        UILayout uiLayout,
        ITooltipRegistrar tooltipRegistrar)
    {
        _growthOverlay = growthOverlay;
        _visualElementLoader = visualElementLoader;
        _uiLayout = uiLayout;
        _tooltipRegistrar = tooltipRegistrar;
    }

    public void Load()
    {
        var element = _visualElementLoader.LoadVisualElement("Common/SquareToggle");
        _tooltipRegistrar.RegisterLocalizable(element, () => TooltipLocKey);
        _toggle = element.Q<Toggle>("Toggle");
        _toggle.AddToClassList(ToggleCssClass);
        _toggle.RegisterCallback<ChangeEvent<bool>>(e => OnOverlayToggled(e.newValue));
        UpdateToggle();
        _uiLayout.AddTopRightButton(element, 3);
        _growthOverlayToggle = _growthOverlay.GetGrowthOverlayToggle();
    }

    void OnOverlayToggled(bool showOverlay)
    {
        if (showOverlay)
        {
            _growthOverlayToggle.EnableOverlay();
            _enabled = true;
        }
        else
        {
            _growthOverlayToggle.DisableOverlay();
            _enabled = false;
        }
        UpdateToggle();
    }

    void UpdateToggle() => _toggle?.SetValueWithoutNotify(_enabled);
}
