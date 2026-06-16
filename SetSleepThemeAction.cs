using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public abstract class SetSleepThemeAction : PluginAction
{
    protected abstract SleepTheme Theme { get; }

    public override string Name => $"Sleep Theme: {Theme}";

    public override string Description => $"Use the {Theme} sleep background animation.";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepProfileController.SetTheme(Theme);
    }
}

public sealed class SetSleepThemeAuroraAction : SetSleepThemeAction
{
    protected override SleepTheme Theme => SleepTheme.Aurora;
}

public sealed class SetSleepThemeOceanAction : SetSleepThemeAction
{
    protected override SleepTheme Theme => SleepTheme.Ocean;
}

public sealed class SetSleepThemeSunsetAction : SetSleepThemeAction
{
    protected override SleepTheme Theme => SleepTheme.Sunset;
}

public sealed class SetSleepThemeMidnightAction : SetSleepThemeAction
{
    protected override SleepTheme Theme => SleepTheme.Midnight;
}
