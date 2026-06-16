using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class Main : MacroDeckPlugin
{
    public override bool CanConfigure => true;

    public override void OpenConfigurator()
    {
        using var configurator = new SleepModeConfigurator();
        configurator.ShowDialog();
    }

    public override void Enable()
    {
        SleepModeController.SetPlugin(this);
        SleepProfileController.SetPlugin(this);
        Actions = new List<PluginAction>
        {
            new GoToSleepProfileAction()
        };
    }
}
