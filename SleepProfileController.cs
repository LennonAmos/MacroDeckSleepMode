using SuchByte.MacroDeck;
using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.Device;
using SuchByte.MacroDeck.Folders;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Profiles;
using SuchByte.MacroDeck.Server;
using SuchByte.MacroDeck.Variables;

namespace MacroDeckSleepMode;

public enum SleepTheme
{
    Aurora,
    Ocean,
    Sunset,
    Midnight
}

internal static class SleepProfileController
{
    public const string SleepProfileName = "Macrodeck Sleeping";
    private const string ThemeVariableName = "macrodeck_sleep_theme";
    private static readonly Dictionary<string, string> PreviousProfileByClient = new();
    private static readonly object ThemeGate = new();
    private static MacroDeckPlugin? plugin;
    private static SleepTheme currentTheme = SleepTheme.Aurora;
    private static bool themeLoaded;

    public static void SetPlugin(MacroDeckPlugin macroDeckPlugin)
    {
        lock (ThemeGate)
        {
            plugin = macroDeckPlugin;
            currentTheme = ReadThemeFromDisk();
            themeLoaded = true;
            PublishTheme();
        }
    }

    public static void SetTheme(SleepTheme theme)
    {
        lock (ThemeGate)
        {
            EnsureThemeLoaded();
            currentTheme = theme;
            SaveThemeToDisk(theme);
            PublishTheme();
        }

        BuildSleepProfileLayout();
    }

    public static bool GoToSleepProfile(string clientId)
    {
        EnsureSleepProfileLayout();
        var sleepProfile = ProfileManager.FindProfileByDisplayName(SleepProfileName);
        if (sleepProfile is null)
        {
            return false;
        }

        var currentProfile = GetCurrentProfile(clientId);
        if (currentProfile is not null && currentProfile.ProfileId != sleepProfile.ProfileId)
        {
            PreviousProfileByClient[GetMemoryKey(clientId)] = currentProfile.ProfileId;
        }

        return SetProfile(clientId, sleepProfile);
    }

    public static bool Wake(string clientId)
    {
        var key = GetMemoryKey(clientId);
        MacroDeckProfile? previousProfile = null;
        if (PreviousProfileByClient.TryGetValue(key, out var previousProfileId))
        {
            previousProfile = ProfileManager.FindProfileById(previousProfileId);
        }

        if (previousProfile is null)
        {
            PreviousProfileByClient.Remove(key);
            previousProfile = ProfileManager.Profiles.FirstOrDefault(profile =>
                !profile.DisplayName.Equals(SleepProfileName, StringComparison.OrdinalIgnoreCase));
            if (previousProfile is null)
            {
                return false;
            }
        }

        var changed = SetProfile(clientId, previousProfile);
        if (changed)
        {
            PreviousProfileByClient.Remove(key);
        }

        return changed;
    }

    public static void BuildSleepProfileLayout()
    {
        var profile = ProfileManager.FindProfileByDisplayName(SleepProfileName)
            ?? ProfileManager.CreateProfile(SleepProfileName);
        if (profile is null)
        {
            return;
        }

        profile.Rows = 3;
        profile.Columns = 5;
        profile.ButtonSpacing = 10;
        profile.ButtonRadius = 40;
        profile.ButtonBackground = true;

        var root = profile.Folders.FirstOrDefault(folder => folder.IsRootFolder)
            ?? profile.Folders.FirstOrDefault();
        if (root is null)
        {
            root = new MacroDeckFolder
            {
                DisplayName = "*Root*",
                FolderId = System.Guid.NewGuid().ToString(),
                Childs = [],
                ActionButtons = []
            };
            profile.Folders.Add(root);
        }

        foreach (var button in root.ActionButtons)
        {
            button.Dispose();
        }

        root.ActionButtons.Clear();

        var sleepIconPackName = GetIconPackName(GetTheme());
        for (var row = 0; row < profile.Rows; row++)
        {
            for (var column = 0; column < profile.Columns; column++)
            {
                var index = row * profile.Columns + column + 1;
                var icon = $"{sleepIconPackName}.tile_{index:00}";
                var button = new ActionButton
                {
                    Position_X = column,
                    Position_Y = row,
                    IconOff = icon,
                    IconOn = icon,
                    BackColorOff = Color.FromArgb(24, 24, 28),
                    BackColorOn = Color.FromArgb(34, 34, 40),
                    StateBindingVariable = "macrodeck_sleeping",
                    LabelOff = CreateTileLabel(),
                    LabelOn = CreateTileLabel(),
                    Actions = [new WakeFromSleepProfileAction()],
                    ActionsRelease = [],
                    ActionsLongPress = [],
                    ActionsLongPressRelease = [],
                    EventListeners = []
                };

                root.ActionButtons.Add(button);
            }
        }

        ProfileManager.Save();
        MacroDeckServer.UpdateFolder(root);
    }

    private static void EnsureSleepProfileLayout()
    {
        if (HasValidSleepProfileLayout())
        {
            return;
        }

        BuildSleepProfileLayout();
    }

    private static bool HasValidSleepProfileLayout()
    {
        var profile = ProfileManager.FindProfileByDisplayName(SleepProfileName);
        var root = profile?.Folders.FirstOrDefault(folder => folder.IsRootFolder)
            ?? profile?.Folders.FirstOrDefault();
        if (profile is null || root is null || profile.Rows != 3 || profile.Columns != 5)
        {
            return false;
        }

        if (root.ActionButtons.Count != 15)
        {
            return false;
        }

        var sleepIconPackName = GetIconPackName(GetTheme());
        return root.ActionButtons.All(button =>
            button.Actions.Any(action => action is WakeFromSleepProfileAction) &&
            button.IconOff.StartsWith($"{sleepIconPackName}.tile_", StringComparison.Ordinal) &&
            button.IconOn.StartsWith($"{sleepIconPackName}.tile_", StringComparison.Ordinal));
    }

    private static SleepTheme GetTheme()
    {
        lock (ThemeGate)
        {
            EnsureThemeLoaded();
            return currentTheme;
        }
    }

    private static void EnsureThemeLoaded()
    {
        if (themeLoaded)
        {
            return;
        }

        currentTheme = ReadThemeFromDisk();
        themeLoaded = true;
    }

    private static string GetIconPackName(SleepTheme theme)
    {
        return theme switch
        {
            SleepTheme.Ocean => "MacroDeck Sleep Ocean",
            SleepTheme.Sunset => "MacroDeck Sleep Sunset",
            SleepTheme.Midnight => "MacroDeck Sleep Midnight",
            _ => "MacroDeck Sleep Aurora"
        };
    }

    private static SleepTheme ReadThemeFromDisk()
    {
        try
        {
            var themeFilePath = GetThemeFilePath();
            if (File.Exists(themeFilePath) &&
                Enum.TryParse<SleepTheme>(File.ReadAllText(themeFilePath).Trim(), true, out var theme))
            {
                return theme;
            }
        }
        catch
        {
            // Fall back to the default theme if the settings file is locked or malformed.
        }

        return SleepTheme.Aurora;
    }

    private static void SaveThemeToDisk(SleepTheme theme)
    {
        try
        {
            var themeFilePath = GetThemeFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(themeFilePath)!);
            File.WriteAllText(themeFilePath, theme.ToString());
        }
        catch
        {
            // Theme switching still works for the current session even if saving fails.
        }
    }

    private static string GetThemeFilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Macro Deck",
            "plugins",
            "lenno.MacroDeckSleepMode",
            "sleep_theme.txt");
    }

    private static void PublishTheme()
    {
        if (plugin is null)
        {
            return;
        }

        VariableManager.SetValue(ThemeVariableName, currentTheme.ToString(), VariableType.String, plugin, new[]
        {
            SleepTheme.Aurora.ToString(),
            SleepTheme.Ocean.ToString(),
            SleepTheme.Sunset.ToString(),
            SleepTheme.Midnight.ToString()
        });
    }

    private static MacroDeckProfile? GetCurrentProfile(string clientId)
    {
        return IsDesktopClient(clientId)
            ? ProfileManager.CurrentProfile
            : MacroDeckServer.GetMacroDeckClient(clientId)?.Profile;
    }

    private static bool SetProfile(string clientId, MacroDeckProfile profile)
    {
        if (IsDesktopClient(clientId))
        {
            MacroDeck.MainWindow?.DeckView?.SetProfile(profile);
            return true;
        }

        var device = DeviceManager.GetMacroDeckDevice(clientId);
        if (device is null)
        {
            return false;
        }

        DeviceManager.SetProfile(device, profile);
        return true;
    }

    private static bool IsDesktopClient(string clientId)
    {
        return string.IsNullOrWhiteSpace(clientId) || clientId == "-1";
    }

    private static string GetMemoryKey(string clientId)
    {
        return IsDesktopClient(clientId) ? "__desktop__" : clientId;
    }

    private static ButtonLabel CreateTileLabel()
    {
        return new ButtonLabel
        {
            LabelText = string.Empty,
            LabelPosition = ButtonLabelPosition.CENTER,
            LabelColor = Color.FromArgb(245, 248, 255),
            Size = 18,
            FontFamily = "Impact"
        };
    }
}
