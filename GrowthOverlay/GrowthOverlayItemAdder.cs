namespace GrowthOverlay;

internal class GrowthOverlayItemAdder : TickableComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    GrowthOverlayService _growthOverlay = null!;
    VisualElementLoader _visualElementLoader = null!;
    EntitySelectionService _selectionManager = null!;

    BlockObjectCenter _blockObjectCenter = null!;
    Growable _growable = null!;
    GatherableYieldGrower? _yieldGrower;
    LivingNaturalResource _livingNaturalResource = null!;

    VisualElement _item = null!;
    Label _itemText = null!;

    [Inject]
    public void InjectDependencies(
        GrowthOverlayService growthOverlay,
        VisualElementLoader visualElementLoader,
        EntitySelectionService selectionManager)
    {
        _growthOverlay = growthOverlay;
        _visualElementLoader = visualElementLoader;
        _selectionManager = selectionManager;
    }

    public void Awake()
    {
        _blockObjectCenter = GetComponent<BlockObjectCenter>();
        _growable = GetComponent<Growable>();
        _yieldGrower = GetComponent<GatherableYieldGrower>();
        _livingNaturalResource = GetComponent<LivingNaturalResource>();

        _item = _visualElementLoader.LoadVisualElement("Game/StockpileOverlayItem");

        var btn = _item.Q<Button>("EntityButton");
        var selectionButton = _item.Q<NineSliceButton>("SelectionButton");

        selectionButton.clicked += delegate { _selectionManager.Select(_growable); };
        btn.clicked += delegate { _selectionManager.Select(_growable); };

        var icon = _item.Q<Image>("Icon");
        icon.sprite = _growable.GetComponent<LabeledEntity>().Image;
        icon.AddToClassList("icon--hidden");

        _itemText = _item.Q<Label>("Stock");

        var fillLevel = _item.Q<VisualElement>("Progress");
        fillLevel.parent.parent.Remove(fillLevel.parent);
    }

    public void InitializeEntity()
    {
        if (!_livingNaturalResource.IsDead && _livingNaturalResource.Enabled)
        {
            AddToOverlay();
            UpdateGrowth();
        }
    }

    public void DeleteEntity()
    {
        _growthOverlay.Remove(_item);
    }

    public override void Tick()
    {
        if (_growthOverlay.IsEnabled && !_livingNaturalResource.IsDead)
            UpdateGrowth();
    }

    void UpdateGrowth()
    {
        float progress = _growable.IsGrown && _yieldGrower is not null
            ? 100f + _yieldGrower.GrowthProgress * 100f
            : _growable.GrowthProgress * 100f;

        _itemText.text = $"{MathF.Floor(progress)}%";
        _itemText.ToggleDisplayStyle(visible: true);
    }

    void AddToOverlay()
    {
        var center = _blockObjectCenter.WorldCenter;
        var grounded = _blockObjectCenter.WorldCenterGrounded;
        float y = (center.y + grounded.y) * 0.5f;
        _growthOverlay.Add(_item, new Vector3(center.x, y, center.z));
    }
}
