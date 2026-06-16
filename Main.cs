using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class Main : MacroDeckPlugin
{
    public override bool CanConfigure => false;

    public override void Enable()
    {
        SleepModeController.SetPlugin(this);
        SleepProfileController.SetPlugin(this);
        Actions = new List<PluginAction>
        {
            new ToggleSleepModeAction(),
            new SleepModeOnAction(),
            new SleepModeOffAction(),
            new GoToSleepProfileAction(),
            new WakeFromSleepProfileAction(),
            new BuildSleepProfileLayoutAction(),
            new SetSleepThemeAuroraAction(),
            new SetSleepThemeOceanAction(),
            new SetSleepThemeSunsetAction(),
            new SetSleepThemeMidnightAction()
        };
    }
}
