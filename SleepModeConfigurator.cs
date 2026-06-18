using System.Diagnostics;
using SuchByte.MacroDeck.ExtensionStore;
using SuchByte.MacroDeck.Icons;

namespace MacroDeckSleepMode;

internal sealed class SleepModeConfigurator : Form
{
    private readonly ComboBox themePicker;
    private readonly Label requirementLabel;
    private readonly Button installButton;
    private readonly Panel previewPanel;
    private readonly System.Windows.Forms.Timer previewTimer;
    private int previewFrame;

    public SleepModeConfigurator()
    {
        Text = "MacroDeck Sleep Mode";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ClientSize = new Size(840, 520);

        const int left = 28;
        const int leftWidth = 280;
        const int previewLeft = 350;
        const int previewWidth = 450;

        var title = new Label
        {
            Text = "Sleep Mode",
            Font = new Font("Segoe UI Semibold", 16, FontStyle.Bold),
            AutoSize = true,
            Left = left,
            Top = 22
        };

        var themeLabel = new Label
        {
            Text = "Background theme",
            AutoSize = true,
            Left = left,
            Top = 78
        };

        var previewLabel = new Label
        {
            Text = "Live theme preview",
            AutoSize = true,
            Left = previewLeft,
            Top = 78
        };

        themePicker = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = left,
            Top = 104,
            Width = leftWidth
        };
        themePicker.Items.AddRange(Enum.GetNames<SleepTheme>());
        themePicker.SelectedItem = SleepProfileController.GetCurrentTheme().ToString();

        requirementLabel = new Label
        {
            AutoSize = false,
            Left = left,
            Top = 142,
            Width = leftWidth,
            Height = 58,
            ForeColor = Color.FromArgb(96, 96, 96)
        };
        installButton = new Button
        {
            Text = "Install required icon pack",
            Left = left,
            Top = 216,
            Width = leftWidth,
            Height = 36
        };
        installButton.Click += (_, _) => InstallRequiredIconPack();
        UpdateRequirementLabel();

        previewPanel = new DoubleBufferedPanel
        {
            Left = previewLeft,
            Top = 104,
            Width = previewWidth,
            Height = 320,
            BackColor = Color.FromArgb(16, 18, 22)
        };
        previewPanel.Paint += (_, e) => PaintPreview(e.Graphics, previewPanel.ClientRectangle);

        themePicker.SelectedIndexChanged += (_, _) =>
        {
            UpdateRequirementLabel();
            previewPanel.Invalidate();
        };

        previewTimer = new System.Windows.Forms.Timer
        {
            Interval = 40
        };
        previewTimer.Tick += (_, _) =>
        {
            previewFrame++;
            previewPanel.Invalidate();
        };
        previewTimer.Start();

        var buildButton = new Button
        {
            Text = "Rebuild sleep profile",
            Left = left,
            Top = 272,
            Width = leftWidth,
            Height = 36
        };
        buildButton.Click += (_, _) =>
        {
            SleepProfileController.BuildSleepProfileLayout();
            MessageBox.Show(this, "Sleep profile rebuilt.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        var folderButton = new Button
        {
            Text = "Open settings folder",
            Left = left,
            Top = 320,
            Width = leftWidth,
            Height = 36
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
            Left = previewLeft + previewWidth - 176,
            Top = 462,
            Width = 82,
            Height = 32
        };
        saveButton.Click += (_, _) => SaveTheme();

        var closeButton = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.Cancel,
            Left = previewLeft + previewWidth - 82,
            Top = 462,
            Width = 82,
            Height = 32
        };

        Controls.AddRange(new Control[]
        {
            title,
            themeLabel,
            previewLabel,
            themePicker,
            requirementLabel,
            installButton,
            previewPanel,
            buildButton,
            folderButton,
            saveButton,
            closeButton
        });

        AcceptButton = saveButton;
        CancelButton = closeButton;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            previewTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateRequirementLabel()
    {
        var theme = GetSelectedTheme();
        var iconPackName = SleepProfileController.GetIconPackNameForTheme(theme);
        requirementLabel.Text = IsIconPackInstalled(theme)
            ? $"Installed icon pack:\r\n{iconPackName}"
            : $"Required icon pack missing:\r\n{iconPackName}";
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

    private void PaintPreview(Graphics graphics, Rectangle bounds)
    {
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        graphics.SetClip(bounds);

        var theme = GetSelectedTheme();
        var grid = SleepProfileController.GetTargetGridSize();
        var phase = previewFrame / 120f;

        using var background = new System.Drawing.Drawing2D.LinearGradientBrush(bounds, GetPreviewColor(theme, 0), GetPreviewColor(theme, 1), 30f);
        graphics.FillRectangle(background, bounds);

        DrawThemeMotion(graphics, bounds, theme, phase);

        var layout = GetPreviewLayout(bounds, grid);

        using var tileBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
        using var tileBorder = new Pen(Color.FromArgb(65, 255, 255, 255), 1f);
        for (var row = 0; row < grid.Rows; row++)
        {
            for (var column = 0; column < grid.Columns; column++)
            {
                var rect = new RectangleF(
                    layout.GridRect.Left + column * (layout.TileSize + layout.Gap),
                    layout.GridRect.Top + row * (layout.TileSize + layout.Gap),
                    layout.TileSize,
                    layout.TileSize);
                using var path = RoundedRect(rect, Math.Max(4f, layout.TileSize * 0.2f));
                graphics.FillPath(tileBrush, path);
                graphics.DrawPath(tileBorder, path);
            }
        }

        DrawShine(graphics, layout.GridRect, phase);

        using var textFont = CreateFittedFont(graphics, "MACRODECK SLEEPING", layout.TextRect, layout.DeckRect);
        using var smallFont = new Font("Segoe UI", 8, FontStyle.Regular);
        using var textBrush = new SolidBrush(Color.FromArgb(245, 248, 255));
        using var shadowBrush = new SolidBrush(Color.FromArgb(120, 0, 0, 0));
        var shadowRect = new RectangleF(layout.TextRect.X + 1.4f, layout.TextRect.Y + 2.2f, layout.TextRect.Width, layout.TextRect.Height);
        using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString("MACRODECK SLEEPING", textFont, shadowBrush, shadowRect, center);
        graphics.DrawString("MACRODECK SLEEPING", textFont, textBrush, layout.TextRect, center);

        graphics.DrawString($"{theme} preview - {grid.Columns} x {grid.Rows}", smallFont, textBrush, new RectangleF(bounds.Left, bounds.Bottom - 24, bounds.Width, 18), center);
        graphics.ResetClip();
    }

    private static PreviewLayout GetPreviewLayout(Rectangle bounds, GridSize grid)
    {
        const float panelMargin = 26f;
        const float captionHeight = 30f;
        var fitRect = new RectangleF(
            bounds.Left + panelMargin,
            bounds.Top + panelMargin,
            bounds.Width - panelMargin * 2f,
            bounds.Height - panelMargin * 2f - captionHeight);

        var gapUnits = 0.11f;
        var ratioWidth = grid.Columns + Math.Max(0, grid.Columns - 1) * gapUnits;
        var ratioHeight = grid.Rows + Math.Max(0, grid.Rows - 1) * gapUnits;
        var tileSize = Math.Min(fitRect.Width / ratioWidth, fitRect.Height / ratioHeight);
        tileSize = Math.Max(12f, tileSize);
        var gap = Math.Max(3f, tileSize * gapUnits);
        var gridWidth = tileSize * grid.Columns + gap * Math.Max(0, grid.Columns - 1);
        var gridHeight = tileSize * grid.Rows + gap * Math.Max(0, grid.Rows - 1);
        var gridRect = new RectangleF(
            fitRect.Left + (fitRect.Width - gridWidth) / 2f,
            fitRect.Top + (fitRect.Height - gridHeight) / 2f,
            gridWidth,
            gridHeight);

        var textHeight = Math.Min(gridRect.Height * 0.42f, tileSize * 0.72f);
        textHeight = Math.Max(18f, textHeight);
        var textRect = new RectangleF(
            gridRect.Left,
            gridRect.Top + (gridRect.Height - textHeight) / 2f,
            gridRect.Width,
            textHeight);

        return new PreviewLayout(fitRect, gridRect, textRect, tileSize, gap);
    }

    private static Font CreateFittedFont(Graphics graphics, string text, RectangleF textRect, RectangleF deckRect)
    {
        var maxSize = Math.Min(28f, Math.Max(12f, deckRect.Width / 15f));
        var minSize = 9f;
        for (var size = maxSize; size >= minSize; size -= 0.5f)
        {
            var candidate = new Font("Segoe UI Semibold", size, FontStyle.Bold);
            var measured = graphics.MeasureString(text, candidate);
            if (measured.Width <= textRect.Width - 6f && measured.Height <= textRect.Height + 6f)
            {
                return candidate;
            }

            candidate.Dispose();
        }

        return new Font("Segoe UI Semibold", minSize, FontStyle.Bold);
    }

    private static void DrawThemeMotion(Graphics graphics, Rectangle bounds, SleepTheme theme, float phase)
    {
        switch (theme)
        {
            case SleepTheme.Starlight:
                DrawStarlightPreview(graphics, bounds, phase);
                break;
            case SleepTheme.Rainfall:
                DrawRainfallPreview(graphics, bounds, phase);
                break;
            case SleepTheme.Firefly:
                DrawFireflyPreview(graphics, bounds, phase);
                break;
            case SleepTheme.Prism:
                DrawPrismPreview(graphics, bounds, phase);
                break;
            default:
                DrawWaves(graphics, bounds, theme, phase);
                break;
        }
    }

    private static void DrawWaves(Graphics graphics, Rectangle bounds, SleepTheme theme, float phase)
    {
        var colors = GetWaveColors(theme);
        for (var layer = 0; layer < 3; layer++)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var baseY = bounds.Top + bounds.Height * (0.46f + layer * 0.12f);
            var amplitude = bounds.Height * (0.055f + layer * 0.014f);
            var offset = layer * 0.18f + phase * (0.18f + layer * 0.04f);

            path.StartFigure();
            path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, baseY);
            var previousX = bounds.Left;
            var previousY = baseY;
            for (var x = bounds.Left; x <= bounds.Right; x += 8)
            {
                var t = (x - bounds.Left) / (float)Math.Max(1, bounds.Width);
                var y = baseY
                    + MathF.Sin((t * 2.0f + offset) * MathF.Tau) * amplitude
                    + MathF.Sin((t * 3.1f - offset * 0.65f) * MathF.Tau) * amplitude * 0.35f;
                path.AddLine(previousX, previousY, x, y);
                previousX = x;
                previousY = y;
            }
            path.AddLine(bounds.Right, bounds.Bottom, bounds.Left, bounds.Bottom);
            path.CloseFigure();

            using var brush = new SolidBrush(Color.FromArgb(70 - layer * 12, colors[layer]));
            graphics.FillPath(brush, path);
        }
    }

    private static void DrawStarlightPreview(Graphics graphics, Rectangle bounds, float phase)
    {
        var loop = phase * MathF.Tau;
        for (var i = 0; i < 58; i++)
        {
            var x = bounds.Left + ((i * 73) % Math.Max(1, bounds.Width)) + MathF.Sin(loop + i * 0.57f) * 6f;
            var y = bounds.Top + ((i * 41) % Math.Max(1, bounds.Height)) + MathF.Cos(loop * 0.7f + i * 0.34f) * 5f;
            var size = 1.2f + (i % 4) * 0.65f;
            var alpha = 58 + (int)(70 * (0.5f + 0.5f * MathF.Sin(loop + i * 0.4f)));
            using var brush = new SolidBrush(Color.FromArgb(alpha, 238, 246, 255));
            graphics.FillEllipse(brush, x, y, size, size);
        }

        for (var i = 0; i < 4; i++)
        {
            var radius = bounds.Width * (0.12f + i * 0.025f);
            var x = bounds.Left + bounds.Width * (0.16f + i * 0.22f) + MathF.Sin(loop + i) * 18f;
            var y = bounds.Top + bounds.Height * (0.22f + (i % 2) * 0.36f) + MathF.Cos(loop * 0.85f + i) * 14f;
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(x - radius, y - radius, radius * 2f, radius * 2f);
            using var glow = new System.Drawing.Drawing2D.PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(42, 118, 150, 255),
                SurroundColors = [Color.FromArgb(0, 118, 150, 255)]
            };
            graphics.FillPath(glow, path);
        }
    }

    private static void DrawRainfallPreview(Graphics graphics, Rectangle bounds, float phase)
    {
        var span = bounds.Height + 100f;
        using var pen = new Pen(Color.FromArgb(60, 190, 226, 255), 1.4f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };

        for (var i = 0; i < 76; i++)
        {
            var baseX = bounds.Left + ((i * 47) % (bounds.Width + 90)) - 45f;
            var y = bounds.Top + (((i * 31) + phase * span * (1f + (i % 5) * 0.05f)) % span) - 70f;
            var x = baseX + ((y - bounds.Top) / Math.Max(1f, bounds.Height)) * 28f;
            var length = 18f + (i % 7) * 4f;
            pen.Color = Color.FromArgb(32 + (i % 5) * 10, 190, 226, 255);
            graphics.DrawLine(pen, x, y, x + 12f, y + length);
        }
    }

    private static void DrawFireflyPreview(Graphics graphics, Rectangle bounds, float phase)
    {
        var loop = phase * MathF.Tau;
        for (var i = 0; i < 28; i++)
        {
            var cx = bounds.Left + ((i * 71) % Math.Max(1, bounds.Width));
            var cy = bounds.Top + ((i * 43) % Math.Max(1, bounds.Height));
            var x = cx + MathF.Sin(loop * (0.7f + (i % 4) * 0.08f) + i) * (12f + (i % 6) * 5f);
            var y = cy + MathF.Cos(loop * (0.8f + (i % 5) * 0.06f) + i * 0.7f) * (10f + (i % 5) * 4f);
            var radius = 11f + (i % 5) * 3.5f;
            var alpha = 66 + (int)(60 * (0.5f + 0.5f * MathF.Sin(loop + i * 0.5f)));

            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(x - radius, y - radius, radius * 2f, radius * 2f);
            using var glow = new System.Drawing.Drawing2D.PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(alpha, 255, 226, 104),
                SurroundColors = [Color.FromArgb(0, 255, 226, 104)]
            };
            graphics.FillPath(glow, path);
        }
    }

    private static void DrawPrismPreview(Graphics graphics, Rectangle bounds, float phase)
    {
        for (var i = 0; i < 12; i++)
        {
            var baseX = bounds.Left + ((i * 81) % (bounds.Width + 130)) - 65f;
            var baseY = bounds.Top + ((i * 53) % (bounds.Height + 90)) - 45f;
            var x = baseX + ((phase * 70f + (i % 4) * 12f) % 70f);
            var y = baseY + MathF.Sin(phase * MathF.Tau + i) * 10f;
            var width = 56f + (i % 5) * 18f;
            var height = 34f + (i % 4) * 14f;
            var color = (i % 4) switch
            {
                0 => Color.FromArgb(34, 88, 222, 255),
                1 => Color.FromArgb(32, 238, 106, 216),
                2 => Color.FromArgb(30, 255, 196, 92),
                _ => Color.FromArgb(32, 138, 118, 255)
            };
            var points = new[]
            {
                new PointF(x, y + height * 0.18f),
                new PointF(x + width * 0.72f, y),
                new PointF(x + width, y + height * 0.84f),
                new PointF(x + width * 0.24f, y + height)
            };
            using var brush = new SolidBrush(color);
            graphics.FillPolygon(brush, points);
        }
    }

    private static void DrawShine(Graphics graphics, RectangleF gridBounds, float phase)
    {
        using var oldClip = graphics.Clip.Clone();
        graphics.SetClip(gridBounds, System.Drawing.Drawing2D.CombineMode.Intersect);

        var loop = phase % 1f;
        var shineWidth = Math.Max(36f, gridBounds.Width / 6f);
        var x = gridBounds.Left - shineWidth + (gridBounds.Width + shineWidth * 2) * loop;
        var rect = new RectangleF(x, gridBounds.Top - 12, shineWidth, gridBounds.Height + 24);
        using var shine = new System.Drawing.Drawing2D.LinearGradientBrush(
            rect,
            Color.FromArgb(0, 255, 255, 255),
            Color.FromArgb(45, 255, 255, 255),
            25f);
        shine.Blend = new System.Drawing.Drawing2D.Blend
        {
            Positions = [0f, 0.42f, 0.5f, 0.58f, 1f],
            Factors = [0f, 0f, 1f, 0f, 0f]
        };
        graphics.FillRectangle(shine, rect);
        graphics.Clip = oldClip;
    }

    private static Color GetPreviewColor(SleepTheme theme, int index)
    {
        return theme switch
        {
            SleepTheme.Ocean => index == 0 ? Color.FromArgb(8, 64, 82) : Color.FromArgb(28, 126, 134),
            SleepTheme.Sunset => index == 0 ? Color.FromArgb(98, 42, 52) : Color.FromArgb(210, 126, 72),
            SleepTheme.Midnight => index == 0 ? Color.FromArgb(14, 18, 38) : Color.FromArgb(58, 72, 126),
            SleepTheme.Nebula => index == 0 ? Color.FromArgb(38, 22, 66) : Color.FromArgb(128, 76, 154),
            SleepTheme.Forest => index == 0 ? Color.FromArgb(20, 58, 42) : Color.FromArgb(88, 130, 76),
            SleepTheme.Ember => index == 0 ? Color.FromArgb(70, 30, 26) : Color.FromArgb(190, 86, 48),
            SleepTheme.Glacier => index == 0 ? Color.FromArgb(36, 74, 94) : Color.FromArgb(146, 198, 206),
            SleepTheme.Starlight => index == 0 ? Color.FromArgb(11, 16, 44) : Color.FromArgb(60, 66, 136),
            SleepTheme.Rainfall => index == 0 ? Color.FromArgb(14, 30, 42) : Color.FromArgb(64, 90, 104),
            SleepTheme.Firefly => index == 0 ? Color.FromArgb(12, 36, 28) : Color.FromArgb(70, 92, 50),
            SleepTheme.Prism => index == 0 ? Color.FromArgb(30, 22, 54) : Color.FromArgb(98, 58, 128),
            _ => index == 0 ? Color.FromArgb(32, 44, 82) : Color.FromArgb(84, 128, 190)
        };
    }

    private static Color[] GetWaveColors(SleepTheme theme)
    {
        return theme switch
        {
            SleepTheme.Ocean => [Color.FromArgb(72, 196, 210), Color.FromArgb(32, 126, 166), Color.FromArgb(12, 84, 124)],
            SleepTheme.Sunset => [Color.FromArgb(255, 190, 116), Color.FromArgb(238, 104, 84), Color.FromArgb(124, 52, 84)],
            SleepTheme.Midnight => [Color.FromArgb(118, 140, 230), Color.FromArgb(76, 92, 170), Color.FromArgb(34, 42, 96)],
            SleepTheme.Nebula => [Color.FromArgb(208, 116, 224), Color.FromArgb(128, 76, 186), Color.FromArgb(58, 44, 112)],
            SleepTheme.Forest => [Color.FromArgb(136, 188, 118), Color.FromArgb(74, 136, 96), Color.FromArgb(26, 82, 58)],
            SleepTheme.Ember => [Color.FromArgb(250, 142, 72), Color.FromArgb(190, 76, 58), Color.FromArgb(98, 36, 34)],
            SleepTheme.Glacier => [Color.FromArgb(190, 238, 240), Color.FromArgb(116, 178, 204), Color.FromArgb(54, 96, 132)],
            _ => [Color.FromArgb(132, 180, 255), Color.FromArgb(96, 128, 224), Color.FromArgb(60, 74, 156)]
        };
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(RectangleF bounds, float radius)
    {
        var diameter = radius * 2;
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private readonly record struct PreviewLayout(
        RectangleF DeckRect,
        RectangleF GridRect,
        RectangleF TextRect,
        float TileSize,
        float Gap);

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

    private sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.Opaque,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
    }
}
