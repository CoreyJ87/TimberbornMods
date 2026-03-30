namespace MoreHttpApi.Services;

[BindSingleton(Contexts = BindAttributeContext.All)]
public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(MoreHttpApi);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public ModSetting<bool> AutoStartApi { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.MHA.AutoStart")
        .SetLocalizedTooltip("LV.MHA.AutoStartDesc"));

    public ModSetting<int> AutoStartPort { get; } = new(8080, ModSettingDescriptor
        .CreateLocalized("LV.MHA.AutoStartPort")
        .SetLocalizedTooltip("LV.MHA.AutoStartPortDesc"));

    public ModSetting<bool> AllowNetworkAccess { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.MHA.AllowNetwork")
        .SetLocalizedTooltip("LV.MHA.AllowNetworkDesc"));

    public ModSetting<int> NetworkPort { get; } = new(8090, ModSettingDescriptor
        .CreateLocalized("LV.MHA.NetworkPort")
        .SetLocalizedTooltip("LV.MHA.NetworkPortDesc"));

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();
        AutoStartPort.Descriptor.SetEnableCondition(() => AutoStartApi.Value);
        AllowNetworkAccess.Descriptor.SetEnableCondition(() => AutoStartApi.Value);
        NetworkPort.Descriptor.SetEnableCondition(() => AutoStartApi.Value && AllowNetworkAccess.Value);
    }

}
