using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class WakeFromSleepProfileAction : PluginAction
{
    public override string Name => "Wake From Sleep Profile";

    public override string Description => "Returns to the profile that was active before sleep mode.";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepProfileController.Wake(clientId);
        SleepModeController.SetSleeping(false);
    }
}
