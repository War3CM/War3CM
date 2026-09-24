using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class UpdateForm : Form
    {
        private readonly KkweUpdateInfo _info;
        private readonly KkweUpdater _updater;
        private readonly string _appDir;
        private readonly ConfigManager _config;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));
        private ProgressBar prgDownload;
        private Label lblStatus;
        private Button btnDownload;
        private Button btnCancel;
        private CancellationTokenSource? _cts;

        public UpdateForm(KkweUpdateInfo info, KkweUpdater updater, string appDir)
        {
            _info = info;
            _updater = updater;
            _appDir = appDir;
            _config = new ConfigManager(appDir);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = T("updaterTitle");
            this.ClientSize = new Size(520, 246);
            this.MinimumSize = new Size(420, 230);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(12, 18, 29);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            var pnl = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20, 18, 20, 18), ColumnCount = 1, RowCount = 6 };
            pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int height in new[] { 36, 27, 34, 36 }) pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            pnl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            this.Controls.Add(pnl);

            var lblTitle = new Label
            {
                Text = string.Format(T("updaterFound"), _info.Title),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(93, 177, 255)
            };
            pnl.Controls.Add(lblTitle, 0, 0);
            new ToolTip().SetToolTip(lblTitle, lblTitle.Text);

            var lblDate = new Label
            {
                Text = string.Format(T("updaterDetails"), _info.PubDate, _info.ContentLength / (1024 * 1024.0)),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(167, 189, 211)
            };
            pnl.Controls.Add(lblDate, 0, 1);

            lblStatus = new Label
            {
                Text = T("updaterReady"),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(210, 224, 240)
            };
            pnl.Controls.Add(lblStatus, 0, 2);

            prgDownload = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 8),
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            pnl.Controls.Add(prgDownload, 0, 3);

            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            pnl.Controls.Add(actions, 0, 5);

            btnDownload = new ModernButton
            {
                Text = T("updaterInstall"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0),
                BackColorNormal = Color.FromArgb(24, 120, 234),
                BackColorHover = Color.FromArgb(48, 148, 255),
                GradientEndColor = Color.FromArgb(12, 82, 189)
            };
            btnDownload.FlatAppearance.BorderSize = 0;
            btnDownload.Click += BtnDownload_Click;
            actions.Controls.Add(btnDownload, 0, 0);

            btnCancel = new ModernButton
            {
                Text = T("btnCancel"),
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) =>
            {
                _cts?.Cancel();
                this.Close();
            };
            actions.Controls.Add(btnCancel, 1, 0);
        }

        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            btnDownload.Enabled = false;
            _cts = new CancellationTokenSource();
            lblStatus.Text = T("updaterConnecting");

            string tempDir = Path.Combine(_appDir, "temp");
            Directory.CreateDirectory(tempDir);
            string exeName = Path.GetFileName(_info.DownloadUrl);
            if (string.IsNullOrEmpty(exeName)) exeName = $"KKWE-{_info.Version}.exe";
            string installerPath = Path.Combine(tempDir, exeName);

            var progress = new Progress<int>(percent =>
            {
                prgDownload.Value = Math.Min(100, Math.Max(0, percent));
                lblStatus.Text = string.Format(T("downloading"), percent);
            });

            bool ok = await _updater.DownloadInstallerAsync(_info.DownloadUrl, installerPath, progress, _cts.Token);
            if (!ok)
            {
                lblStatus.Text = T("updaterFailed");
                btnDownload.Enabled = true;
                return;
            }

            lblStatus.Text = T("extracting");
            prgDownload.Style = ProgressBarStyle.Marquee;

            await Task.Run(() =>
            {
                try
                {
                    // Target profile directory in app Profiles
                    string profileDir = Path.Combine(_appDir, "Profiles", $"KKWE_{_info.Version.Replace(".", "_")}");
                    Directory.CreateDirectory(profileDir);

                    // Execute NSIS silent install into profileDir
                    var psi = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = $"/S /D={profileDir}",
                        UseShellExecute = true
                    };
                    var p = Process.Start(psi);
                    p?.WaitForExit(30000);
                }
                catch { }
            });

            prgDownload.Style = ProgressBarStyle.Blocks;
            prgDownload.Value = 100;
            lblStatus.Text = T("updateSuccess");
            MessageBox.Show(this, T("updaterInstalled"), T("successTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
