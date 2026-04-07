namespace TerrainEraser.UI;

public class TerrainEraserButtonElement(
    ToolButtonFactory toolButtonFactory,
    TerrainEraserTool tool
) : CustomRootToolElement<TerrainEraserTool>(toolButtonFactory, tool)
{
    public override string Id { get; } = "LV.TerrainEraser";
    protected override string ImageName { get; } = "DeleteBuildingTool";
}
