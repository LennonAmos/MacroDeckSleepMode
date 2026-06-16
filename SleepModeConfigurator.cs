using System.Diagnostics;

namespace MacroDeckSleepMode;

internal sealed class SleepModeConfigurator : Form
{
    private readonly ComboBox themePicker;

    public SleepModeConfigurator()
    {
        Text = "MacroDeck Sleep Mode";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        Width = 430;
        Height = 260;

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

        var buildButton = new Button
        {
            Text = "Rebuild sleep profile",
            Left = 24,
            Top = 142,
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
            Top = 142,
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
            Top = 188,
            Width = 80,
            Height = 30
        };
        saveButton.Click += (_, _) => SaveTheme();

        var closeButton = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.Cancel,
            Left = 304,
            Top = 188,
            Width = 80,
            Height = 30
        };

        Controls.AddRange(new Control[]
        {
            title,
            themeLabel,
            themePicker,
            buildButton,
            folderButton,
            saveButton,
            closeButton
        });

        AcceptButton = saveButton;
        CancelButton = closeButton;
    }

    private void SaveTheme()
    {
        if (themePicker.SelectedItem is not string selected ||
            !Enum.TryParse<SleepTheme>(selected, out var theme))
        {
            return;
        }

        SleepProfileController.SetTheme(theme);
    }
}
