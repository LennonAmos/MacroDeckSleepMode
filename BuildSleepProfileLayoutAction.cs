using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeckSleepMode;

public sealed class BuildSleepProfileLayoutAction : PluginAction
{
    public override string Name => "Build Sleep Profile Layout";

    public override string Description => "Creates or refreshes the Macrodeck Sleeping profile with joined animated wake buttons.";

    public override void Trigger(string clientId, ActionButton actionButton)
    {
        SleepProfileController.BuildSleepProfileLayout();
    }
}
