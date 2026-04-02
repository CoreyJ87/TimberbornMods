namespace Ladder;

[Context("Game")]
public class LadderConfigurator : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    static TemplateModule ProvideTemplateModule()
    {
        var builder = new TemplateModule.Builder();
        builder.AddDecorator<LadderSpec, LadderComponent>();
        return builder.Build();
    }
}
