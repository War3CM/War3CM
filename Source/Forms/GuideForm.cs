using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class GuideForm : Form
    {
        public const string DRIVE_URL = "https://drive.google.com/drive/folders/1BbGpDIKfuIFfC2PXpDfE18QDmPQxarAQ?usp=sharing";
        public const string DISCORD_URL = "https://discord.gg/wXtdt7PwpT";
        public const string YOUTUBE_URL = "https://www.youtube.com/channel/UCw9col-g45AuA2Xhjwps7Lw/";

        private readonly ConfigManager _config;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));
        private string CurrentLang => _config.GetSetting("Language", "EN").ToUpperInvariant();

        public GuideForm(ConfigManager config)
        {
            _config = config;
            Icon = AppIcons.GetAppIcon();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = T("guideTitle");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(720, 680);
            MinimumSize = new Size(720, 680);
            MaximumSize = new Size(720, 680);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(12, 18, 29);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            var shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            Controls.Add(shell);

            // Title Bar
            var titleBar = new ModernTitleBar(this, T("guideTitle"), showMin: false, showMax: false) { Dock = DockStyle.Fill };
            shell.Controls.Add(titleBar, 0, 0);

            // Footer Bar
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(17, 26, 40),
                Padding = new Padding(18, 10, 18, 10),
                ColumnCount = 3
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));

            var btnCopyLink = new ModernButton
            {
                Text = T("btnCopyLink"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0),
                BorderColor = Color.FromArgb(50, 75, 105)
            };
            btnCopyLink.Click += (s, e) =>
            {
                try
                {
                    Clipboard.SetText(DRIVE_URL);
                    MessageBox.Show(this, T("linkCopied"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var btnOpenDrive = new ModernButton
            {
                Text = T("btnOpenDrive"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0),
                BackColorNormal = Color.FromArgb(24, 120, 234),
                BackColorHover = Color.FromArgb(48, 148, 255),
                GradientEndColor = Color.FromArgb(12, 82, 189),
                BorderColor = Color.FromArgb(48, 148, 255),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnOpenDrive.Click += (s, e) => OpenDriveUrl();

            var btnClose = new ModernButton
            {
                Text = T("btnClose"),
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BorderColor = Color.FromArgb(50, 75, 105)
            };
            btnClose.Click += (s, e) => Close();

            footer.Controls.Add(btnCopyLink, 0, 0);
            footer.Controls.Add(btnOpenDrive, 1, 0);
            footer.Controls.Add(btnClose, 2, 0);
            shell.Controls.Add(footer, 0, 2);

            // Scrollable Content
            var scroller = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(18, 12, 18, 12)
            };
            shell.Controls.Add(scroller, 0, 1);

            var stack = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            scroller.Controls.Add(stack);

            // Build all content cards
            PopulateGuideCards(stack);
        }

        private void OpenDriveUrl() => OpenExternalUrl(DRIVE_URL);

        private void OpenExternalUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message + "\n\n" + url, T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGuideCards(TableLayoutPanel stack)
        {
            var content = GuideLocalization.GetContent(CurrentLang);

            // 1. Header Banner Card
            var banner = CreateCard(Color.FromArgb(19, 36, 56), Color.FromArgb(44, 76, 110), 12);
            var bannerLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblTitle = new Label
            {
                Text = content.BannerTitle,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 160, 240),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblSub = new Label
            {
                Text = content.BannerSubtitle,
                Font = new Font("Segoe UI", 9.25f),
                ForeColor = Color.FromArgb(170, 195, 225),
                AutoSize = true
            };
            bannerLayout.Controls.Add(lblTitle);
            bannerLayout.Controls.Add(lblSub);
            banner.Controls.Add(bannerLayout);
            stack.Controls.Add(banner);

            // 2. Drive Download Card (Step 1)
            var driveCard = CreateCard(Color.FromArgb(18, 32, 50), Color.FromArgb(35, 95, 160), 12);
            var driveLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblStep1Title = CreateStepHeader(content.Step1Title, Color.FromArgb(64, 180, 255));
            var lblStep1Desc = CreateBodyText(content.Step1Desc);

            var driveActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var btnCardDrive = new ModernButton
            {
                Text = content.BtnCardDrive,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0),
                BackColorNormal = Color.FromArgb(24, 120, 234),
                BackColorHover = Color.FromArgb(48, 148, 255),
                GradientEndColor = Color.FromArgb(12, 82, 189),
                BorderColor = Color.FromArgb(48, 148, 255),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold)
            };
            btnCardDrive.Click += (s, e) => OpenDriveUrl();

            var btnCardCopy = new ModernButton
            {
                Text = content.BtnCardCopy,
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0),
                BorderColor = Color.FromArgb(50, 75, 105)
            };
            btnCardCopy.Click += (s, e) =>
            {
                Clipboard.SetText(DRIVE_URL);
                MessageBox.Show(this, T("linkCopied"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            driveActionRow.Controls.Add(btnCardDrive, 0, 0);
            driveActionRow.Controls.Add(btnCardCopy, 1, 0);

            driveLayout.Controls.Add(lblStep1Title);
            driveLayout.Controls.Add(lblStep1Desc);
            driveLayout.Controls.Add(driveActionRow);
            driveCard.Controls.Add(driveLayout);
            stack.Controls.Add(driveCard);

            // 2. Community & Video Channel Card (Discord & YouTube)
            var commCard = CreateCard(Color.FromArgb(20, 30, 48), Color.FromArgb(48, 80, 120), 12);
            var commLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblCommTitle = CreateStepHeader(content.CommTitle, Color.FromArgb(100, 190, 255));
            var lblCommDesc = CreateBodyText(content.CommDesc);

            var commActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnCardDiscord = new ModernButton
            {
                Text = content.BtnDiscord,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 6, 0),
                BackColorNormal = Color.FromArgb(88, 101, 242),
                BackColorHover = Color.FromArgb(114, 125, 245),
                BorderColor = Color.FromArgb(114, 125, 245),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold)
            };
            btnCardDiscord.Click += (s, e) => OpenExternalUrl(DISCORD_URL);

            var btnCardYouTube = new ModernButton
            {
                Text = content.BtnYouTube,
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 0, 0),
                BackColorNormal = Color.FromArgb(220, 30, 30),
                BackColorHover = Color.FromArgb(255, 50, 50),
                BorderColor = Color.FromArgb(255, 70, 70),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold)
            };
            btnCardYouTube.Click += (s, e) => OpenExternalUrl(YOUTUBE_URL);

            commActionRow.Controls.Add(btnCardDiscord, 0, 0);
            commActionRow.Controls.Add(btnCardYouTube, 1, 0);

            commLayout.Controls.Add(lblCommTitle);
            commLayout.Controls.Add(lblCommDesc);
            commLayout.Controls.Add(commActionRow);
            commCard.Controls.Add(commLayout);
            stack.Controls.Add(commCard);

            // 3. CRITICAL WARNING: Map name length
            var warnCard = CreateCard(Color.FromArgb(36, 22, 14), Color.FromArgb(220, 115, 20), 12);
            var warnLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblWarnTitle = CreateStepHeader(content.WarnTitle, Color.FromArgb(255, 160, 40));
            var lblWarnBody = CreateBodyText(content.WarnBody);
            lblWarnBody.ForeColor = Color.FromArgb(255, 215, 170);
            warnLayout.Controls.Add(lblWarnTitle);
            warnLayout.Controls.Add(lblWarnBody);
            warnCard.Controls.Add(warnLayout);
            stack.Controls.Add(warnCard);

            // 4. Step 2: Extract Game
            stack.Controls.Add(CreateStepCard(content.Step2.Title, content.Step2.Body));

            // 5. Step 3: Copy Maps
            stack.Controls.Add(CreateStepCard(content.Step3.Title, content.Step3.Body));

            // 6. Step 4: Game Library setup in Launcher
            stack.Controls.Add(CreateStepCard(content.Step4.Title, content.Step4.Body));

            // 7. Step 5: Update & Plugins
            stack.Controls.Add(CreateStepCard(content.Step5.Title, content.Step5.Body));

            // 8. Step 6: Map & Save Slots
            stack.Controls.Add(CreateStepCard(content.Step6.Title, content.Step6.Body));

            // 9. Step 7: In-Game Options
            stack.Controls.Add(CreateStepCard(content.Step7.Title, content.Step7.Body));

            // 10. Step 8: Play & Auto Sync
            stack.Controls.Add(CreateStepCard(content.Step8.Title, content.Step8.Body));
        }

        private static DarkCardPanel CreateCard(Color backColor, Color borderColor, int padding = 12)
        {
            return new DarkCardPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(padding),
                BackColor = backColor,
                BorderColor = borderColor,
                BorderRadius = 8
            };
        }

        private static DarkCardPanel CreateStepCard(string title, string body)
        {
            var card = CreateCard(Color.FromArgb(16, 26, 40), Color.FromArgb(36, 56, 82), 12);
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            layout.Controls.Add(CreateStepHeader(title, Color.FromArgb(130, 205, 255)));
            layout.Controls.Add(CreateBodyText(body));
            card.Controls.Add(layout);
            return card;
        }

        private static Label CreateStepHeader(string text, Color color)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = color,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
        }

        private static Label CreateBodyText(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.25f),
                ForeColor = Color.FromArgb(195, 215, 235),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };
        }
    }
}
