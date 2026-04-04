namespace GrowthOverlay;

internal class GrowthOverlayShower : ILoadableSingleton, IInputProcessor
{
    readonly GrowthOverlayService _growthOverlay;
    readonly InputService _inputService;
    GrowthOverlayToggle _growthOverlayToggle = null!;
    bool _isShown;

    public GrowthOverlayShower(GrowthOverlayService growthOverlay, InputService inputService)
    {
        _growthOverlay = growthOverlay;
        _inputService = inputService;
    }

    public void Load()
    {
        _growthOverlayToggle = _growthOverlay.GetGrowthOverlayToggle();
        _inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (_inputService.IsKeyHeld("ShowStockpileOverlay"))
        {
            if (!_isShown)
            {
                _isShown = true;
                _growthOverlayToggle.EnableOverlay();
            }
        }
        else if (_isShown)
        {
            _isShown = false;
            _growthOverlayToggle.DisableOverlay();
        }

        return false;
    }
}
