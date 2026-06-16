using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;

namespace MacroDeckSleepMode;

internal static class SleepModeController
{
    public const int TileCount = 15;
    private const string Message = "MACRODECK SLEEPING";
    private static readonly object Gate = new();
    private static MacroDeckPlugin? plugin;
    private static System.Threading.Timer? timer;
    private static bool sleeping;
    private static int frame;

    public static void SetPlugin(MacroDeckPlugin macroDeckPlugin)
    {
        lock (Gate)
        {
            plugin = macroDeckPlugin;
            sleeping = false;
            frame = 0;
            Publish();
        }
    }

    public static void Toggle()
    {
        lock (Gate)
        {
            SetSleeping(!sleeping);
        }
    }

    public static void SetSleeping(bool value)
    {
        lock (Gate)
        {
            sleeping = value;
            frame = 0;

            timer?.Dispose();
            timer = null;

            Publish();
        }
    }

    private static void Publish()
    {
        if (plugin is null)
        {
            return;
        }

        VariableManager.SetValue("macrodeck_sleeping", sleeping, VariableType.Bool, plugin, Array.Empty<string>());
        VariableManager.SetValue("macrodeck_sleep_text", sleeping ? "Macrodeck Sleeping" : string.Empty, VariableType.String, plugin, Array.Empty<string>());
        VariableManager.SetValue("macrodeck_sleep_frame", frame, VariableType.Integer, plugin, Array.Empty<string>());

        var tiles = BuildTiles();
        for (var i = 0; i < TileCount; i++)
        {
            VariableManager.SetValue($"macrodeck_sleep_tile_{i + 1:00}", tiles[i], VariableType.String, plugin, Array.Empty<string>());
        }
    }

    private static string[] BuildTiles()
    {
        var tiles = Enumerable.Repeat(string.Empty, TileCount).ToArray();
        if (!sleeping)
        {
            return tiles;
        }

        var textTiles = new[]
        {
            string.Empty, "MAC", "RO", "DECK", string.Empty,
            string.Empty, string.Empty, "SLEEP", string.Empty, string.Empty,
            string.Empty, string.Empty, "ING", string.Empty, string.Empty
        };

        for (var i = 0; i < textTiles.Length && i < tiles.Length; i++)
        {
            tiles[i] = textTiles[i];
        }

        var sparkle = frame % TileCount;
        if (string.IsNullOrEmpty(tiles[sparkle]))
        {
            tiles[sparkle] = "/";
        }

        var sparkle2 = (frame + 5) % TileCount;
        if (string.IsNullOrEmpty(tiles[sparkle2]))
        {
            tiles[sparkle2] = "*";
        }

        var sparkle3 = (frame + 10) % TileCount;
        if (string.IsNullOrEmpty(tiles[sparkle3]))
        {
            tiles[sparkle3] = "\\";
        }

        return tiles;
    }
}
