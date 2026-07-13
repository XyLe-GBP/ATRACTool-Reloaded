using System.Drawing.Drawing2D;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ATRACTool_Reloaded.ModernUI
{
    internal static class ThemeColors
    {
        public static Color Window => ModernTheme.IsDarkMode ? Color.FromArgb(32, 33, 36) : Color.FromArgb(246, 247, 249);
        public static Color Surface => ModernTheme.IsDarkMode ? Color.FromArgb(43, 45, 48) : Color.White;
        public static Color SurfaceHover => ModernTheme.IsDarkMode ? Color.FromArgb(59, 64, 70) : Color.FromArgb(220, 230, 241);
        public static Color SurfacePressed => ModernTheme.IsDarkMode ? Color.FromArgb(72, 78, 86) : Color.FromArgb(202, 216, 231);
        public static Color Border => ModernTheme.IsDarkMode ? Color.FromArgb(88, 92, 99) : Color.FromArgb(210, 214, 220);
        public static Color Text => ModernTheme.IsDarkMode ? Color.FromArgb(242, 243, 245) : Color.FromArgb(31, 35, 40);
        public static Color TextMuted => ModernTheme.IsDarkMode ? Color.FromArgb(176, 182, 190) : Color.FromArgb(87, 96, 106);
        public static Color Accent => Color.FromArgb(0, 120, 212);
        public static Color AccentHover => ModernTheme.IsDarkMode ? Color.FromArgb(48, 156, 255) : Color.FromArgb(0, 95, 170);
        public static Color AccentPressed => ModernTheme.IsDarkMode ? Color.FromArgb(25, 134, 235) : Color.FromArgb(0, 73, 132);
        public static Color Disabled => ModernTheme.IsDarkMode ? Color.FromArgb(63, 66, 72) : Color.FromArgb(205, 210, 216);
        public static Color DisabledBorder => ModernTheme.IsDarkMode ? Color.FromArgb(82, 86, 94) : Color.FromArgb(164, 171, 179);
        public static Color DisabledText => ModernTheme.IsDarkMode ? Color.FromArgb(170, 176, 186) : Color.FromArgb(108, 115, 123);
    }

    internal static class ModernTheme
    {
        public enum AppThemeMode
        {
            Light,
            Dark,
        }

        private const int DwmwaWindowCornerPreference = 33;
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwcpRound = 2;
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const int DateTimePickerButtonWidth = 24;
        private static readonly ConditionalWeakTable<TabControl, ModernTabControlPainter> TabControlPainters = [];
        private static readonly ConditionalWeakTable<TabPage, ModernTabPagePainter> TabPagePainters = [];
        private static readonly ConditionalWeakTable<DateTimePicker, ModernDateTimePickerPainter> DateTimePickerPainters = [];

        static ModernTheme()
        {
            SystemEvents.UserPreferenceChanged += (_, _) => RefreshOpenWindows();
        }

        public static bool UseSystemTheme
        {
            get
            {
                return Common.Utils.GetBool("Theme_FollowSystem", false);
            }
        }

        public static AppThemeMode SelectedThemeMode
        {
            get
            {
                string mode = Common.Utils.GetString("Theme_Mode", nameof(AppThemeMode.Light));
                return string.Equals(mode, nameof(AppThemeMode.Dark), StringComparison.OrdinalIgnoreCase)
                    ? AppThemeMode.Dark
                    : AppThemeMode.Light;
            }
        }

        public static bool IsDarkMode => UseSystemTheme ? IsSystemDarkMode : SelectedThemeMode == AppThemeMode.Dark;

        public static bool IsSystemDarkMode
        {
            get
            {
                try
                {
                    using RegistryKey? key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
                    return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attribute,
            ref int attributeValue,
            int attributeSize);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string? subAppName, string? subIdList);

        [DllImport("user32.dll")]
        private static extern IntPtr BeginPaint(IntPtr hwnd, out PaintStruct paint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EndPaint(IntPtr hwnd, ref PaintStruct paint);

        [StructLayout(LayoutKind.Sequential)]
        private struct PaintStruct
        {
            public IntPtr Hdc;
            [MarshalAs(UnmanagedType.Bool)]
            public bool Erase;
            public Rectangle Paint;
            [MarshalAs(UnmanagedType.Bool)]
            public bool Restore;
            [MarshalAs(UnmanagedType.Bool)]
            public bool IncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] Reserved;
        }

        public static void Apply(Form form)
        {
            EnableDoubleBuffering(form);
            form.BackColor = ThemeColors.Window;
            form.ForeColor = ThemeColors.Text;

            form.HandleCreated -= ApplyWindowChrome;
            form.HandleCreated += ApplyWindowChrome;
            ApplyWindowChrome(form, EventArgs.Empty);

            ApplyControls(form.Controls);
        }

        public static void RefreshOpenWindows()
        {
            ModernWpfTheme.RefreshResources();

            foreach (Form form in Application.OpenForms)
            {
                if (form.IsDisposed || form.Disposing)
                {
                    continue;
                }

                try
                {
                    if (form.InvokeRequired)
                    {
                        form.BeginInvoke(new Action(() => Apply(form)));
                    }
                    else
                    {
                        Apply(form);
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
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

                int darkMode = IsDarkMode ? 1 : 0;
                DwmSetWindowAttribute(
                    form.Handle,
                    DwmwaUseImmersiveDarkMode,
                    ref darkMode,
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
            control.EnabledChanged -= UpdateControlEnabledState;
            control.EnabledChanged += UpdateControlEnabledState;
            ApplyControlEnabledState(control);

            switch (control)
            {
                case Button button:
                    ApplyButton(button);
                    break;

                case TextBoxBase textBox:
                    textBox.BackColor = IsEffectivelyEnabled(textBox) ? ThemeColors.Surface : ThemeColors.Disabled;
                    textBox.ForeColor = IsEffectivelyEnabled(textBox) ? ThemeColors.Text : ThemeColors.DisabledText;
                    break;

                case ComboBox comboBox:
                    ApplyComboBox(comboBox);
                    break;

                case DateTimePicker dateTimePicker:
                    ApplyDateTimePicker(dateTimePicker);
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = IsEffectivelyEnabled(numericUpDown) ? ThemeColors.Surface : ThemeColors.Disabled;
                    numericUpDown.ForeColor = IsEffectivelyEnabled(numericUpDown) ? ThemeColors.Text : ThemeColors.DisabledText;
                    break;

                case LinkLabel linkLabel:
                    ApplyLinkLabel(linkLabel);
                    break;

                case Label label:
                    ApplyLabel(label);
                    break;

                case GroupBox groupBox:
                    ApplyGroupBox(groupBox);
                    break;

                case TabControl tabControl:
                    ApplyTabControl(tabControl);
                    break;

                case TabPage tabPage:
                    ApplyTabPage(tabPage);
                    break;

                case MenuStrip menuStrip:
                    menuStrip.BackColor = ThemeColors.Surface;
                    menuStrip.Renderer = new ModernToolStripRenderer();
                    ApplyToolStripItems(menuStrip.Items);
                    break;

                case StatusStrip statusStrip:
                    statusStrip.BackColor = ThemeColors.Surface;
                    statusStrip.SizingGrip = false;
                    statusStrip.Renderer = new ModernToolStripRenderer();
                    ApplyToolStripItems(statusStrip.Items);
                    break;

                case ToolStrip toolStrip:
                    toolStrip.BackColor = ThemeColors.Surface;
                    toolStrip.Renderer = new ModernToolStripRenderer();
                    ApplyToolStripItems(toolStrip.Items);
                    break;

                case Panel panel:
                    panel.BackColor = ThemeColors.Window;
                    break;

                case CheckedListBox checkedListBox:
                    checkedListBox.BackColor = IsEffectivelyEnabled(checkedListBox) ? ThemeColors.Surface : ThemeColors.Disabled;
                    checkedListBox.ForeColor = IsEffectivelyEnabled(checkedListBox) ? ThemeColors.Text : ThemeColors.DisabledText;
                    checkedListBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListBox listBox:
                    listBox.BackColor = IsEffectivelyEnabled(listBox) ? ThemeColors.Surface : ThemeColors.Disabled;
                    listBox.ForeColor = IsEffectivelyEnabled(listBox) ? ThemeColors.Text : ThemeColors.DisabledText;
                    listBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListView listView:
                    listView.BackColor = IsEffectivelyEnabled(listView) ? ThemeColors.Surface : ThemeColors.Disabled;
                    listView.ForeColor = IsEffectivelyEnabled(listView) ? ThemeColors.Text : ThemeColors.DisabledText;
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case DataGridView dataGrid:
                    ApplyDataGrid(dataGrid);
                    break;
            }
        }

        private static void UpdateControlEnabledState(object? sender, EventArgs e)
        {
            if (sender is Control control)
            {
                ApplyControlTree(control);
            }
        }

        private static void ApplyControlEnabledState(Control control)
        {
            if (control is Button)
            {
                return;
            }

            bool enabled = IsEffectivelyEnabled(control);

            control.ForeColor = enabled
                ? ThemeColors.Text
                : ThemeColors.DisabledText;

            switch (control)
            {
                case TextBoxBase textBox:
                    textBox.BackColor = enabled
                        ? ThemeColors.Surface
                        : ThemeColors.Disabled;
                    break;

                case ComboBox comboBox:
                    ApplyComboBox(comboBox);
                    break;

                case DateTimePicker dateTimePicker:
                    ApplyDateTimePicker(dateTimePicker);
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = enabled
                        ? ThemeColors.Surface
                        : ThemeColors.Disabled;
                    break;

                case ListControl listControl:
                    listControl.BackColor = enabled
                        ? ThemeColors.Surface
                        : ThemeColors.Disabled;
                    break;

                case Label label:
                    ApplyLabel(label);
                    break;

                case CheckBox checkBox:
                    ApplyCheckBox(checkBox);
                    break;

                case RadioButton radioButton:
                    ApplyRadioButton(radioButton);
                    break;
            }
        }

        private static void ApplyControlTree(Control root)
        {
            if (root is Button button)
            {
                UpdateButtonAppearance(button);
            }
            else
            {
                ApplyControlEnabledState(root);
            }

            foreach (Control child in root.Controls)
            {
                ApplyControlTree(child);
            }
        }

        private static bool IsEffectivelyEnabled(Control control)
        {
            for (Control? current = control; current != null; current = current.Parent)
            {
                if (!current.Enabled)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ApplyComboBox(ComboBox comboBox)
        {
            bool enabled = IsEffectivelyEnabled(comboBox);

            comboBox.BackColor = enabled ? ThemeColors.Surface : ThemeColors.Disabled;
            comboBox.ForeColor = enabled ? ThemeColors.Text : ThemeColors.DisabledText;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.DrawMode = DrawMode.Normal;
            comboBox.Invalidate();
        }

        private static void ApplyDateTimePicker(DateTimePicker dateTimePicker)
        {
            bool enabled = IsEffectivelyEnabled(dateTimePicker);

            dateTimePicker.BackColor = enabled ? ThemeColors.Surface : ThemeColors.Disabled;
            dateTimePicker.ForeColor = enabled ? ThemeColors.Text : ThemeColors.DisabledText;

            ApplyControlWindowTheme(dateTimePicker);
            DateTimePickerPainters.GetValue(dateTimePicker, _ => new ModernDateTimePickerPainter()).Attach(dateTimePicker);
            dateTimePicker.Invalidate();
        }

        private static void ApplyControlWindowTheme(Control control)
        {
            control.HandleCreated -= ApplyControlWindowThemeOnHandleCreated;
            control.HandleCreated += ApplyControlWindowThemeOnHandleCreated;
            ApplyControlWindowThemeOnHandleCreated(control, EventArgs.Empty);
        }

        private static void ApplyControlWindowThemeOnHandleCreated(object? sender, EventArgs e)
        {
            if (sender is not Control control || !control.IsHandleCreated)
            {
                return;
            }

            try
            {
                SetWindowTheme(control.Handle, IsDarkMode ? "DarkMode_Explorer" : "Explorer", null);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void ApplyToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = item.Enabled
                    ? ThemeColors.Text
                    : ThemeColors.DisabledText;

                item.BackColor = ThemeColors.Surface;
                item.EnabledChanged -= UpdateToolStripItemEnabledState;
                item.EnabledChanged += UpdateToolStripItemEnabledState;

                if (item is ToolStripDropDownItem dropDownItem)
                {
                    dropDownItem.DropDown.BackColor = ThemeColors.Surface;
                    dropDownItem.DropDown.ForeColor = ThemeColors.Text;
                    dropDownItem.DropDown.Renderer = new ModernToolStripRenderer();
                    ApplyToolStripItems(dropDownItem.DropDownItems);
                }
            }
        }

        private static void UpdateToolStripItemEnabledState(object? sender, EventArgs e)
        {
            if (sender is not ToolStripItem item)
            {
                return;
            }

            item.ForeColor = item.Enabled
                ? ThemeColors.Text
                : ThemeColors.DisabledText;
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
            if (IsEffectivelyEnabled(button))
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

        private static void ApplyLinkLabel(LinkLabel linkLabel)
        {
            linkLabel.BackColor = linkLabel.Parent?.BackColor ?? ThemeColors.Window;
            linkLabel.ForeColor = IsEffectivelyEnabled(linkLabel) ? ThemeColors.Text : ThemeColors.DisabledText;
            linkLabel.LinkColor = ThemeColors.AccentHover;
            linkLabel.ActiveLinkColor = ThemeColors.AccentPressed;
            linkLabel.VisitedLinkColor = ThemeColors.Accent;
            linkLabel.DisabledLinkColor = ThemeColors.DisabledText;
        }

        private static void ApplyLabel(Label label)
        {
            label.BackColor = label.Parent?.BackColor ?? ThemeColors.Window;
            label.ForeColor = IsEffectivelyEnabled(label) ? ThemeColors.Text : ThemeColors.DisabledText;
            label.Paint -= DrawLabel;
            label.Paint += DrawLabel;
        }

        private static void DrawLabel(object? sender, PaintEventArgs e)
        {
            if (sender is not Label label)
            {
                return;
            }

            Color backColor = label.Parent?.BackColor ?? ThemeColors.Window;
            Color textColor = IsEffectivelyEnabled(label) ? ThemeColors.Text : ThemeColors.DisabledText;
            Rectangle bounds = label.ClientRectangle;

            using SolidBrush background = new(backColor);
            e.Graphics.FillRectangle(background, bounds);

            TextRenderer.DrawText(
                e.Graphics,
                label.Text,
                label.Font,
                bounds,
                textColor,
                GetLabelTextFlags(label));
        }

        private static TextFormatFlags GetLabelTextFlags(Label label)
        {
            TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;

            flags |= label.TextAlign switch
            {
                ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
                _ => TextFormatFlags.Left,
            };

            flags |= label.TextAlign switch
            {
                ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter,
                ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight => TextFormatFlags.Bottom,
                _ => TextFormatFlags.Top,
            };

            if (!label.AutoSize)
            {
                flags |= TextFormatFlags.WordBreak;
            }

            return flags;
        }

        private static void ApplyGroupBox(GroupBox groupBox)
        {
            EnableDoubleBuffering(groupBox);
            groupBox.BackColor = ThemeColors.Window;
            groupBox.ForeColor = IsEffectivelyEnabled(groupBox) ? ThemeColors.Text : ThemeColors.DisabledText;
            groupBox.Paint -= DrawGroupBox;
            groupBox.Paint += DrawGroupBox;
        }

        private static void DrawGroupBox(object? sender, PaintEventArgs e)
        {
            if (sender is not GroupBox groupBox)
            {
                return;
            }

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix;
            Size textSize = TextRenderer.MeasureText(groupBox.Text, groupBox.Font);
            Rectangle border = new(0, textSize.Height / 2, groupBox.Width - 1, groupBox.Height - textSize.Height / 2 - 1);
            Rectangle textBounds = new(8, 0, Math.Min(textSize.Width + 4, Math.Max(0, groupBox.Width - 16)), textSize.Height);

            using Pen borderPen = new(ThemeColors.Border);
            e.Graphics.DrawRectangle(borderPen, border);

            using SolidBrush textBack = new(groupBox.BackColor);
            e.Graphics.FillRectangle(textBack, textBounds);
            TextRenderer.DrawText(
                e.Graphics,
                groupBox.Text,
                groupBox.Font,
                textBounds,
                IsEffectivelyEnabled(groupBox) ? ThemeColors.Text : ThemeColors.DisabledText,
                flags);
        }

        private static void ApplyCheckBox(CheckBox checkBox)
        {
            EnsureCheckRadioSize(checkBox);
            checkBox.UseVisualStyleBackColor = false;
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.BackColor = checkBox.Parent?.BackColor ?? ThemeColors.Window;
            checkBox.ForeColor = IsEffectivelyEnabled(checkBox) ? ThemeColors.Text : ThemeColors.DisabledText;
            checkBox.FlatAppearance.BorderColor = IsEffectivelyEnabled(checkBox) ? ThemeColors.Border : ThemeColors.DisabledBorder;
            checkBox.FlatAppearance.CheckedBackColor = ThemeColors.Accent;
            checkBox.Paint -= DrawCheckBox;
            checkBox.Paint += DrawCheckBox;
            checkBox.CheckedChanged -= InvalidateControl;
            checkBox.CheckedChanged += InvalidateControl;
        }

        private static void DrawCheckBox(object? sender, PaintEventArgs e)
        {
            if (sender is not CheckBox checkBox)
            {
                return;
            }

            DrawCheckRadioBase(
                e.Graphics,
                checkBox,
                checkBox.Text,
                checkBox.Font,
                checkBox.Checked,
                isRadioButton: false);
        }

        private static void ApplyRadioButton(RadioButton radioButton)
        {
            EnsureCheckRadioSize(radioButton);
            radioButton.UseVisualStyleBackColor = false;
            radioButton.FlatStyle = FlatStyle.Flat;
            radioButton.BackColor = radioButton.Parent?.BackColor ?? ThemeColors.Window;
            radioButton.ForeColor = IsEffectivelyEnabled(radioButton) ? ThemeColors.Text : ThemeColors.DisabledText;
            radioButton.FlatAppearance.BorderColor = IsEffectivelyEnabled(radioButton) ? ThemeColors.Border : ThemeColors.DisabledBorder;
            radioButton.FlatAppearance.CheckedBackColor = ThemeColors.Accent;
            radioButton.Paint -= DrawRadioButton;
            radioButton.Paint += DrawRadioButton;
            radioButton.CheckedChanged -= InvalidateControl;
            radioButton.CheckedChanged += InvalidateControl;
        }

        private static void DrawRadioButton(object? sender, PaintEventArgs e)
        {
            if (sender is not RadioButton radioButton)
            {
                return;
            }

            DrawCheckRadioBase(
                e.Graphics,
                radioButton,
                radioButton.Text,
                radioButton.Font,
                radioButton.Checked,
                isRadioButton: true);
        }

        private static void DrawCheckRadioBase(Graphics graphics, Control control, string text, Font font, bool isChecked, bool isRadioButton)
        {
            bool enabled = IsEffectivelyEnabled(control);
            Color backColor = control.Parent?.BackColor ?? ThemeColors.Window;
            Color borderColor = enabled ? (isChecked ? ThemeColors.Accent : ThemeColors.Border) : ThemeColors.DisabledBorder;
            Color textColor = enabled ? ThemeColors.Text : ThemeColors.DisabledText;
            int glyphSize = 14;
            Rectangle glyphBounds = new(1, Math.Max(0, (control.Height - glyphSize) / 2), glyphSize, glyphSize);
            Rectangle textBounds = new(glyphBounds.Right + 6, 0, Math.Max(0, control.Width - glyphBounds.Right - 6), control.Height);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(backColor);

            using Pen borderPen = new(borderColor, 1.4F);
            using SolidBrush surfaceBrush = new(enabled ? ThemeColors.Surface : ThemeColors.Disabled);
            using SolidBrush accentBrush = new(enabled ? ThemeColors.Accent : ThemeColors.DisabledBorder);

            if (isRadioButton)
            {
                graphics.FillEllipse(surfaceBrush, glyphBounds);
                graphics.DrawEllipse(borderPen, glyphBounds);

                if (isChecked)
                {
                    Rectangle dotBounds = Rectangle.Inflate(glyphBounds, -4, -4);
                    graphics.FillEllipse(accentBrush, dotBounds);
                }
            }
            else
            {
                graphics.FillRectangle(surfaceBrush, glyphBounds);
                graphics.DrawRectangle(borderPen, glyphBounds);

                if (isChecked)
                {
                    graphics.FillRectangle(accentBrush, Rectangle.Inflate(glyphBounds, -1, -1));
                    using Pen checkPen = new(Color.White, 2F)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round,
                        LineJoin = LineJoin.Round
                    };
                    graphics.DrawLines(checkPen,
                    [
                        new Point(glyphBounds.Left + 3, glyphBounds.Top + 7),
                        new Point(glyphBounds.Left + 6, glyphBounds.Top + 10),
                        new Point(glyphBounds.Left + 11, glyphBounds.Top + 4)
                    ]);
                }
            }

            TextRenderer.DrawText(
                graphics,
                text,
                font,
                textBounds,
                textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
        }

        private static void EnsureCheckRadioSize(Control control)
        {
            if (!control.AutoSize || string.IsNullOrEmpty(control.Text))
            {
                return;
            }

            Size textSize = TextRenderer.MeasureText(
                control.Text,
                control.Font,
                Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int desiredWidth = 1 + 14 + 6 + textSize.Width + 2;
            int desiredHeight = Math.Max(control.Height, Math.Max(18, textSize.Height + 2));

            if (control.Width < desiredWidth || control.Height < desiredHeight)
            {
                control.Size = new Size(Math.Max(control.Width, desiredWidth), Math.Max(control.Height, desiredHeight));
            }
        }

        private static void InvalidateControl(object? sender, EventArgs e)
        {
            if (sender is Control control)
            {
                control.Invalidate();
            }
        }

        private static void ApplyTabControl(TabControl tabControl)
        {
            EnableDoubleBuffering(tabControl);
            tabControl.BackColor = ThemeColors.Window;
            tabControl.Appearance = TabAppearance.Normal;
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.SizeMode = TabSizeMode.Normal;
            tabControl.HandleCreated -= ApplyTabControlWindowTheme;
            tabControl.HandleCreated += ApplyTabControlWindowTheme;
            tabControl.DrawItem -= DrawTab;
            tabControl.DrawItem += DrawTab;
            ApplyTabControlWindowTheme(tabControl, EventArgs.Empty);
            TabControlPainters.GetValue(tabControl, _ => new ModernTabControlPainter()).Attach(tabControl);

            foreach (TabPage tabPage in tabControl.TabPages)
            {
                ApplyTabPage(tabPage);
            }
        }

        private static void ApplyTabPage(TabPage tabPage)
        {
            EnableDoubleBuffering(tabPage);
            tabPage.BackColor = ThemeColors.Window;
            tabPage.ForeColor = ThemeColors.Text;
            tabPage.UseVisualStyleBackColor = false;
            tabPage.BorderStyle = BorderStyle.None;
            tabPage.HandleCreated -= ApplyTabPageWindowTheme;
            tabPage.HandleCreated += ApplyTabPageWindowTheme;
            ApplyTabPageWindowTheme(tabPage, EventArgs.Empty);
            TabPagePainters.GetValue(tabPage, _ => new ModernTabPagePainter()).Attach(tabPage);
        }

        private static void EnableDoubleBuffering(Control control)
        {
            try
            {
                typeof(Control)
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(control, true);
            }
            catch
            {
            }
        }

        private static void ApplyTabControlWindowTheme(object? sender, EventArgs e)
        {
            if (sender is not TabControl tabControl || !tabControl.IsHandleCreated)
            {
                return;
            }

            try
            {
                SetWindowTheme(tabControl.Handle, string.Empty, string.Empty);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void ApplyTabPageWindowTheme(object? sender, EventArgs e)
        {
            if (sender is not TabPage tabPage || !tabPage.IsHandleCreated)
            {
                return;
            }

            try
            {
                SetWindowTheme(tabPage.Handle, string.Empty, string.Empty);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private sealed class ModernDateTimePickerPainter : NativeWindow
        {
            private const int WmEraseBkgnd = 0x0014;
            private const int WmPaint = 0x000F;
            private const int WmPrintClient = 0x0318;
            private DateTimePicker? dateTimePicker;

            public void Attach(DateTimePicker target)
            {
                if (ReferenceEquals(dateTimePicker, target))
                {
                    return;
                }

                if (dateTimePicker != null)
                {
                    dateTimePicker.HandleCreated -= DateTimePicker_HandleCreated;
                    dateTimePicker.HandleDestroyed -= DateTimePicker_HandleDestroyed;
                    ReleaseHandle();
                }

                dateTimePicker = target;
                dateTimePicker.HandleCreated += DateTimePicker_HandleCreated;
                dateTimePicker.HandleDestroyed += DateTimePicker_HandleDestroyed;

                if (dateTimePicker.IsHandleCreated)
                {
                    AssignHandle(dateTimePicker.Handle);
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (CanPaint())
                {
                    switch (m.Msg)
                    {
                        case WmEraseBkgnd:
                            DrawDateTimePickerBackground(m.WParam);
                            m.Result = new IntPtr(1);
                            return;

                        case WmPrintClient:
                            DrawDateTimePicker(m.WParam);
                            m.Result = IntPtr.Zero;
                            return;

                        case WmPaint:
                            PaintStruct paint;
                            IntPtr hdc = BeginPaint(Handle, out paint);
                            try
                            {
                                DrawDateTimePicker(hdc);
                            }
                            finally
                            {
                                EndPaint(Handle, ref paint);
                            }

                            m.Result = IntPtr.Zero;
                            return;
                    }
                }

                base.WndProc(ref m);
            }

            private void DateTimePicker_HandleCreated(object? sender, EventArgs e)
            {
                if (dateTimePicker?.IsHandleCreated == true)
                {
                    AssignHandle(dateTimePicker.Handle);
                }
            }

            private void DateTimePicker_HandleDestroyed(object? sender, EventArgs e)
            {
                ReleaseHandle();
            }

            private bool CanPaint()
            {
                return IsDarkMode
                    && dateTimePicker != null
                    && !dateTimePicker.IsDisposed
                    && dateTimePicker.IsHandleCreated;
            }

            private void DrawDateTimePickerBackground(IntPtr hdc)
            {
                if (dateTimePicker == null || hdc == IntPtr.Zero)
                {
                    return;
                }

                bool enabled = IsEffectivelyEnabled(dateTimePicker);
                using Graphics graphics = Graphics.FromHdc(hdc);
                using SolidBrush surface = new(enabled ? ThemeColors.Surface : ThemeColors.Disabled);
                graphics.FillRectangle(surface, dateTimePicker.ClientRectangle);
            }

            private void DrawDateTimePicker(IntPtr hdc)
            {
                if (dateTimePicker == null || hdc == IntPtr.Zero)
                {
                    return;
                }

                bool enabled = IsEffectivelyEnabled(dateTimePicker);
                Rectangle bounds = dateTimePicker.ClientRectangle;
                Color surfaceColor = enabled ? ThemeColors.Surface : ThemeColors.Disabled;
                Color textColor = enabled ? ThemeColors.Text : ThemeColors.DisabledText;
                Color borderColor = dateTimePicker.Focused && enabled ? ThemeColors.Accent : ThemeColors.Border;

                using Graphics graphics = Graphics.FromHdc(hdc);
                using SolidBrush surface = new(surfaceColor);
                using Pen border = new(borderColor);

                graphics.FillRectangle(surface, bounds);
                graphics.DrawRectangle(border, 0, 0, bounds.Width - 1, bounds.Height - 1);

                Rectangle buttonBounds = new(
                    Math.Max(bounds.Left, bounds.Right - DateTimePickerButtonWidth),
                    bounds.Top + 1,
                    Math.Min(DateTimePickerButtonWidth, bounds.Width),
                    Math.Max(0, bounds.Height - 2));

                using SolidBrush button = new(enabled ? ThemeColors.SurfaceHover : ThemeColors.Disabled);
                graphics.FillRectangle(button, buttonBounds);
                DrawDropDownArrow(graphics, buttonBounds, enabled ? ThemeColors.TextMuted : ThemeColors.DisabledText);

                Rectangle textBounds = new(
                    bounds.Left + 8,
                    bounds.Top + 1,
                    Math.Max(0, bounds.Width - buttonBounds.Width - 12),
                    Math.Max(0, bounds.Height - 2));

                TextRenderer.DrawText(
                    graphics,
                    dateTimePicker.Text,
                    dateTimePicker.Font,
                    textBounds,
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }

        private static void DrawDropDownArrow(Graphics graphics, Rectangle bounds, Color color)
        {
            int centerX = bounds.Left + bounds.Width / 2;
            int centerY = bounds.Top + bounds.Height / 2;

            Point[] points =
            [
                new(centerX - 4, centerY - 2),
                new(centerX + 4, centerY - 2),
                new(centerX, centerY + 3)
            ];

            using SolidBrush brush = new(color);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.FillPolygon(brush, points);
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

        private sealed class ModernTabControlPainter : NativeWindow
        {
            private const int WmEraseBkgnd = 0x0014;
            private const int WmPaint = 0x000F;
            private const int WmPrintClient = 0x0318;
            private TabControl? tabControl;

            public void Attach(TabControl control)
            {
                if (ReferenceEquals(tabControl, control))
                {
                    return;
                }

                if (tabControl != null)
                {
                    tabControl.HandleCreated -= TabControl_HandleCreated;
                    tabControl.HandleDestroyed -= TabControl_HandleDestroyed;
                }

                tabControl = control;
                tabControl.HandleCreated += TabControl_HandleCreated;
                tabControl.HandleDestroyed += TabControl_HandleDestroyed;

                if (tabControl.IsHandleCreated)
                {
                    AssignHandle(tabControl.Handle);
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmEraseBkgnd && TryEraseBackground(m.WParam))
                {
                    m.Result = 1;
                    return;
                }

                base.WndProc(ref m);

                if (m.Msg == WmPaint || m.Msg == WmPrintClient)
                {
                    DrawDarkFrame();
                }
            }

            private bool TryEraseBackground(IntPtr hdc)
            {
                if (!IsDarkMode || tabControl == null || tabControl.IsDisposed || !tabControl.IsHandleCreated)
                {
                    return false;
                }

                using Graphics graphics = Graphics.FromHdc(hdc);
                using SolidBrush brush = new(ThemeColors.Window);
                Rectangle client = tabControl.ClientRectangle;
                graphics.FillRectangle(brush, client);
                return true;
            }

            private void TabControl_HandleCreated(object? sender, EventArgs e)
            {
                if (tabControl?.IsHandleCreated == true)
                {
                    AssignHandle(tabControl.Handle);
                }
            }

            private void TabControl_HandleDestroyed(object? sender, EventArgs e)
            {
                ReleaseHandle();
            }

            private void DrawDarkFrame()
            {
                if (!IsDarkMode || tabControl == null || tabControl.IsDisposed || !tabControl.IsHandleCreated)
                {
                    return;
                }

                Rectangle client = tabControl.ClientRectangle;
                Rectangle display = tabControl.DisplayRectangle;

                if (client.Width <= 0 || client.Height <= 0 || display.Width <= 0 || display.Height <= 0)
                {
                    return;
                }

                using Graphics graphics = Graphics.FromHwnd(tabControl.Handle);
                using Pen borderPen = new(ThemeColors.Border);

                Rectangle border = display;
                border.Inflate(1, 1);
                border.Width = Math.Min(border.Width, client.Width - border.Left - 1);
                border.Height = Math.Min(border.Height, client.Height - border.Top - 1);

                if (border.Width > 0 && border.Height > 0)
                {
                    graphics.DrawRectangle(borderPen, border);
                }
            }

            private static void FillIfVisible(Graphics graphics, Brush brush, Rectangle rectangle)
            {
                if (rectangle.Width > 0 && rectangle.Height > 0)
                {
                    graphics.FillRectangle(brush, rectangle);
                }
            }
        }

        private sealed class ModernTabPagePainter : NativeWindow
        {
            private const int WmEraseBkgnd = 0x0014;
            private TabPage? tabPage;

            public void Attach(TabPage page)
            {
                if (ReferenceEquals(tabPage, page))
                {
                    return;
                }

                if (tabPage != null)
                {
                    tabPage.HandleCreated -= TabPage_HandleCreated;
                    tabPage.HandleDestroyed -= TabPage_HandleDestroyed;
                }

                tabPage = page;
                tabPage.HandleCreated += TabPage_HandleCreated;
                tabPage.HandleDestroyed += TabPage_HandleDestroyed;

                if (tabPage.IsHandleCreated)
                {
                    AssignHandle(tabPage.Handle);
                }
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmEraseBkgnd && TryEraseBackground(m.WParam))
                {
                    m.Result = 1;
                    return;
                }

                base.WndProc(ref m);
            }

            private void TabPage_HandleCreated(object? sender, EventArgs e)
            {
                if (tabPage?.IsHandleCreated == true)
                {
                    AssignHandle(tabPage.Handle);
                }
            }

            private void TabPage_HandleDestroyed(object? sender, EventArgs e)
            {
                ReleaseHandle();
            }

            private bool TryEraseBackground(IntPtr hdc)
            {
                if (!IsDarkMode || tabPage == null || tabPage.IsDisposed || !tabPage.IsHandleCreated)
                {
                    return false;
                }

                using Graphics graphics = Graphics.FromHdc(hdc);
                using SolidBrush brush = new(ThemeColors.Window);
                graphics.FillRectangle(brush, tabPage.ClientRectangle);
                return true;
            }
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

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled
                ? ThemeColors.Text
                : ThemeColors.DisabledText;
            base.OnRenderItemText(e);
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
