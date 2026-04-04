namespace GrowthOverlay;

internal class GrowthOverlayService : ILoadableSingleton, ILateUpdatableSingleton
{
    readonly Underlay _underlay;
    readonly CameraService _cameraComponent;
    readonly UISettings _uiSettings;
    readonly Dictionary<VisualElement, Vector3> _items = new();
    readonly List<GrowthOverlayToggle> _toggles = new();
    bool _enabled;
    bool _isDirty;

    internal bool IsEnabled => _enabled;

    public GrowthOverlayService(Underlay underlay, CameraService cameraComponent, UISettings uiSettings)
    {
        _underlay = underlay;
        _cameraComponent = cameraComponent;
        _uiSettings = uiSettings;
    }

    public void Load()
    {
        _cameraComponent.CameraPositionOrRotationChanged += delegate { UpdatePosition(); };
        _uiSettings.UIScaleFactorChanged += delegate { UpdatePosition(); };
    }

    public void LateUpdateSingleton()
    {
        if (_isDirty)
            UpdatePosition();
    }

    public GrowthOverlayToggle GetGrowthOverlayToggle()
    {
        var toggle = new GrowthOverlayToggle();
        _toggles.Add(toggle);
        toggle.StateChanged += delegate { UpdateOverlay(); };
        return toggle;
    }

    public void Add(VisualElement element, Vector3 anchor)
    {
        if (_items.TryAdd(element, anchor) && _enabled)
        {
            _underlay.Add(element);
            _isDirty = true;
        }
    }

    public void Remove(VisualElement element)
    {
        if (_items.Remove(element) && _enabled)
            _underlay.Remove(element);
    }

    void UpdatePosition()
    {
        if (_enabled)
        {
            foreach (var (item, anchor) in _items)
                UpdatePosition(item, anchor);
        }
        _isDirty = false;
    }

    void UpdateOverlay()
    {
        bool shouldShow = _toggles.FastAny(t => t.Enabled) && _toggles.FastAll(t => !t.Hidden);
        if (shouldShow && !_enabled)
            Enable();
        else if (!shouldShow && _enabled)
            Disable();
    }

    void Enable()
    {
        _enabled = true;
        foreach (var key in _items.Keys)
            _underlay.Add(key);
        _isDirty = true;
    }

    void Disable()
    {
        _enabled = false;
        foreach (var key in _items.Keys)
            _underlay.Remove(key);
    }

    void UpdatePosition(VisualElement item, Vector3 anchor)
    {
        var root = _underlay.Root;
        if (item.panel is null)
            return;

        bool inFront = _cameraComponent.IsInFront(anchor);
        item.ToggleDisplayStyle(inFront);
        if (inFront)
        {
            var pos = _cameraComponent.WorldSpaceToPanelSpace(root, anchor);
            item.style.translate = new Translate(
                pos.x - root.layout.width / 2f,
                pos.y - root.layout.height / 2f);
        }
    }
}
