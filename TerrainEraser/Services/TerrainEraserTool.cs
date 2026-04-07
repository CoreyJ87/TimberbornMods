namespace TerrainEraser.Services;

public class TerrainEraserTool(
    ILoc t,
    AreaPicker areaPicker,
    CursorService cursorService,
    InputService inputService,
    DevModeManager devModeManager,
    DialogService dialogService,
    DestructionService destructionService
) : ITool, IToolDescriptor, ILoadableSingleton, IInputProcessor
{
#nullable disable
    ToolDescription toolDescription;
#nullable enable

    public void Load()
    {
        toolDescription = new ToolDescription.Builder(t.T("LV.TE.ToolName"))
            .AddSection(t.T("LV.TE.ToolDesc"))
            .Build();
    }

    public ToolDescription DescribeTool() => toolDescription;

    public void Enter()
    {
        cursorService.SetCursor("DeleteBuildingCursor");
        inputService.AddInputProcessor(this);
    }

    public void Exit()
    {
        destructionService.UnhighlightDestructionEntities();
        cursorService.ResetCursor();
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (!devModeManager.Enabled) return false;

        return areaPicker.PickTerrainIntArea(PreviewCallback, ActionCallback, ShowNoneCallback);
    }

    void PreviewCallback(IEnumerable<Vector3Int> terrainBlocks, Vector3Int start)
    {
        var blocks = terrainBlocks.ToArray();
        if (blocks.Length == 0)
        {
            destructionService.UnhighlightDestructionEntities();
            return;
        }

        var destroying = destructionService.QueryDestructingEntities(blocks);
        destructionService.HighlightDestructionEntities(destroying);
    }

    void ActionCallback(IEnumerable<Vector3Int> terrainBlocks, Vector3Int start)
    {
        var blocks = terrainBlocks.ToArray();
        if (blocks.Length == 0) return;

        var destroying = destructionService.QueryDestructingEntities(blocks);
        AttemptDelete(destroying);
    }

    void ShowNoneCallback()
    {
        destructionService.UnhighlightDestructionEntities();
    }

    async void AttemptDelete(DestroyingEntities destroying)
    {
        var terrainCount = destroying.Terrains.Length;
        var buildingCount = destroying.BlockObjects.Length;

        var message = buildingCount > 0
            ? t.T("LV.TE.ConfirmWithBuildings", terrainCount, buildingCount)
            : t.T("LV.TE.Confirm", terrainCount);

        if (!await dialogService.ConfirmAsync(message,
                localizedOkText: "LV.TE.Delete",
                localizedCancelText: "LV.TE.Cancel"))
        {
            return;
        }

        destructionService.DestroyEntities(destroying);
    }
}
