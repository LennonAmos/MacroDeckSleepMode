using SuchByte.MacroDeck.Plugins;
using System.Reflection;

namespace MacroDeckSleepMode;

public sealed class Main : MacroDeckPlugin
{
    public Main()
    {
        SetPluginMetadata("Name", "MacroDeck Sleep Mode");
        SetPluginMetadata("Version", "1.0.3");
        SetPluginMetadata("Author", "lenno");

        if (!PluginManager.UpdatedPlugins.Contains(this))
        {
            PluginManager.UpdatedPlugins.Add(this);
        }
    }

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

    private void SetPluginMetadata(string propertyName, string value)
    {
        typeof(MacroDeckPlugin)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(this, value);
    }
}
