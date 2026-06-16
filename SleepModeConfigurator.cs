using System.Diagnostics;
using SuchByte.MacroDeck.ExtensionStore;
using SuchByte.MacroDeck.Icons;

namespace MacroDeckSleepMode;

internal sealed class SleepModeConfigurator : Form
{
    private readonly ComboBox themePicker;
    private readonly Label requirementLabel;
    private readonly Button installButton;

    public SleepModeConfigurator()
    {
        Text = "MacroDeck Sleep Mode";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        Width = 430;
        Height = 335;

        var title = new Label
        {
            Text = "Sleep Mode",
            Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
            AutoSize = true,
            Left = 22,
            Top = 18
        };

        var themeLabel = new Label
        {
            Text = "Background theme",
            AutoSize = true,
            Left = 24,
            Top = 70
        };

        themePicker = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 24,
            Top = 95,
            Width = 360
        };
        themePicker.Items.AddRange(Enum.GetNames<SleepTheme>());
        themePicker.SelectedItem = SleepProfileController.GetCurrentTheme().ToString();

        requirementLabel = new Label
        {
            AutoSize = false,
            Left = 24,
            Top = 128,
            Width = 360,
            Height = 42,
            ForeColor = Color.FromArgb(214, 214, 214)
        };
        themePicker.SelectedIndexChanged += (_, _) => UpdateRequirementLabel();

        installButton = new Button
        {
            Text = "Install required icon pack",
            Left = 24,
            Top = 174,
            Width = 360,
            Height = 34
        };
        installButton.Click += (_, _) => InstallRequiredIconPack();
        UpdateRequirementLabel();

        var buildButton = new Button
        {
            Text = "Rebuild sleep profile",
            Left = 24,
            Top = 222,
            Width = 170,
            Height = 34
        };
        buildButton.Click += (_, _) =>
        {
            SleepProfileController.BuildSleepProfileLayout();
            MessageBox.Show(this, "Sleep profile rebuilt.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var folderButton = new Button
        {
            Text = "Open settings folder",
            Left = 214,
            Top = 222,
            Width = 170,
            Height = 34
        };
        folderButton.Click += (_, _) =>
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Macro Deck",
                "plugins",
                "lenno.MacroDeckSleepMode");
            Directory.CreateDirectory(folder);
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        };

        var saveButton = new Button
        {
            Text = "Save",
            DialogResult = DialogResult.OK,
            Left = 214,
            Top = 268,
            Width = 80,
            Height = 30
        };
        saveButton.Click += (_, _) => SaveTheme();

        var closeButton = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.Cancel,
            Left = 304,
            Top = 268,
            Width = 80,
            Height = 30
        };

        Controls.AddRange(new Control[]
        {
            title,
            themeLabel,
            themePicker,
            requirementLabel,
            installButton,
            buildButton,
            folderButton,
            saveButton,
            closeButton
        });

        AcceptButton = saveButton;
        CancelButton = closeButton;
    }

    private void UpdateRequirementLabel()
    {
        var theme = GetSelectedTheme();
        var iconPackName = SleepProfileController.GetIconPackNameForTheme(theme);
        requirementLabel.Text = IsIconPackInstalled(theme)
            ? $"Installed icon pack: {iconPackName}"
            : $"Required icon pack missing: {iconPackName}";
        installButton.Enabled = !IsIconPackInstalled(theme);
        installButton.Text = IsIconPackInstalled(theme)
            ? "Required icon pack installed"
            : "Install required icon pack";
    }

    private void InstallRequiredIconPack()
    {
        var theme = GetSelectedTheme();
        if (IsIconPackInstalled(theme))
        {
            UpdateRequirementLabel();
            return;
        }

        var packageId = SleepProfileController.GetIconPackPackageIdForTheme(theme);
        ExtensionStoreHelper.InstallIconPackById(packageId);
        UpdateRequirementLabel();
    }

    private bool ConfirmRequiredPack(SleepTheme theme)
    {
        if (IsIconPackInstalled(theme))
        {
            return true;
        }

        var iconPackName = SleepProfileController.GetIconPackNameForTheme(theme);
        var result = MessageBox.Show(
            this,
            $"{iconPackName} is required for this theme. Install it now?",
            Text,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            return false;
        }

        InstallRequiredIconPack();
        return IsIconPackInstalled(theme);
    }

    private static bool IsIconPackInstalled(SleepTheme theme)
    {
        var packageId = SleepProfileController.GetIconPackPackageIdForTheme(theme);
        return IconManager.IconPacks.Any(iconPack =>
            iconPack.PackageId.Equals(packageId, StringComparison.OrdinalIgnoreCase));
    }

    private SleepTheme GetSelectedTheme()
    {
        var selected = themePicker.SelectedItem?.ToString() ?? SleepTheme.Aurora.ToString();
        return Enum.TryParse<SleepTheme>(selected, out var theme) ? theme : SleepTheme.Aurora;
    }

    private void SaveTheme()
    {
        var theme = GetSelectedTheme();

        if (!ConfirmRequiredPack(theme))
        {
            DialogResult = DialogResult.None;
            return;
        }

        SleepProfileController.SetTheme(theme);
    }
}
