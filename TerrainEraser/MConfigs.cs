global using TerrainEraser.Services;
global using TerrainEraser.UI;

namespace TerrainEraser;

[Context("Game")]
public class MGameConfig : Configurator
{
    public override void Configure()
    {
        Bind<TerrainEraserTool>().AsSingleton();
        this.MultiBindCustomTool<TerrainEraserButtonElement>();
    }
}
