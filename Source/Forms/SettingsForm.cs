using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class SettingsForm : Form
    {
        private readonly ConfigManager _config;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));
        private readonly string _appDir;
        private ModernTextBox txtUserName;
        private ModernCheckBox chkMuteSaveValue;
        private ModernCheckBox chkLockMouse;
        private ModernCheckBox chkFixRatio;
        private ModernCheckBox chkWideScreen;
        private ModernCheckBox chkFastLoad;
        private ModernComboBox cboLanguage;
        private ModernButton btnSave;
        private ModernButton btnCancel;
        private ModernButton btnRestoreRegistry;

        public SettingsForm(ConfigManager config)
        {
            _config = config;
            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            InitializeComponent();
            LoadData();
        }

        private Image? LoadIcon(string name)
        {
            try
            {
                string p = Path.Combine(_appDir, "Resources", name);
                if (File.Exists(p)) return Image.FromFile(p);
                string p2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", name);
                if (File.Exists(p2)) return Image.FromFile(p2);
            }
            catch { }
            return null;
        }

        private void InitializeComponent()
        {
            Text = T("settingsTitle");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(510, 520);
            MinimumSize = new Size(410, 405);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(12, 18, 29);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            Shown += (s, e) =>
            {
                Rectangle area = Screen.FromControl(this).WorkingArea;
                MinimumSize = new Size(Math.Min(410, area.Width - 16), Math.Min(405, area.Height - 16));
                Size = new Size(Math.Min(Width, area.Width - 16), Math.Min(Height, area.Height - 16));
            };
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            Controls.Add(shell);
            var titleBar = new ModernTitleBar(this, T("settingsTitle"), showMin: false, showMax: false) { Dock = DockStyle.Fill };
            shell.Controls.Add(titleBar, 0, 0);
            var footer = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.FromArgb(18, 28, 43), Padding = new Padding(17, 10, 17, 10), ColumnCount = 2 };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            btnCancel = new ModernButton { Text = T("btnCancel"), Dock = DockStyle.Fill, Margin = new Padding(2, 0, 7, 0) };
            btnCancel.Click += (s, e) => Close();
            btnSave = new ModernButton { Text = T("btnSaveSettings"), Dock = DockStyle.Fill, Margin = new Padding(7, 0, 2, 0),
                BackColorNormal = Color.FromArgb(24, 120, 234), BackColorHover = Color.FromArgb(48, 148, 255),
                GradientEndColor = Color.FromArgb(12, 82, 189) };
            btnSave.Click += BtnSave_Click;
            footer.Controls.Add(btnCancel, 0, 0); footer.Controls.Add(btnSave, 1, 0);
            shell.Controls.Add(footer, 0, 2);

            var scroller = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(18, 16, 18, 10) };
            shell.Controls.Add(scroller, 0, 1);
            var content = new TableLayoutPanel { Dock = DockStyle.Top, Height = 442, ColumnCount = 1, RowCount = 11 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int h in new[] { 34, 42, 30, 42, 31, 34, 34, 34, 34, 34, 60 })
                content.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
            scroller.Controls.Add(content);
            Label Field(string key) => new Label { Text = T(key), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(174, 196, 220), AutoEllipsis = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            content.Controls.Add(Field("userName"), 0, 0);
            txtUserName = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) };
            content.Controls.Add(txtUserName, 0, 1);
            content.Controls.Add(Field("languageLabel"), 0, 2);
            cboLanguage = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) };
            cboLanguage.Items.AddRange(new object[] { "EN - English", "VN - Tiếng Việt", "CN - 中文" });
            content.Controls.Add(cboLanguage, 0, 3);
            content.Controls.Add(Field("gameOptions"), 0, 4);
            ModernCheckBox Check(string key, int row)
            {
                var c = new ModernCheckBox { Text = T(key), Dock = DockStyle.Fill, Margin = new Padding(2, 2, 2, 2) };
                content.Controls.Add(c, 0, row); return c;
            }
            chkMuteSaveValue = Check("muteSaveValue", 5);
            chkLockMouse = Check("lockMouse", 6);
            chkFixRatio = Check("fixRatio", 7);
            chkWideScreen = Check("wideScreen", 8);
            chkFastLoad = Check("fastLoad", 9);
            btnRestoreRegistry = new ModernButton { Text = T("restoreRegistry"), Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 8), Font = new Font("Segoe UI", 9f), BorderColor = Color.FromArgb(55, 75, 103) };
            btnRestoreRegistry.Click += (s, e) =>
            {
                string originalPath = @"D:\Game\Warcraft3 1.27 DZ\Warcraft 3.2";
                if (!Directory.Exists(originalPath))
                {
                    MessageBox.Show(this, string.Format(T("registryMissing"), originalPath), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                RegistryHelper.SetWar3InstallPath(originalPath);
                MessageBox.Show(this, string.Format(T("registryRestored"), originalPath), T("successTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            content.Controls.Add(btnRestoreRegistry, 0, 10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Color.FromArgb(43, 63, 88));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void LoadData()
        {
            txtUserName.TextContent = _config.GetSetting("UserName", "MrP");
            chkMuteSaveValue.Checked = _config.GetSetting("MuteSaveValue", "1") == "1";
            chkLockMouse.Checked = _config.GetSetting("LockMouse", "1") == "1";
            chkFixRatio.Checked = _config.GetSetting("FixRatio", "1") == "1";
            chkWideScreen.Checked = _config.GetSetting("WideScreen", "1") == "1";
            chkFastLoad.Checked = _config.GetSetting("FastLoad", "1") == "1";

            string lang = _config.GetSetting("Language", "EN");
            cboLanguage.SelectedIndex = lang.Equals("VN", StringComparison.OrdinalIgnoreCase) ? 1 : lang.Equals("CN", StringComparison.OrdinalIgnoreCase) ? 2 : 0;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _config.SetSetting("UserName", txtUserName.TextContent.Trim());
            _config.SetSetting("MuteSaveValue", chkMuteSaveValue.Checked ? "1" : "0");
            _config.SetSetting("LockMouse", chkLockMouse.Checked ? "1" : "0");
            _config.SetSetting("FixRatio", chkFixRatio.Checked ? "1" : "0");
            _config.SetSetting("WideScreen", chkWideScreen.Checked ? "1" : "0");
            _config.SetSetting("FastLoad", chkFastLoad.Checked ? "1" : "0");
            _config.SetSetting("Language", cboLanguage.SelectedIndex == 1 ? "VN" : cboLanguage.SelectedIndex == 2 ? "CN" : "EN");
            _config.SaveSettings();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
