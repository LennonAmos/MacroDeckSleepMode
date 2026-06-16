using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class GoToSleepProfileAction : PluginAction
{
    public override string Name => "Go To Sleep Profile";

    public override string Description => "Switches to the Macrodeck Sleeping profile and remembers the previous profile.";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepModeController.SetSleeping(true);
        SleepProfileController.GoToSleepProfile(clientId);
    }
}
