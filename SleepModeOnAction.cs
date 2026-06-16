using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class SleepModeOnAction : PluginAction
{
    public override string Name => "Sleep Mode On";

    public override string Description => "Turns sleep-mode variables on.";

    public override string BindableVariable => "macrodeck_sleeping_dev";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepModeController.SetSleeping(true);
    }
}
