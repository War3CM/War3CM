using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class AboutForm : Form
    {
        private readonly ConfigManager _config;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));
        private Label lblUpdateStatus = null!;
        private ModernButton btnCheckUpdates = null!;

        public AboutForm(ConfigManager config)
        {
            _config = config;
            Icon = AppIcons.GetAppIcon();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = T("aboutTitle");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(520, 490);
            MinimumSize = new Size(520, 490);
            MaximumSize = new Size(520, 490);
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
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            Controls.Add(shell);

            var titleBar = new ModernTitleBar(this, T("aboutTitle"), showMin: false, showMax: false) { Dock = DockStyle.Fill };
            shell.Controls.Add(titleBar, 0, 0);

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16, 10, 16, 10)
            };
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));  // Header
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Features card
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 110)); // Updates & Community card
            shell.Controls.Add(content, 0, 1);

            // 1. Header (Icon + Title + Version + Subtitle)
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var iconBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Icon?.ToBitmap()
            };
            header.Controls.Add(iconBox, 0, 0);

            var headerText = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
            headerText.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            headerText.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            headerText.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));

            var lblTitle = new Label
            {
                Text = "Warcraft Platform Manager",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerText.Controls.Add(lblTitle, 0, 0);

            var lblVersion = new Label
            {
                Text = $"Version {AppUpdater.CurrentVersion} (Build 2026.10)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerText.Controls.Add(lblVersion, 0, 1);

            var lblSub = new Label
            {
                Text = "All-in-One Warcraft III 1.27a RPG Launcher, Plugin Switcher & Optimizer",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = Color.FromArgb(160, 185, 215),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            headerText.Controls.Add(lblSub, 0, 2);

            header.Controls.Add(headerText, 1, 0);
            content.Controls.Add(header, 0, 0);

            // 2. Features Card
            var cardFeatures = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 6, 0, 6),
                Padding = new Padding(14, 10, 14, 10),
                BackColor = Color.FromArgb(18, 28, 43),
                BorderColor = Color.FromArgb(43, 63, 88)
            };
            var featureLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Margin = Padding.Empty };
            featureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            for (int i = 0; i < 5; i++) featureLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            var lblFeatHeader = new Label
            {
                Text = "KEY CAPABILITIES & ARCHITECTURE",
                Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 165, 205),
                Dock = DockStyle.Fill
            };
            featureLayout.Controls.Add(lblFeatHeader, 0, 0);

            string[] features = {
                "• KKWE 2.0 & YDWE Plugin Management: Auto-sync profiles, clean vanilla isolation",
                "• Map Level (1–100) & Rank 1 Sync: Full DzAPI & KK Platform compatibility",
                "• VIP Save Profiles: Isolated character slots, external import (.ini/.txt) & auto-backup",
                "• In-Game Optimization: 16:9 widescreen, cursor lock, fast load & save spam mute",
                "• 10-Language Localization: EN, RU, DE, KO, ES, UK, FR, PL, PT, CN"
            };

            for (int i = 0; i < features.Length; i++)
            {
                var lblF = new Label
                {
                    Text = features[i],
                    Font = new Font("Segoe UI", 8.75f),
                    ForeColor = Color.FromArgb(205, 225, 245),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                featureLayout.Controls.Add(lblF, 0, i + 1);
            }
            cardFeatures.Controls.Add(featureLayout);
            content.Controls.Add(cardFeatures, 0, 1);

            // 3. Updates & Community Card
            var cardUpdates = new DarkCardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 6, 0, 0),
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(18, 28, 43),
                BorderColor = Color.FromArgb(43, 63, 88)
            };
            var updatesLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Margin = Padding.Empty };
            updatesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            updatesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            updatesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27.5f));
            updatesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            updatesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            btnCheckUpdates = new ModernButton
            {
                Text = T("btnAboutCheckUpdate"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 2, 4, 2),
                BorderRadius = 6,
                BackColorNormal = Color.FromArgb(24, 120, 234),
                BackColorHover = Color.FromArgb(48, 148, 255),
                GradientEndColor = Color.FromArgb(12, 82, 189)
            };
            btnCheckUpdates.Click += BtnCheckUpdates_Click;
            updatesLayout.Controls.Add(btnCheckUpdates, 0, 0);

            var btnGitHub = new ModernButton
            {
                Text = "🌐 GitHub",
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 2, 4, 2),
                BorderRadius = 6,
                BorderColor = Color.FromArgb(48, 68, 95),
                BackColorNormal = Color.FromArgb(28, 42, 60),
                BackColorHover = Color.FromArgb(52, 152, 219)
            };
            btnGitHub.Click += (s, e) => OpenUrl("https://github.com/War3CM/War3CM");
            updatesLayout.Controls.Add(btnGitHub, 1, 0);

            var btnDiscord = new ModernButton
            {
                Text = "💬 Discord",
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 2, 0, 2),
                BorderRadius = 6,
                BorderColor = Color.FromArgb(48, 68, 95),
                BackColorNormal = Color.FromArgb(28, 42, 60),
                BackColorHover = Color.FromArgb(88, 101, 242)
            };
            btnDiscord.Click += (s, e) => OpenUrl("https://discord.gg/wXtdt/PwpT");
            updatesLayout.Controls.Add(btnDiscord, 2, 0);

            lblUpdateStatus = new Label
            {
                Text = string.Format(T("appUpToDate"), AppUpdater.CurrentVersion),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(46, 204, 113),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            updatesLayout.Controls.Add(lblUpdateStatus, 0, 1);
            updatesLayout.SetColumnSpan(lblUpdateStatus, 3);

            cardUpdates.Controls.Add(updatesLayout);
            content.Controls.Add(cardUpdates, 0, 2);

            // 4. Footer
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 28, 43),
                Padding = new Padding(16, 10, 16, 10),
                ColumnCount = 2,
                RowCount = 1
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            var lblCopyright = new Label
            {
                Text = "War3CM Project • Dedicated to Warcraft III RPG Gamers",
                Font = new Font("Segoe UI", 8.25f),
                ForeColor = Color.FromArgb(130, 155, 185),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            footer.Controls.Add(lblCopyright, 0, 0);

            var btnClose = new ModernButton
            {
                Text = T("btnClose"),
                Dock = DockStyle.Fill,
                BorderRadius = 6,
                BorderColor = Color.FromArgb(55, 75, 103),
                BackColorNormal = Color.FromArgb(28, 42, 60),
                BackColorHover = Color.FromArgb(48, 68, 95)
            };
            btnClose.Click += (s, e) => Close();
            footer.Controls.Add(btnClose, 1, 0);

            shell.Controls.Add(footer, 0, 2);
        }

        private async void BtnCheckUpdates_Click(object? sender, EventArgs e)
        {
            btnCheckUpdates.Enabled = false;
            lblUpdateStatus.Text = T("checkingUpdate");
            lblUpdateStatus.ForeColor = Color.FromArgb(52, 152, 219);

            try
            {
                var update = await AppUpdater.CheckForUpdateAsync(AppUpdater.CurrentVersion);
                if (update != null)
                {
                    lblUpdateStatus.Text = string.Format(T("appUpdateAvailable"), update.Version, "");
                    lblUpdateStatus.ForeColor = Color.FromArgb(241, 196, 15);
                    string changelog = AppUpdater.FormatChangelogForDialog(update.Changelog);
                    string msg = string.Format(T("appUpdateAvailable"), update.Version, string.IsNullOrWhiteSpace(changelog) ? "" : "\n\n" + changelog);
                    var res = MessageBox.Show(this, msg, T("appUpdateTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (res == DialogResult.Yes)
                    {
                        string currentExe = Environment.ProcessPath ?? Application.ExecutablePath;
                        var progress = new Progress<int>(pct =>
                        {
                            if (!IsDisposed) lblUpdateStatus.Text = string.Format(T("appUpdating"), pct);
                        });
                        bool success = await AppUpdater.DownloadAndApplyAsync(update.DownloadUrl, currentExe, progress);
                        if (success)
                        {
                            lblUpdateStatus.Text = T("appUpdateRestarting");
                            lblUpdateStatus.ForeColor = Color.FromArgb(46, 204, 113);
                            await System.Threading.Tasks.Task.Delay(500);
                            AppUpdater.RestartApplication(currentExe);
                        }
                        else
                        {
                            lblUpdateStatus.Text = string.Format(T("appUpdateFailed"), "Download error");
                            lblUpdateStatus.ForeColor = Color.FromArgb(231, 76, 60);
                        }
                    }
                }
                else
                {
                    lblUpdateStatus.Text = string.Format(T("appUpToDate"), AppUpdater.CurrentVersion);
                    lblUpdateStatus.ForeColor = Color.FromArgb(46, 204, 113);
                }
            }
            catch (Exception ex)
            {
                lblUpdateStatus.Text = ex.Message;
                lblUpdateStatus.ForeColor = Color.FromArgb(231, 76, 60);
            }
            finally
            {
                btnCheckUpdates.Enabled = true;
            }
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Color.FromArgb(43, 63, 88));
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
