using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class SleepModeOffAction : PluginAction
{
    public override string Name => "Sleep Mode Off";

    public override string Description => "Turns sleep-mode variables off.";

    public override string BindableVariable => "macrodeck_sleeping";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepModeController.SetSleeping(false);
    }
}
