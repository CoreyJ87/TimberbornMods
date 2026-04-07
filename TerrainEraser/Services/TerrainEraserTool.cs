namespace TerrainEraser.Services;

public class TerrainEraserTool(
    ILoc t,
    AreaPicker areaPicker,
    CursorService cursorService,
    InputService inputService,
    DevModeManager devModeManager,
    DialogService dialogService,
    DestructionService destructionService,
    ITerrainService terrainService
) : ITool, IToolDescriptor, ILoadableSingleton, IInputProcessor
{
#nullable disable
    ToolDescription toolDescription;
#nullable enable

    public void Load()
    {
        toolDescription = new ToolDescription.Builder(t.T("LV.TE.ToolName"))
            .AddSection(t.T("LV.TE.ToolDesc"))
            .AddSection(t.T("LV.TE.ToolDescShift"))
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
        areaPicker.Reset();
        destructionService.UnhighlightDestructionEntities();
        cursorService.ResetCursor();
        inputService.RemoveInputProcessor(this);
    }

    public bool ProcessInput()
    {
        if (!devModeManager.Enabled) return false;

        return areaPicker.PickTerrainIntArea(PreviewCallback, ActionCallback, ShowNoneCallback);
    }

    Vector3Int[] FilterBlocks(IEnumerable<Vector3Int> terrainBlocks)
    {
        bool shiftHeld = Keyboard.current?.shiftKey.isPressed == true;
        if (shiftHeld)
        {
            return terrainBlocks
                .Select(b => new Vector3Int(b.x, b.y, b.z + 1))
                .Where(b => terrainService.Underground(b))
                .ToArray();
        }

        return terrainBlocks.Where(b => terrainService.Underground(b)).ToArray();
    }

    void PreviewCallback(IEnumerable<Vector3Int> terrainBlocks, Ray ray)
    {
        var blocks = FilterBlocks(terrainBlocks);
        if (blocks.Length == 0)
        {
            destructionService.UnhighlightDestructionEntities();
            return;
        }

        var destroying = destructionService.QueryDestructingEntities(blocks);
        destructionService.HighlightDestructionEntities(destroying);
    }

    void ActionCallback(IEnumerable<Vector3Int> terrainBlocks, Ray ray)
    {
        var blocks = FilterBlocks(terrainBlocks);
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
