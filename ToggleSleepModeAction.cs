using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class ToggleSleepModeAction : PluginAction
{
    public override string Name => "Toggle Sleep Mode";

    public override string Description => "Toggles sleep-mode variables for labels and safe sleep screens.";

    public override string BindableVariable => "macrodeck_sleeping_dev";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepModeController.Toggle();
    }
}
