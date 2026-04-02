namespace GrowthOverlay;

internal class GrowthOverlayHider : ILoadableSingleton
{
    readonly EventBus _eventBus;
    readonly GrowthOverlayService _growthOverlay;
    GrowthOverlayToggle _growthOverlayToggle = null!;

    public GrowthOverlayHider(EventBus eventBus, GrowthOverlayService growthOverlay)
    {
        _eventBus = eventBus;
        _growthOverlay = growthOverlay;
    }

    public void Load()
    {
        _eventBus.Register(this);
        _growthOverlayToggle = _growthOverlay.GetGrowthOverlayToggle();
    }

    [OnEvent]
    public void OnUIVisibilityChanged(UIVisibilityChangedEvent uiVisibilityChangedEvent)
    {
        if (uiVisibilityChangedEvent.UIVisible)
            _growthOverlayToggle.ShowOverlay();
        else
            _growthOverlayToggle.HideOverlay();
    }
}
