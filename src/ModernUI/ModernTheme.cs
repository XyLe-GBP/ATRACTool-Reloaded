using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace ATRACTool_Reloaded.ModernUI
{
    internal static class ThemeColors
    {
        public static readonly Color Window = Color.FromArgb(246, 247, 249);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceHover = Color.FromArgb(220, 230, 241);
        public static readonly Color SurfacePressed = Color.FromArgb(202, 216, 231);
        public static readonly Color Border = Color.FromArgb(210, 214, 220);
        public static readonly Color Text = Color.FromArgb(31, 35, 40);
        public static readonly Color TextMuted = Color.FromArgb(87, 96, 106);
        public static readonly Color Accent = Color.FromArgb(0, 120, 212);
        public static readonly Color AccentHover = Color.FromArgb(0, 95, 170);
        public static readonly Color AccentPressed = Color.FromArgb(0, 73, 132);
        public static readonly Color Disabled = Color.FromArgb(205, 210, 216);
        public static readonly Color DisabledBorder = Color.FromArgb(164, 171, 179);
        public static readonly Color DisabledText = Color.FromArgb(108, 115, 123);
    }

    internal static class ModernTheme
    {
        private const int DwmwaWindowCornerPreference = 33;
        private const int DwmwcpRound = 2;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        public static void Apply(Form form)
        {
            form.BackColor = ThemeColors.Window;
            form.ForeColor = ThemeColors.Text;

            form.HandleCreated -= ApplyWindowChrome;
            form.HandleCreated += ApplyWindowChrome;
            ApplyWindowChrome(form, EventArgs.Empty);

            ApplyControls(form.Controls);
        }

        private static void ApplyWindowChrome(object? sender, EventArgs e)
        {
            if (sender is not Form form || !form.IsHandleCreated)
            {
                return;
            }

            try
            {
                int preference = DwmwcpRound;
                DwmSetWindowAttribute(
                    form.Handle,
                    DwmwaWindowCornerPreference,
                    ref preference,
                    sizeof(int));
            }
            catch (DllNotFoundException)
            {
                // Older Windows versions keep the standard frame.
            }
            catch (EntryPointNotFoundException)
            {
                // Older Windows versions keep the standard frame.
            }
        }

        private static void ApplyControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyControl(control);

                if (control.HasChildren)
                {
                    ApplyControls(control.Controls);
                }
            }
        }

        private static void ApplyControl(Control control)
        {
            control.ForeColor = ThemeColors.Text;

            switch (control)
            {
                case Button button:
                    ApplyButton(button);
                    break;

                case TextBoxBase textBox:
                    textBox.BackColor = ThemeColors.Surface;
                    textBox.ForeColor = ThemeColors.Text;
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = ThemeColors.Surface;
                    comboBox.ForeColor = ThemeColors.Text;
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = ThemeColors.Surface;
                    numericUpDown.ForeColor = ThemeColors.Text;
                    break;

                case GroupBox groupBox:
                    groupBox.BackColor = ThemeColors.Window;
                    groupBox.ForeColor = ThemeColors.Text;
                    break;

                case TabControl tabControl:
                    ApplyTabControl(tabControl);
                    break;

                case TabPage tabPage:
                    tabPage.BackColor = ThemeColors.Window;
                    tabPage.ForeColor = ThemeColors.Text;
                    tabPage.UseVisualStyleBackColor = false;
                    break;

                case MenuStrip menuStrip:
                    menuStrip.BackColor = ThemeColors.Surface;
                    menuStrip.ForeColor = ThemeColors.Text;
                    menuStrip.Renderer = new ModernToolStripRenderer();
                    break;

                case StatusStrip statusStrip:
                    statusStrip.BackColor = ThemeColors.Surface;
                    statusStrip.ForeColor = ThemeColors.Text;
                    statusStrip.SizingGrip = false;
                    statusStrip.Renderer = new ModernToolStripRenderer();
                    break;

                case ToolStrip toolStrip:
                    toolStrip.BackColor = ThemeColors.Surface;
                    toolStrip.ForeColor = ThemeColors.Text;
                    toolStrip.Renderer = new ModernToolStripRenderer();
                    break;

                case Panel panel:
                    panel.BackColor = ThemeColors.Window;
                    break;

                case CheckedListBox checkedListBox:
                    checkedListBox.BackColor = ThemeColors.Surface;
                    checkedListBox.ForeColor = ThemeColors.Text;
                    checkedListBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListBox listBox:
                    listBox.BackColor = ThemeColors.Surface;
                    listBox.ForeColor = ThemeColors.Text;
                    listBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListView listView:
                    listView.BackColor = ThemeColors.Surface;
                    listView.ForeColor = ThemeColors.Text;
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case DataGridView dataGrid:
                    ApplyDataGrid(dataGrid);
                    break;
            }
        }

        private static void ApplyButton(Button button)
        {
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ThemeColors.SurfaceHover;
            button.FlatAppearance.MouseDownBackColor = ThemeColors.SurfacePressed;
            button.EnabledChanged -= UpdateButtonState;
            button.EnabledChanged += UpdateButtonState;
            button.GotFocus -= UpdateButtonFocus;
            button.GotFocus += UpdateButtonFocus;
            button.LostFocus -= UpdateButtonFocus;
            button.LostFocus += UpdateButtonFocus;
            UpdateButtonAppearance(button);
        }

        private static void UpdateButtonFocus(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                UpdateButtonAppearance(button);
            }
        }

        private static void UpdateButtonState(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                UpdateButtonAppearance(button);
            }
        }

        private static void UpdateButtonAppearance(Button button)
        {
            if (button.Enabled)
            {
                button.BackColor = ThemeColors.Surface;
                button.ForeColor = ThemeColors.Text;
                button.FlatAppearance.BorderColor = button.Focused
                    ? ThemeColors.Accent
                    : ThemeColors.Border;
                button.FlatAppearance.BorderSize = button.Focused ? 2 : 1;
                button.Cursor = Cursors.Hand;
                return;
            }

            button.BackColor = ThemeColors.Disabled;
            button.ForeColor = ThemeColors.DisabledText;
            button.FlatAppearance.BorderColor = ThemeColors.DisabledBorder;
            button.FlatAppearance.BorderSize = 1;
            button.Cursor = Cursors.Default;
        }

        private static void ApplyTabControl(TabControl tabControl)
        {
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Normal;
            tabControl.DrawItem -= DrawTab;
            tabControl.DrawItem += DrawTab;
        }

        private static void DrawTab(object? sender, DrawItemEventArgs e)
        {
            if (sender is not TabControl tabControl || e.Index < 0)
            {
                return;
            }

            Rectangle bounds = e.Bounds;
            bool selected = e.Index == tabControl.SelectedIndex;

            using SolidBrush background = new(selected ? ThemeColors.Surface : ThemeColors.Window);
            e.Graphics.FillRectangle(background, bounds);

            if (selected)
            {
                using SolidBrush accent = new(ThemeColors.Accent);
                e.Graphics.FillRectangle(accent, bounds.Left + 8, bounds.Bottom - 3, bounds.Width - 16, 3);
            }

            TextRenderer.DrawText(
                e.Graphics,
                tabControl.TabPages[e.Index].Text,
                tabControl.Font,
                bounds,
                selected ? ThemeColors.Text : ThemeColors.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void ApplyDataGrid(DataGridView dataGrid)
        {
            dataGrid.BackgroundColor = ThemeColors.Window;
            dataGrid.BorderStyle = BorderStyle.None;
            dataGrid.GridColor = ThemeColors.Border;
            dataGrid.EnableHeadersVisualStyles = false;
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = ThemeColors.Surface;
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = ThemeColors.Text;
            dataGrid.DefaultCellStyle.BackColor = ThemeColors.Surface;
            dataGrid.DefaultCellStyle.ForeColor = ThemeColors.Text;
            dataGrid.DefaultCellStyle.SelectionBackColor = ThemeColors.SurfaceHover;
            dataGrid.DefaultCellStyle.SelectionForeColor = ThemeColors.Text;
        }
    }

    internal sealed class ModernToolStripRenderer : ToolStripProfessionalRenderer
    {
        public ModernToolStripRenderer()
            : base(new ModernColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ThemeColors.TextMuted;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            Rectangle checkArea = new(e.ImageRectangle.X - 2, e.ImageRectangle.Y - 2, 18, 18);
            using SolidBrush background = new(ThemeColors.Accent);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(background, checkArea);

            using Pen check = new(Color.White, 2F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            e.Graphics.DrawLines(check,
            [
                new Point(checkArea.Left + 4, checkArea.Top + 9),
                new Point(checkArea.Left + 8, checkArea.Top + 13),
                new Point(checkArea.Left + 14, checkArea.Top + 5)
            ]);
        }
    }

    internal sealed class ModernColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => ThemeColors.Surface;
        public override Color ToolStripGradientMiddle => ThemeColors.Surface;
        public override Color ToolStripGradientEnd => ThemeColors.Surface;
        public override Color MenuStripGradientBegin => ThemeColors.Surface;
        public override Color MenuStripGradientEnd => ThemeColors.Surface;
        public override Color StatusStripGradientBegin => ThemeColors.Surface;
        public override Color StatusStripGradientEnd => ThemeColors.Surface;
        public override Color ToolStripDropDownBackground => ThemeColors.Surface;
        public override Color ImageMarginGradientBegin => ThemeColors.Surface;
        public override Color ImageMarginGradientMiddle => ThemeColors.Surface;
        public override Color ImageMarginGradientEnd => ThemeColors.Surface;
        public override Color MenuItemSelected => ThemeColors.SurfaceHover;
        public override Color MenuItemSelectedGradientBegin => ThemeColors.SurfaceHover;
        public override Color MenuItemSelectedGradientEnd => ThemeColors.SurfaceHover;
        public override Color MenuItemPressedGradientBegin => ThemeColors.SurfaceHover;
        public override Color MenuItemPressedGradientMiddle => ThemeColors.SurfaceHover;
        public override Color MenuItemPressedGradientEnd => ThemeColors.SurfaceHover;
        public override Color MenuItemBorder => ThemeColors.Border;
        public override Color SeparatorDark => ThemeColors.Border;
        public override Color SeparatorLight => ThemeColors.Surface;
        public override Color ToolStripBorder => ThemeColors.Border;
    }
}
