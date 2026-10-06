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
        private ModernButton btnSave;
        private ModernButton btnCancel;
        private ModernButton btnRestoreRegistry;
        private readonly ToolTip _toolTip = new ToolTip { AutoPopDelay = 12000, InitialDelay = 250, ReshowDelay = 100, ShowAlways = true };

        public SettingsForm(ConfigManager config)
        {
            _config = config;
            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            Icon = AppIcons.GetAppIcon();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            Text = T("settingsTitle");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(510, 480);
            MinimumSize = new Size(510, 480);
            MaximumSize = new Size(510, 480);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(12, 18, 29);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

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

            var scroller = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(18, 10, 18, 8) };
            shell.Controls.Add(scroller, 0, 1);
            var content = new TableLayoutPanel { Dock = DockStyle.Top, Height = 310, ColumnCount = 1, RowCount = 9 };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int h in new[] { 25, 36, 28, 32, 32, 32, 32, 32, 54 })
                content.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
            scroller.Controls.Add(content);
            Label Field(string key) => new Label { Text = T(key), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(174, 196, 220), AutoEllipsis = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            content.Controls.Add(Field("userName"), 0, 0);
            txtUserName = new ModernTextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6), MaxLength = 10 };
            content.Controls.Add(txtUserName, 0, 1);
            content.Controls.Add(Field("gameOptions"), 0, 2);
            ModernCheckBox Check(string key, int row)
            {
                var c = new ModernCheckBox { Text = T(key), Tag = key, Dock = DockStyle.Fill, Margin = new Padding(2, 2, 2, 2) };
                content.Controls.Add(c, 0, row); return c;
            }
            chkMuteSaveValue = Check("muteSaveValue", 3);
            chkLockMouse = Check("lockMouse", 4);
            chkFixRatio = Check("fixRatio", 5);
            chkWideScreen = Check("wideScreen", 6);
            chkFastLoad = Check("fastLoad", 7);
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
            content.Controls.Add(btnRestoreRegistry, 0, 8);

            // Rich tooltips explaining in-game effect on hover
            _toolTip.SetToolTip(txtUserName.InnerTextBox, T("tipUserName"));
            _toolTip.SetToolTip(chkMuteSaveValue, T("tipMuteSaveValue"));
            _toolTip.SetToolTip(chkLockMouse, T("tipLockMouse"));
            _toolTip.SetToolTip(chkFixRatio, T("tipFixRatio"));
            _toolTip.SetToolTip(chkWideScreen, T("tipWideScreen"));
            _toolTip.SetToolTip(chkFastLoad, T("tipFastLoad"));
            _toolTip.SetToolTip(btnRestoreRegistry, T("tipRestoreRegistry"));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Color.FromArgb(43, 63, 88));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        private void LoadData()
        {
            string defaultName = _config.GetSetting("UserName", RegistryHelper.GetPlayerName() ?? "War3CM");
            if (defaultName.Length > 10) defaultName = defaultName.Substring(0, 10);
            txtUserName.TextContent = defaultName;
            chkMuteSaveValue.Checked = _config.GetSetting("MuteSaveValue", "1") == "1";
            chkLockMouse.Checked = _config.GetSetting("LockMouse", "1") == "1";
            chkFixRatio.Checked = _config.GetSetting("FixRatio", "1") == "1";
            chkWideScreen.Checked = _config.GetSetting("WideScreen", "1") == "1";
            chkFastLoad.Checked = _config.GetSetting("FastLoad", "1") == "1";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            string userName = txtUserName.TextContent.Trim();
            if (string.IsNullOrEmpty(userName)) userName = "War3CM";
            if (userName.Length > 10) userName = userName.Substring(0, 10);
            _config.SetSetting("UserName", userName);
            RegistryHelper.SetPlayerName(userName);
            _config.SetSetting("MuteSaveValue", chkMuteSaveValue.Checked ? "1" : "0");
            _config.SetSetting("LockMouse", chkLockMouse.Checked ? "1" : "0");
            _config.SetSetting("FixRatio", chkFixRatio.Checked ? "1" : "0");
            _config.SetSetting("WideScreen", chkWideScreen.Checked ? "1" : "0");
            _config.SetSetting("FastLoad", chkFastLoad.Checked ? "1" : "0");
            _config.SaveSettings();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _toolTip.Dispose();
            base.Dispose(disposing);
        }
    }
}
