using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text;

namespace ATRACTool_Reloaded.src.Controls
{


    public partial class CustomTrackBar : Control
    {
        private bool isScrolling = false;
        private DateTime lastScrollTime = DateTime.MinValue;
        private const int scrollInterval = 16;

        private SolidBrush trackBrush = null!;
        private SolidBrush thumbBrush = null!;
        private Pen tickPen = null!;
        private List<Point> tickPositions = null!;
        private int cachedValue = -1;

        private int minimum = 0;
        private int maximum = 100;
        private int value = 0;
        private int trackThickness = 4;
        private bool showTicks = true;
        private int tickFrequency = 10;
        private int tickSize = 5;
        private Color tickColor = Color.Black;
        private Color trackColor = Color.Gray;
        private Color thumbColor = Color.Red;
        private Color backgroundColor = Color.White;

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
                    trackBrush?.Dispose();
                    trackBrush = null!; // 再生成をトリガー
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
                    thumbBrush?.Dispose();
                    thumbBrush = null!; // 再生成をトリガー
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
                    //CalculateTickPositions(); // 目盛りの再計算
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
                    tickPen?.Dispose();
                    tickPen = null!; // 再生成をトリガー
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
                if (this.value != value)
                {
                    if (value < minimum || value > maximum)
                    {
                        throw new ArgumentOutOfRangeException(nameof(Value), "Value must be between Minimum and Maximum");
                    }
                    this.value = value;

                    // 再描画をトリガー
                    Invalidate();
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
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();
            CalculateTickPositions();
        }

        private void InitializeResources()
        {
            if (trackBrush == null) trackBrush = new SolidBrush(TrackColor);
            if (thumbBrush == null) thumbBrush = new SolidBrush(ThumbColor);
            if (tickPen == null) tickPen = new Pen(TickColor, 1);
        }

        private void CalculateTickPositions()
        {
            if (!ShowTicks) return;

            tickPositions = new List<Point>();

            if (ShowTicks)
            {
                for (int i = Minimum; i <= Maximum; i += TickFrequency)
                {
                    int position = (int)((float)(i - Minimum) / (Maximum - Minimum) * Width);
                    tickPositions.Add(new Point(position, Height / 2));
                }
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            if (cachedValue == Value) return;
            if (Common.Generic.IsLPCStreamingReloaded) return;

            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.Clear(BackgroundColor);
            InitializeResources();

            // トラックの描画
            //Rectangle trackRect = new Rectangle(0, (Height - 4) / 2, Width, 4);
            Rectangle trackRect = new Rectangle(0, (Height - TrackThickness) / 2, Width, TrackThickness);
            g.FillRectangle(trackBrush, trackRect);

            // つまみの位置とサイズを計算
            int thumbHeight = Math.Min(ThumbHeight, Height); // 最大でコントロールの高さ
            int thumbWidth = (Height - thumbHeight) / 2;         // 中央に配置
            int thumbY2 = Shape == ThumbShape.UpArrow || Shape == ThumbShape.DownArrow
        ? (Height - thumbHeight) / 2 // 矢印の場合も中央揃え
        : (Height - ThumbHeight) / 2;

            // 目盛りの描画
            if (ShowTicks && tickPositions != null)
            {
                foreach (var tick in tickPositions)
                {
                    g.DrawLine(tickPen, tick.X, trackRect.Bottom + 2, tick.X, trackRect.Bottom + 6);
                }
            }

            Rectangle thumbRect;

            if (Orientation == Orientation.Horizontal)
            {
                int thumbX = (int)((float)(value - minimum) / (maximum - minimum) * (Width - ThumbWidth));

                trackRect = new Rectangle(
                    0,
                    (Height - TrackThickness) / 2,
                    Width,
                    TrackThickness
                );

                // 横幅と高さを反映したつまみの位置とサイズ

                thumbRect = new Rectangle(
                    thumbX,
                    (Height - thumbHeight) / 2,
                    ThumbWidth,
                    ThumbHeight
                );

                // Draw Ticks (目盛りを描画)
                if (ShowTicks)
                {
                    using (Pen tickPen = new(TickColor, 1))
                    {
                        for (int i = minimum; i <= maximum; i += TickFrequency)
                        {
                            // 目盛り位置を計算
                            int x = (int)((float)(i - minimum) / (maximum - minimum) * Width);

                            // 目盛りの描画位置を調整
                            int adjustedTickY = trackRect.Bottom + 2;
                            if (Shape == ThumbShape.UpArrow)
                            {
                                adjustedTickY = Height - (ThumbHeight / 2) - TickSize - 2;
                            }

                            // 目盛りを描画
                            if (TickPos == TickPosition.Below || TickPos == TickPosition.Both)
                            {
                                g.DrawLine(tickPen, x, adjustedTickY, x, adjustedTickY + TickSize);
                            }
                            if (TickPos == TickPosition.Above || TickPos == TickPosition.Both)
                            {
                                g.DrawLine(tickPen, x, trackRect.Top - TickSize - 2, x, trackRect.Top - 2);
                            }
                        }
                    }
                }
            }
            else
            {
                trackRect = new Rectangle(
                    (Width - TrackThickness) / 2,
                    0,
                    TrackThickness,
                    Height
                );

                // 横幅と高さを反映したつまみの位置とサイズ
                int thumbY = (int)((float)(value - minimum) / (maximum - minimum) * (Height - ThumbHeight));
                thumbRect = new Rectangle(
                    (Width - ThumbWidth) / 2,
                    thumbY,
                    ThumbWidth,
                    ThumbHeight
                );

                // Draw Ticks (目盛りを描画)
                if (ShowTicks)
                {
                    using Pen tickPen = new(TickColor, 1);
                    for (int i = minimum; i <= maximum; i += TickFrequency)
                    {
                        int y = (int)((float)(i - minimum) / (maximum - minimum) * Height);

                        // 左右に目盛りを描画
                        if (TickPos == TickPosition.Above || TickPos == TickPosition.Both)
                        {
                            g.DrawLine(tickPen, trackRect.Left - TickSize - 2, y, trackRect.Left - 2, y);
                        }

                        if (TickPos == TickPosition.Below || TickPos == TickPosition.Both)
                        {
                            g.DrawLine(tickPen, trackRect.Right + 2, y, trackRect.Right + 2 + TickSize, y);
                        }
                    }
                }
            }

            // Draw Track
            using (Brush trackBrush = new SolidBrush(TrackColor))
            {
                g.FillRectangle(trackBrush, trackRect);
            }

            // Draw Thumb
            switch (Shape)
            {
                case ThumbShape.Circle:
                    {
                        using Brush sb = new SolidBrush(ThumbColor);
                        g.FillEllipse(sb, thumbRect);
                        using (Brush thumbBrush = new SolidBrush(isDragging ? DraggedThumbColor : ThumbColor))
                        {
                            g.FillRectangle(thumbBrush, thumbRect);
                        }
                        break;
                    }
                case ThumbShape.Rectangle:
                    {
                        using Brush sb = new SolidBrush(ThumbColor);
                        g.FillRectangle(sb, thumbRect);
                        using (Brush thumbBrush = new SolidBrush(isDragging ? DraggedThumbColor : ThumbColor))
                        {
                            g.FillRectangle(thumbBrush, thumbRect);
                        }
                        break;
                    }
                case ThumbShape.RoundedRectangle:
                    using (GraphicsPath thumbPath = GetRoundedRectangle(thumbRect, Math.Min(ThumbWidth, ThumbHeight) / 3))
                    {
                        g.FillPath(new SolidBrush(ThumbColor), thumbPath);
                    }
                    using (Brush thumbBrush = new SolidBrush(isDragging ? DraggedThumbColor : ThumbColor))
                    {
                        g.FillRectangle(thumbBrush, thumbRect);
                    }
                    break;
                case ThumbShape.UpArrow:
                    DrawArrow(g, thumbRect, isDragging ? DraggedThumbColor : ThumbColor, true);
                    break;

                case ThumbShape.DownArrow:
                    DrawArrow(g, thumbRect, isDragging ? DraggedThumbColor : ThumbColor, false);
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            //UpdateValueFromMouse(e.Location);
            isScrolling = true;
            isDragging = true;
            //Invalidate(); // 再描画
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
            /*if (e.Button == MouseButtons.Left)
            {
                UpdateValueFromMouse(e.Location);
            }*/
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
                int oldThumbX = (int)((float)(Value - Minimum) / (Maximum - Minimum) * (Width - ThumbWidth));
                Value = newValue;
                int newThumbX = (int)((float)(Value - Minimum) / (Maximum - Minimum) * (Width - ThumbWidth));
                OnScrollEvent();
                // 古い位置と新しい位置のみ再描画
                Invalidate(new Rectangle(Math.Min(oldThumbX, newThumbX), (Height - ThumbHeight) / 2, ThumbWidth, ThumbHeight));
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
            path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
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
