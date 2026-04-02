namespace GrowthOverlay;

[Context("Game")]
public class ModConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.Bind<GrowthOverlayService>().AsSingleton();
        containerDefinition.Bind<GrowthOverlayShower>().AsSingleton();
        containerDefinition.Bind<GrowthOverlayTogglePanel>().AsSingleton();
        containerDefinition.Bind<GrowthOverlayHider>().AsSingleton();
        containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    static TemplateModule ProvideTemplateModule()
    {
        var builder = new TemplateModule.Builder();
        builder.AddDecorator<Growable, GrowthOverlayItemAdder>();
        return builder.Build();
    }
}
