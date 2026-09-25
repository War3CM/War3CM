using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Phanmemwar3.Forms
{
    public static class GraphicsUtils
    {
        public static GraphicsPath GetRoundedRectangle(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (bounds.Width <= 0 || bounds.Height <= 0) return path;
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top left arc
            path.AddArc(arc, 180, 90);

            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawDiscordLogo(Graphics g, Rectangle bounds, bool isHovered)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = 20;
            int h = 15;
            float x = bounds.X + (bounds.Width - w) / 2f;
            float y = bounds.Y + (bounds.Height - h) / 2f;

            Color iconColor = isHovered ? Color.White : Color.FromArgb(88, 101, 242);
            using var brush = new SolidBrush(iconColor);

            using var path = new GraphicsPath();
            path.AddBezier(x + 2, y + 2, x + 6, y, x + 14, y, x + 18, y + 2);
            path.AddBezier(x + 18, y + 2, x + 20, y + 5, x + 19, y + 11, x + 16, y + 14);
            path.AddBezier(x + 16, y + 14, x + 14, y + 12.5f, x + 12, y + 13.5f, x + 11.5f, y + 13.5f);
            path.AddBezier(x + 11.5f, y + 13.5f, x + 10.5f, y + 12.5f, x + 9.5f, y + 12.5f, x + 8.5f, y + 13.5f);
            path.AddBezier(x + 8.5f, y + 13.5f, x + 8, y + 13.5f, x + 6, y + 12.5f, x + 4, y + 14);
            path.AddBezier(x + 4, y + 14, x + 1, y + 11, x + 0, y + 5, x + 2, y + 2);
            path.CloseFigure();
            g.FillPath(brush, path);

            Color eyeColor = isHovered ? Color.FromArgb(88, 101, 242) : Color.FromArgb(20, 30, 45);
            using var eyeBrush = new SolidBrush(eyeColor);
            g.FillEllipse(eyeBrush, x + 5f, y + 5.5f, 3.2f, 4f);
            g.FillEllipse(eyeBrush, x + 11.8f, y + 5.5f, 3.2f, 4f);
        }

        public static void DrawYouTubeLogo(Graphics g, Rectangle bounds, bool isHovered)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = 22;
            int h = 15;
            float x = bounds.X + (bounds.Width - w) / 2f;
            float y = bounds.Y + (bounds.Height - h) / 2f;

            Color badgeColor = isHovered ? Color.White : Color.FromArgb(255, 0, 0);
            Color playColor = isHovered ? Color.FromArgb(220, 20, 20) : Color.White;

            using var badgeBrush = new SolidBrush(badgeColor);
            using var playBrush = new SolidBrush(playColor);

            using var path = GetRoundedRectangle(new Rectangle((int)x, (int)y, w, h), 4);
            g.FillPath(badgeBrush, path);

            PointF[] triangle = new PointF[]
            {
                new PointF(x + 8.5f, y + 4f),
                new PointF(x + 15f, y + 7.5f),
                new PointF(x + 8.5f, y + 11f)
            };
            g.FillPolygon(playBrush, triangle);
        }
    }

    public class ModernTitleBar : Panel
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private readonly Form _parentForm;
        private readonly Label _lblTitle;
        private readonly Button _btnClose;
        private readonly Button _btnMax;
        private readonly Button _btnMin;

        public ModernTitleBar(Form parentForm, string title, bool showMin = true, bool showMax = true)
        {
            _parentForm = parentForm;
            this.Dock = DockStyle.Top;
            this.Height = 34;
            this.BackColor = Color.FromArgb(11, 15, 23);

            // Window Icon & Title
            var iconBox = new PictureBox
            {
                Location = new Point(12, 9),
                Size = new Size(16, 16),
                BackColor = Color.Transparent
            };
            iconBox.Paint += (s, e) =>
            {
                var icon = _parentForm.Icon ?? AppIcons.GetAppIcon();
                if (icon != null)
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.DrawIcon(icon, new Rectangle(0, 0, 16, 16));
                }
                else
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using var b = new SolidBrush(Color.FromArgb(50, 160, 240));
                    e.Graphics.FillRectangle(b, 0, 0, 6, 6);
                    e.Graphics.FillRectangle(b, 9, 0, 6, 6);
                    e.Graphics.FillRectangle(b, 0, 9, 6, 6);
                    e.Graphics.FillRectangle(b, 9, 9, 6, 6);
                }
            };
            this.Controls.Add(iconBox);

            _lblTitle = new Label
            {
                Text = title,
                Location = new Point(34, 8),
                AutoSize = true,
                UseMnemonic = false,
                ForeColor = Color.FromArgb(200, 215, 235),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            this.Controls.Add(_lblTitle);

            // Control Buttons
            int right = 0;
            _btnClose = CreateTitleButton("✕", Color.FromArgb(220, 50, 50), () => _parentForm.Close());
            _btnClose.Location = new Point(this.Width - 45, 0);
            _btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Controls.Add(_btnClose);
            right += 45;

            if (showMax)
            {
                _btnMax = CreateTitleButton("⬜", Color.FromArgb(30, 45, 65), () =>
                {
                    _parentForm.WindowState = _parentForm.WindowState == FormWindowState.Maximized
                        ? FormWindowState.Normal
                        : FormWindowState.Maximized;
                });
                _btnMax.Location = new Point(this.Width - right - 40, 0);
                _btnMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                this.Controls.Add(_btnMax);
                right += 40;
            }

            if (showMin)
            {
                _btnMin = CreateTitleButton("—", Color.FromArgb(30, 45, 65), () =>
                {
                    _parentForm.WindowState = FormWindowState.Minimized;
                });
                _btnMin.Location = new Point(this.Width - right - 40, 0);
                _btnMin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                this.Controls.Add(_btnMin);
            }

            this.Resize += (s, e) => {
                _btnClose.Location = new Point(Width - 46, 0);
                if (_btnMax != null) _btnMax.Location = new Point(Width - 86, 0);
                if (_btnMin != null) _btnMin.Location = new Point(Width - (showMax ? 126 : 86), 0);
            };
            this.MouseDown += OnMouseDownDrag;
            _lblTitle.MouseDown += OnMouseDownDrag;
            iconBox.MouseDown += OnMouseDownDrag;
        }

        private Button CreateTitleButton(string text, Color hoverColor, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(text == "✕" ? 44 : 38, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(170, 185, 205),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hoverColor;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(hoverColor.R / 2, hoverColor.G / 2, hoverColor.B / 2);
            btn.Click += (s, e) => onClick();
            return btn;
        }

        private void OnMouseDownDrag(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(_parentForm.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }

    public class ModernButton : Button
    {
        public int BorderRadius { get; set; } = 6;
        public Color BackColorNormal { get; set; } = Color.FromArgb(22, 34, 50);
        public Color BackColorHover { get; set; } = Color.FromArgb(32, 48, 70);
        public Color BackColorPressed { get; set; } = Color.FromArgb(16, 26, 40);
        public Color? GradientEndColor { get; set; } = null;
        public Color? GradientEndColorHover { get; set; } = null;
        public Color BorderColor { get; set; } = Color.FromArgb(38, 55, 80);
        public Image? ButtonIcon { get; set; } = null;
        public Action<Graphics, Rectangle, bool>? IconPainter { get; set; } = null;

        private bool _isHovered;
        private bool _isPressed;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            this.ForeColor = Color.White;
            this.BackColor = Color.FromArgb(22, 34, 50);
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color parentBg = Parent?.BackColor ?? BackColorNormal;
            using var b = new SolidBrush(parentBg);
            pevent.Graphics.FillRectangle(b, ClientRectangle);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color parentBg = Parent?.BackColor ?? BackColorNormal;
            using (var bgBrush = new SolidBrush(parentBg))
            {
                e.Graphics.FillRectangle(bgBrush, ClientRectangle);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using var path = GraphicsUtils.GetRoundedRectangle(rect, BorderRadius);

            Color c1 = _isPressed ? BackColorPressed : (_isHovered ? BackColorHover : BackColorNormal);
            Color? c2 = _isHovered && GradientEndColorHover.HasValue ? GradientEndColorHover : GradientEndColor;

            if (c2.HasValue)
            {
                using var brush = new LinearGradientBrush(rect, c1, c2.Value, LinearGradientMode.Vertical);
                e.Graphics.FillPath(brush, path);
            }
            else
            {
                using var brush = new SolidBrush(c1);
                e.Graphics.FillPath(brush, path);
            }

            if (BorderColor != Color.Transparent)
            {
                using var pen = new Pen(_isHovered ? Color.FromArgb(Math.Min(255, BorderColor.R + 40), Math.Min(255, BorderColor.G + 40), Math.Min(255, BorderColor.B + 40)) : BorderColor, 1);
                e.Graphics.DrawPath(pen, path);
            }

            if (IconPainter != null)
            {
                IconPainter(e.Graphics, rect, _isHovered);
                if (Focused && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(rect, -4, -4), Color.White, c1);
                return;
            }

            // Draw Icon + Text
            int textX = 0;
            if (ButtonIcon != null)
            {
                int iconY = (this.Height - ButtonIcon.Height) / 2;
                int iconX = 14;
                e.Graphics.DrawImage(ButtonIcon, iconX, iconY, ButtonIcon.Width, ButtonIcon.Height);
                textX = iconX + ButtonIcon.Width + 8;
            }

            var textRect = ButtonIcon != null
                ? new Rectangle(textX, 0, this.Width - textX - 8, this.Height)
                : new Rectangle(2, 0, Math.Max(0, this.Width - 4), this.Height);
            if (textRect.Width > 0)
            {
                var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding |
                    (ButtonIcon == null ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);
                if (this.Width >= 55) flags |= TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(e.Graphics, Text, Font, textRect,
                    Enabled ? ForeColor : Color.FromArgb(115, 132, 155), flags);
            }
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(rect, -4, -4), Color.White, c1);
        }

        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    }

    public class ModernTextBox : Panel
    {
        private readonly TextBox _innerBox;
        private bool _isFocused = false;

        public string TextContent
        {
            get => _innerBox.Text;
            set => _innerBox.Text = value;
        }

        public TextBox InnerTextBox => _innerBox;

        public ModernTextBox()
        {
            this.Height = 34;
            this.BackColor = Color.FromArgb(16, 23, 36);
            this.Padding = new Padding(10, 6, 10, 5);
            this.DoubleBuffered = true;

            _innerBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(16, 23, 36),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Dock = DockStyle.Fill
            };

            _innerBox.GotFocus += (s, e) => { _isFocused = true; Invalidate(); };
            _innerBox.LostFocus += (s, e) => { _isFocused = false; Invalidate(); };

            this.Controls.Add(_innerBox);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using var path = GraphicsUtils.GetRoundedRectangle(rect, 4);

            using var bgBrush = new SolidBrush(_innerBox.BackColor);
            e.Graphics.FillPath(bgBrush, path);

            Color border = _isFocused ? Color.FromArgb(30, 136, 229) : Color.FromArgb(30, 48, 71);
            using var pen = new Pen(border, _isFocused ? 1.5f : 1f);
            e.Graphics.DrawPath(pen, path);
        }
    }

    public class ModernComboBox : ComboBox
    {
        public Func<object?, string>? DisplayText { get; set; }
        public ModernComboBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ItemHeight = 26;
            this.BackColor = Color.FromArgb(16, 23, 36);
            this.ForeColor = Color.White;
            this.FlatStyle = FlatStyle.Flat;
            this.Font = new Font("Segoe UI", 9.5f);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? Color.FromArgb(24, 52, 86) : Color.FromArgb(16, 23, 36);
            using (var b = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }

            string text = DisplayText?.Invoke(this.Items[e.Index]) ?? this.Items[e.Index]?.ToString() ?? "";
            bool isEdit = (e.State & DrawItemState.ComboBoxEdit) != 0;
            Rectangle rect;
            if (isEdit)
            {
                int textLeft = 7;
                int textRight = Math.Max(textLeft, this.Width - 30);
                rect = new Rectangle(textLeft, 0, textRight - textLeft, this.Height);
            }
            else
            {
                rect = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 12), e.Bounds.Height);
            }

            var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.Left;
            if (text.Length > 4)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            TextRenderer.DrawText(e.Graphics, text, Font, rect, Enabled ? ForeColor : Color.FromArgb(145, 161, 182), flags);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // WM_PAINT = 0x000F
            if (m.Msg == 0x000F && Width > 29 && Height > 4)
            {
                using var g = Graphics.FromHwnd(this.Handle);

                // 1. Cover the native light-theme arrow button completely (from y = 0 to Height)
                var arrowRect = new Rectangle(Math.Max(0, Width - 28), 0, 28, Height);
                using (var arrowBrush = new SolidBrush(Color.FromArgb(22, 34, 50)))
                {
                    g.FillRectangle(arrowBrush, arrowRect);
                }
                using (var divider = new Pen(Color.FromArgb(45, 65, 90)))
                {
                    g.DrawLine(divider, arrowRect.Left, 3, arrowRect.Left, Height - 3);
                }
                int cx = this.Width - 14;
                int cy = this.Height / 2 - 1;
                using (var arrowPen = new Pen(Color.FromArgb(80, 160, 240), 2))
                {
                    g.DrawLine(arrowPen, cx - 4, cy - 2, cx, cy + 2);
                    g.DrawLine(arrowPen, cx, cy + 2, cx + 4, cy - 2);
                }

                // 2. Draw crisp 1px dark border covering all outer perimeter pixels and corners
                using (var pen = new Pen(Color.FromArgb(45, 65, 90), 1))
                {
                    g.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }
    }

    public class ModernCheckBox : Control
    {
        private bool _checked = false;
        public bool Checked
        {
            get => _checked;
            set { _checked = value; Invalidate(); }
        }

        public event EventHandler? CheckedChanged;

        public ModernCheckBox()
        {
            this.Height = 28;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 9.5f);
            this.ForeColor = Color.FromArgb(230, 240, 255);
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            AccessibleRole = AccessibleRole.CheckButton;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color parentBg = Parent?.BackColor ?? Color.FromArgb(22, 32, 48);
            using var b = new SolidBrush(parentBg);
            pevent.Graphics.FillRectangle(b, ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color parentBg = Parent?.BackColor ?? Color.FromArgb(22, 32, 48);
            using (var b = new SolidBrush(parentBg))
            {
                e.Graphics.FillRectangle(b, ClientRectangle);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Box: 18x18
            int boxY = (this.Height - 18) / 2;
            var boxRect = new Rectangle(1, boxY, 18, 18);
            using var path = GraphicsUtils.GetRoundedRectangle(boxRect, 4);

            if (_checked)
            {
                using var b = new SolidBrush(Color.FromArgb(14, 117, 234));
                e.Graphics.FillPath(b, path);

                // White checkmark
                using var p = new Pen(Color.White, 2.2f);
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                e.Graphics.DrawLine(p, boxRect.X + 4, boxRect.Y + 9, boxRect.X + 8, boxRect.Y + 13);
                e.Graphics.DrawLine(p, boxRect.X + 8, boxRect.Y + 13, boxRect.X + 14, boxRect.Y + 5);
            }
            else
            {
                using var b = new SolidBrush(Color.FromArgb(16, 23, 36));
                e.Graphics.FillPath(b, path);
                using var p = new Pen(Color.FromArgb(42, 62, 88), 1.5f);
                e.Graphics.DrawPath(p, path);
            }

            // Text
            using var sf = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            var textRect = new Rectangle(26, 0, this.Width - 26, this.Height);
            using var tb = new SolidBrush(this.ForeColor);
            e.Graphics.DrawString(this.Text, this.Font, tb, textRect, sf);
            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(24, 2, Math.Max(0, Width - 26), Math.Max(0, Height - 4)));
        }
    }
}

namespace Phanmemwar3.Forms
{
    public class DarkCardPanel : Panel
    {
        public int BorderRadius { get; set; } = 9;
        public Color BorderColor { get; set; } = Color.FromArgb(43, 63, 88);

        public DarkCardPanel()
        {
            BackColor = Color.FromArgb(22, 32, 48);
            DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            Color parentBg = Parent?.BackColor ?? Color.FromArgb(12, 18, 29);
            using var b = new SolidBrush(parentBg);
            pevent.Graphics.FillRectangle(b, ClientRectangle);

            if (BorderRadius > 0)
            {
                using var path = GraphicsUtils.GetRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), BorderRadius);
                using var cardBrush = new SolidBrush(BackColor);
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pevent.Graphics.FillPath(cardBrush, path);
            }
            else
            {
                using var cardBrush = new SolidBrush(BackColor);
                pevent.Graphics.FillRectangle(cardBrush, ClientRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (BorderRadius > 0)
            {
                using var path = GraphicsUtils.GetRoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), BorderRadius);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawPath(pen, path);
            }
            else
            {
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}
