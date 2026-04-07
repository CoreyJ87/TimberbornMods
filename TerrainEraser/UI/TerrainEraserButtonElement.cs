namespace TerrainEraser.UI;

public class TerrainEraserButtonElement(
    ToolButtonFactory toolButtonFactory,
    DevModeManager devModeManager,
    EventBus eventBus,
    TerrainEraserTool tool
) : CustomRootToolElement<TerrainEraserTool>(toolButtonFactory, tool)
{
    public override string Id { get; } = "LV.TerrainEraser";
    protected override string ImageName { get; } = "DeleteObjectIcon";

    public override IEnumerable<BottomBarElement> GetElements()
    {
        foreach (var el in base.GetElements())
        {
            var root = el.MainElement;
            root.ToggleDisplayStyle(devModeManager.Enabled);
            eventBus.Register(new DevModeVisibilityToggle(root));
            yield return el;
        }
    }

    class DevModeVisibilityToggle(VisualElement root)
    {
        [OnEvent]
        public void OnDevModeToggled(DevModeToggledEvent e)
        {
            root.ToggleDisplayStyle(e.Enabled);
        }
    }
}
