using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text;
using ATRACTool_Reloaded.ModernUI;

namespace ATRACTool_Reloaded.src.Controls
{


    public partial class CustomTrackBar : Control
    {
        private bool isScrolling = false;
        private bool isHovered = false;
        private DateTime lastScrollTime = DateTime.MinValue;
        private const int scrollInterval = 16;

        private int minimum = 0;
        private int maximum = 100;
        private int value = 0;
        private int trackThickness = 4;
        private bool showTicks = true;
        private int tickFrequency = 10;
        private int tickSize = 5;
        private Color tickColor = ThemeColors.TextMuted;
        private Color trackColor = ThemeColors.Border;
        private Color thumbColor = ThemeColors.Accent;
        private Color backgroundColor = ThemeColors.Window;
        private string overlayText = string.Empty;
        private int overlayTextTop = 0;
        private Size overlayTextSize = Size.Empty;
        private Font? overlayTextFont;

        private ToolTip toolTip = new();
        private DateTime lastToolTipUpdate = DateTime.MinValue;
        private const int toolTipUpdateInterval = 100; // ツールチップの更新間隔 (ms)

        private bool showLPCSamples = true;

        [Category("Appearance")]
        [Description("Specifies the color of the trackbar's bar.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color TrackColor
        {
            get => trackColor;
            set
            {
                if (trackColor != value)
                {
                    trackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Specifies the color of the trackbar's thumb.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color ThumbColor
        {
            get => thumbColor;
            set
            {
                if (thumbColor != value)
                {
                    thumbColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Specifies the color of the knob when it is being dragged.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DraggedThumbColor { get; set; } = Color.DarkRed;
        private bool isDragging = false; // ドラッグ中を追跡

        [Category("Appearance")]
        [Description("Specify the width of the knob.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ThumbWidth { get; set; } = 10; // デフォルトの横幅

        [Category("Appearance")]
        [Description("Specify the height of the knob.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ThumbHeight
        {
            get => thumbHeight;
            set
            {
                thumbHeight = Math.Min(value, Height); // コントロールの高さを超えない
                Invalidate(); // 再描画
            }
        }
        private int thumbHeight = 20; // 初期値

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ThumbSize { get; set; } = 10;

        public enum ThumbShape
        {
            Circle,
            Rectangle,
            RoundedRectangle,
            UpArrow,     // 上矢印
            DownArrow    // 下矢印
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ThumbShape Shape { get; set; } = ThumbShape.Circle;

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TrackThickness
        {
            get => trackThickness;
            set
            {
                if (trackThickness != value)
                {
                    trackThickness = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    Invalidate();
                }
            }
        }

        public enum TickPosition
        {
            None,       // 目盛りを表示しない
            Above,      // ゲージの上に表示
            Below,      // ゲージの下に表示
            Both        // ゲージの上下に表示
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public TickPosition TickPos { get; set; } = TickPosition.Below;

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string OverlayText
        {
            get => overlayText;
            set
            {
                if (overlayText != value)
                {
                    overlayText = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int OverlayTextTop
        {
            get => overlayTextTop;
            set
            {
                if (overlayTextTop != value)
                {
                    overlayTextTop = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Size OverlayTextSize
        {
            get => overlayTextSize;
            set
            {
                if (overlayTextSize != value)
                {
                    overlayTextSize = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font OverlayTextFont
        {
            get => overlayTextFont ?? Font;
            set
            {
                if (overlayTextFont != value)
                {
                    overlayTextFont = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowTicks
        {
            get => showTicks;
            set
            {
                if (showTicks != value)
                {
                    showTicks = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowLPCSamples
        {
            get => showLPCSamples;
            set
            {
                if (showLPCSamples != value)
                {
                    showLPCSamples = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TickFrequency
        {
            get => tickFrequency;
            set
            {
                if (tickFrequency != value)
                {
                    tickFrequency = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TickSize
        {
            get => tickSize;
            set
            {
                if (tickSize != value)
                {
                    tickSize = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color TickColor
        {
            get => tickColor;
            set
            {
                if (tickColor != value)
                {
                    tickColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Minimum
        {
            get => minimum;
            set
            {
                if (value >= maximum)
                    throw new ArgumentException("Minimum must be less than Maximum");
                minimum = value;
                if (this.value < minimum)
                    this.value = minimum;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Maximum
        {
            get => maximum;
            set
            {
                if (value <= minimum)
                    throw new ArgumentException("Maximum must be greater than Minimum");
                maximum = value;
                if (this.value > maximum)
                    this.value = maximum;
                Invalidate();
            }
        }

        public event EventHandler Scroll = null!;

        protected virtual void OnScroll(EventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Value
        {
            get => value;
            set
            {
                int clampedValue = Math.Clamp(value, minimum, maximum);
                if (this.value != clampedValue)
                {
                    this.value = clampedValue;
                    Invalidate();
                    OnValueChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public event EventHandler ValueChanged = null!;

        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        public CustomTrackBar()
        {
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable,
                true);
            UpdateStyles();
            TabStop = true;
            Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Common.Generic.IsLPCStreamingReloaded) return;

            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(Parent?.BackColor ?? BackgroundColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle trackRect = GetTrackRectangle();
            Rectangle thumbRect = GetThumbRectangle();
            Color baseTrackColor = Enabled ? TrackColor : ThemeColors.Disabled;
            Color activeColor = Enabled ? ThumbColor : ThemeColors.TextMuted;

            using (GraphicsPath trackPath = GetRoundedRectangle(trackRect, Math.Max(1, Math.Min(trackRect.Width, trackRect.Height) / 2)))
            using (SolidBrush trackBackground = new(baseTrackColor))
            {
                g.FillPath(trackBackground, trackPath);
            }

            Rectangle activeRect = GetActiveTrackRectangle(trackRect, thumbRect);
            if (activeRect.Width > 0 && activeRect.Height > 0)
            {
                using GraphicsPath activePath = GetRoundedRectangle(
                    activeRect,
                    Math.Max(1, Math.Min(activeRect.Width, activeRect.Height) / 2));
                using SolidBrush activeBrush = new(Color.FromArgb(175, activeColor));
                g.FillPath(activeBrush, activePath);
            }

            DrawTicks(g, trackRect, baseTrackColor, activeColor);
            DrawThumb(g, thumbRect, activeColor);
            DrawOverlayText(g);

            if (Focused && ShowFocusCues)
            {
                Rectangle focusRect = ClientRectangle;
                focusRect.Inflate(-1, -1);
                using Pen focusPen = new(ThemeColors.Accent) { DashStyle = DashStyle.Dot };
                g.DrawRectangle(focusPen, focusRect);
            }
        }

        private void DrawOverlayText(Graphics graphics)
        {
            if (string.IsNullOrEmpty(OverlayText))
            {
                return;
            }

            int range = Math.Max(1, Maximum - Minimum);
            float ratio = (float)(Value - Minimum) / range;
            Size textSize = OverlayTextSize.IsEmpty
                ? TextRenderer.MeasureText(OverlayText, OverlayTextFont)
                : OverlayTextSize;
            int x = (int)Math.Round(ratio * Width) - (textSize.Width / 2);
            x = Math.Clamp(x, 0, Math.Max(0, Width - textSize.Width));

            Rectangle textBounds = new(x, OverlayTextTop, textSize.Width, textSize.Height);
            TextRenderer.DrawText(
                graphics,
                OverlayText,
                OverlayTextFont,
                textBounds,
                ModernTheme.IsDarkMode ? ControlPaint.Dark(ThemeColors.TextMuted, 0.35F) : ThemeColors.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        private Rectangle GetTrackRectangle()
        {
            int thickness = Math.Max(2, TrackThickness);
            int margin = Math.Max(ThumbWidth, 8) / 2;

            return Orientation == Orientation.Horizontal
                ? new Rectangle(margin, (Height - thickness) / 2, Math.Max(1, Width - (margin * 2)), thickness)
                : new Rectangle((Width - thickness) / 2, margin, thickness, Math.Max(1, Height - (margin * 2)));
        }

        private Rectangle GetThumbRectangle()
        {
            int width = Math.Max(4, Math.Min(ThumbWidth, Width));
            int height = Math.Max(4, Math.Min(ThumbHeight, Height));
            float ratio = (float)(Value - Minimum) / (Maximum - Minimum);

            if (Orientation == Orientation.Horizontal)
            {
                int x = (int)Math.Round(ratio * Math.Max(0, Width - width));
                return new Rectangle(x, (Height - height) / 2, width, height);
            }

            int y = (int)Math.Round(ratio * Math.Max(0, Height - height));
            return new Rectangle((Width - width) / 2, y, width, height);
        }

        private Rectangle GetActiveTrackRectangle(Rectangle trackRect, Rectangle thumbRect)
        {
            if (Orientation == Orientation.Horizontal)
            {
                int end = Math.Min(trackRect.Right, thumbRect.Left + (thumbRect.Width / 2));
                return new Rectangle(trackRect.Left, trackRect.Top, Math.Max(1, end - trackRect.Left), trackRect.Height);
            }

            int bottom = Math.Min(trackRect.Bottom, thumbRect.Top + (thumbRect.Height / 2));
            return new Rectangle(trackRect.Left, trackRect.Top, trackRect.Width, Math.Max(1, bottom - trackRect.Top));
        }

        private void DrawTicks(Graphics graphics, Rectangle trackRect, Color baseTrackColor, Color activeTrackColor)
        {
            if (!ShowTicks || TickPos == TickPosition.None || TickFrequency <= 0)
            {
                return;
            }

            long range = (long)Maximum - Minimum;
            if (range <= 0)
            {
                return;
            }

            Color disabledTickColor = ModernTheme.IsDarkMode
                ? ControlPaint.Dark(ThemeColors.DisabledText, 0.2F)
                : ControlPaint.Dark(ThemeColors.TextMuted, 0.2F);

            void DrawTick(int tickValue)
            {
                float ratio = (float)((long)tickValue - Minimum) / range;
                bool isActiveTick = tickValue <= Value;
                Color tickPenColor = Enabled
                    ? (isActiveTick ? Color.FromArgb(175, activeTrackColor) : baseTrackColor)
                    : disabledTickColor;
                using Pen pen = new(tickPenColor, 1F);

                if (Orientation == Orientation.Horizontal)
                {
                    int x = trackRect.Left + (int)Math.Round(ratio * trackRect.Width);
                    if (TickPos is TickPosition.Above or TickPosition.Both)
                    {
                        graphics.DrawLine(pen, x, trackRect.Top - 3, x, trackRect.Top - 3 - TickSize);
                    }
                    if (TickPos is TickPosition.Below or TickPosition.Both)
                    {
                        graphics.DrawLine(pen, x, trackRect.Bottom + 3, x, trackRect.Bottom + 3 + TickSize);
                    }
                }
                else
                {
                    int y = trackRect.Top + (int)Math.Round(ratio * trackRect.Height);
                    if (TickPos is TickPosition.Above or TickPosition.Both)
                    {
                        graphics.DrawLine(pen, trackRect.Left - 3, y, trackRect.Left - 3 - TickSize, y);
                    }
                    if (TickPos is TickPosition.Below or TickPosition.Both)
                    {
                        graphics.DrawLine(pen, trackRect.Right + 3, y, trackRect.Right + 3 + TickSize, y);
                    }
                }
            }

            int? lastTickValue = null;
            for (long tickValue = Minimum; tickValue <= Maximum; tickValue += TickFrequency)
            {
                int currentTickValue = (int)tickValue;
                DrawTick(currentTickValue);
                lastTickValue = currentTickValue;

                if (tickValue > Maximum - (long)TickFrequency)
                {
                    break;
                }
            }

            if (!lastTickValue.HasValue || lastTickValue.Value != Maximum)
            {
                DrawTick(Maximum);
            }
        }

        private void DrawThumb(Graphics graphics, Rectangle thumbRect, Color color)
        {
            Color thumbFill = isDragging
                ? DraggedThumbColor
                : isHovered || Focused
                    ? ControlPaint.Dark(color, 0.12F)
                    : color;

            if (isHovered || isDragging || Focused)
            {
                Rectangle halo = thumbRect;
                halo.Inflate(4, 4);
                using SolidBrush haloBrush = new(Color.FromArgb(45, color));
                graphics.FillEllipse(haloBrush, halo);
            }

            using SolidBrush fill = new(thumbFill);
            using Pen border = new(ControlPaint.Dark(thumbFill, 0.12F), 1F);

            switch (Shape)
            {
                case ThumbShape.Circle:
                    graphics.FillEllipse(fill, thumbRect);
                    graphics.DrawEllipse(border, thumbRect);
                    break;

                case ThumbShape.Rectangle:
                    graphics.FillRectangle(fill, thumbRect);
                    graphics.DrawRectangle(border, thumbRect);
                    break;

                case ThumbShape.RoundedRectangle:
                    using (GraphicsPath thumbPath = GetRoundedRectangle(
                        thumbRect,
                        Math.Max(2, Math.Min(thumbRect.Width, thumbRect.Height) / 3)))
                    {
                        graphics.FillPath(fill, thumbPath);
                        graphics.DrawPath(border, thumbPath);
                    }
                    break;

                case ThumbShape.UpArrow:
                    DrawArrow(graphics, thumbRect, thumbFill, true);
                    break;

                case ThumbShape.DownArrow:
                    DrawArrow(graphics, thumbRect, thumbFill, false);
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            Focus();
            isScrolling = true;
            isDragging = true;
            UpdateThumbPosition(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isScrolling = false;
            isDragging = false;
            toolTip.Hide(this);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isScrolling)
            {
                StringBuilder str;
                if (ShowLPCSamples)
                {
                    str = new($"Value: {Value}" + "\n" + $"Total Samples: {Common.Generic.LPCTotalSamples}" + "\n" + $"Current Samples: {FormLPC.FormLPCInstance.SampleLabel}", 128);
                }
                else
                {
                    str = new($"Value: {Value}", 64);
                }
                
                UpdateThumbPosition(e.Location);

                if ((DateTime.Now - lastToolTipUpdate).TotalMilliseconds >= toolTipUpdateInterval)
                {
                    lastToolTipUpdate = DateTime.Now;
                    
                    //string toolTipText = $"Value: {Value}";
                    toolTip.Show(str.ToString(), this, e.Location.X, e.Location.Y - 20);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down
                || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int delta = e.KeyCode is Keys.Right or Keys.Up ? 1
                : e.KeyCode is Keys.Left or Keys.Down ? -1
                : 0;

            if (delta == 0)
            {
                return;
            }

            SetValueFromInput(Value + delta);
            e.Handled = true;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetValueFromInput(Value + Math.Sign(e.Delta));
        }

        private void SetValueFromInput(int newValue)
        {
            int clamped = Math.Max(Minimum, Math.Min(Maximum, newValue));
            if (clamped == Value)
            {
                return;
            }

            Value = clamped;
            OnScroll(EventArgs.Empty);
        }

        private void UpdateValueFromMouse(Point mouseLocation)
        {
            int newValue;
            if (Orientation == Orientation.Horizontal)
            {
                newValue = minimum + (int)((float)(mouseLocation.X) / Width * (maximum - minimum));
            }
            else
            {
                newValue = minimum + (int)((float)(mouseLocation.Y) / Height * (maximum - minimum));
            }

            newValue = Math.Max(minimum, Math.Min(maximum, newValue));
            if (newValue != value)
            {
                Value = newValue;
            }
        }

        private void UpdateThumbPosition(Point mousePosition)
        {
            int newValue = Orientation == Orientation.Horizontal
                ? (int)((float)mousePosition.X / Width * (Maximum - Minimum)) + Minimum
                : (int)((float)mousePosition.Y / Height * (Maximum - Minimum)) + Minimum;

            newValue = Math.Max(Minimum, Math.Min(Maximum, newValue));

            if (newValue != Value)
            {
                Value = newValue;
                OnScrollEvent();
                Invalidate();
            }
        }

        private void OnScrollEvent()
        {
            if ((DateTime.Now - lastScrollTime).TotalMilliseconds < scrollInterval)
            {
                return;
            }
            lastScrollTime = DateTime.Now;

            OnScroll(EventArgs.Empty);
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new();
            if (rect.Width <= 2 || rect.Height <= 2)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = Math.Max(2, Math.Min(cornerRadius * 2, Math.Min(rect.Width, rect.Height)));
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawArrow(Graphics g, Rectangle rect, Color color, bool isUp)
        {
            Point[] points;

            if (isUp)
            {
                points =
                [
                    new Point(rect.X + rect.Width / 2, rect.Y),          // 上の頂点
                    new Point(rect.X, rect.Bottom),                     // 左下
                    new Point(rect.Right, rect.Bottom)                  // 右下
                ];
            }
            else
            {
                points =
                [
                    
                    new Point(rect.X, rect.Y),                          // 左上
                    new Point(rect.Right, rect.Y),                      // 右上
                    new Point(rect.X + rect.Width / 2, rect.Bottom)     // 下の頂点
                ];
            }

            using (Brush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, points);
            }
        }
    }
}
