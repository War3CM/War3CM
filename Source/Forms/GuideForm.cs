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
            string lang = CurrentLang;

            // 1. Header Banner Card
            var banner = CreateCard(Color.FromArgb(19, 36, 56), Color.FromArgb(44, 76, 110), 12);
            var bannerLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblTitle = new Label
            {
                Text = lang switch
                {
                    "CN" => "📖 魔兽对战与RPG平台 - 完整安装与使用指南",
                    _ => "📖 Warcraft Platform - Setup & Quick Start Guide"
                },
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 160, 240),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblSub = new Label
            {
                Text = lang switch
                {
                    "CN" => "准备纯净版游戏、更新KKWE插件、载入VIP存档、定制地图等级以及自动同步进度的分步教程。",
                    _ => "Step-by-step instructions for clean game setup, plugin updates, VIP save loading, Map Level, and automatic progress sync."
                },
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
            var lblStep1Title = CreateStepHeader(lang switch
            {
                "CN" => "1. 下载纯净版 (Warcraft_1.27.5_clean.zip) 与运行库 (aio-runtimes_v2.4.9.exe)",
                _ => "1. Download Clean Game (Warcraft_1.27.5_clean.zip) & Install aio-runtimes_v2.4.9.exe"
            }, Color.FromArgb(64, 180, 255));
            var lblStep1Desc = CreateBodyText(lang switch
            {
                "CN" => "• 访问官方共享网盘，下载 Warcraft_1.27.5_clean.zip 以及必备运行库 aio-runtimes_v2.4.9.exe。\n" +
                        "• 【重要步骤】：请先运行并安装 aio-runtimes_v2.4.9.exe，自动配置好 Visual C++ (2005-2022) 与 DirectX 9.0c 运行环境。彻底解决魔兽及 KKWE 插件启动时缺失 DLL (MSVCR/D3DX) 的报错问题。\n" +
                        "• 网盘地址: " + DRIVE_URL,
                _ => "• Download Warcraft_1.27.5_clean.zip and aio-runtimes_v2.4.9.exe from the Google Drive link below.\n" +
                     "• IMPORTANT: Run and install aio-runtimes_v2.4.9.exe first to install all essential Visual C++ runtimes (2005-2022) and DirectX. This prevents missing DLL crashes when launching Warcraft or loading KKWE plugins.\n" +
                     "• Google Drive: " + DRIVE_URL
            });

            var driveActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var btnCardDrive = new ModernButton
            {
                Text = lang switch
                {
                    "CN" => "🌐 打开网盘下载地址",
                    _ => "🌐 Open Google Drive Link"
                },
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
                Text = lang switch
                {
                    "CN" => "📋 复制下载链接",
                    _ => "📋 Copy Download Link"
                },
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
            var lblCommTitle = CreateStepHeader(lang switch
            {
                "CN" => "💬 加入官方社区与视频频道（Discord & YouTube）",
                _ => "💬 Join Community & Video Channel (Discord & YouTube)"
            }, Color.FromArgb(100, 190, 255));
            var lblCommDesc = CreateBodyText(lang switch
            {
                "CN" => "欢迎加入我们活跃的魔兽RPG玩家社群，交流游戏心得、组队开黑并观看官方视频教程：",
                _ => "Connect with our active Warcraft RPG community, share maps, find teammates, and watch tutorials:"
            });

            var commActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnCardDiscord = new ModernButton
            {
                Text = lang switch
                {
                    "CN" => "💬 加入 Discord 社区",
                    _ => "💬 Join Discord Server"
                },
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
                Text = lang switch
                {
                    "CN" => "▶ 访问 YouTube 频道",
                    _ => "▶ Watch YouTube Channel"
                },
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
            var lblWarnTitle = CreateStepHeader(lang switch
            {
                "CN" => "⚠️ 核心警告：地图文件名切勿过长！",
                _ => "⚠️ CRITICAL WARNING: Map File Name Length"
            }, Color.FromArgb(255, 160, 40));
            var lblWarnBody = CreateBodyText(lang switch
            {
                "CN" => "魔兽争霸3引擎内部对地图启动路径存在54字节的最大缓冲区限制！\n" +
                        "• 切勿将地图名称保存过长，更不要包含复杂特殊符号或多层深层子目录。\n" +
                        "• 建议在放入游戏前将地图名重命名为简短纯英文字符（例如: kiemthe.w3x）。\n" +
                        "• 若地图名过长，启动时必定造成魔兽游戏崩溃 (Crash) 或存档无法正常读取！",
                _ => "The Warcraft III engine has a strict 54-byte internal buffer limit on map launch paths!\n" +
                     "• Never use overly long map filenames or deep subfolder paths.\n" +
                     "• Recommended: Rename your map to a short name before playing (e.g. kiemthe.w3x).\n" +
                     "• Overly long names WILL crash Warcraft III or fail to sync save slot data!"
            });
            lblWarnBody.ForeColor = Color.FromArgb(255, 215, 170);
            warnLayout.Controls.Add(lblWarnTitle);
            warnLayout.Controls.Add(lblWarnBody);
            warnCard.Controls.Add(warnLayout);
            stack.Controls.Add(warnCard);

            // 4. Step 2: Extract Game
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "2. 解压 Warcraft_1.27.5_clean.zip",
                    _ => "2. Extract Clean Warcraft 1.27.5"
                },
                lang switch
                {
                    "CN" => "解压压缩包至电脑任意磁盘目录（例如: D:\\Warcraft_1.27.5 或 C:\\Games\\Warcraft3）。\n" +
                            "💡 提示：目录路径中切勿包含中文、特殊字符或过长目录层级，以防DLL加载异常。",
                    _ => "Extract the zip file to any folder on your computer (e.g. D:\\Warcraft_1.27.5 or C:\\Games\\Warcraft3).\n" +
                         "💡 Tip: Avoid non-ASCII characters or accented paths to prevent plugin loading issues."
                }
            ));

            // 5. Step 3: Copy Maps
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "3. 将地图文件放入 Maps 文件夹",
                    _ => "3. Copy Maps into the Maps Directory"
                },
                lang switch
                {
                    "CN" => "将你下载的 RPG 地图（.w3x 或 .w3m）复制到解压目录下的 Maps 文件夹中（例如: Maps\\Download\\）。\n" +
                            "💡 再次提醒：务必保持地图文件名简明短小。",
                    _ => "Copy custom RPG map files (.w3x or .w3m) into the Maps directory inside your game folder (e.g. Maps\\Download\\).\n" +
                         "💡 Reminder: Always keep map filenames short and clean."
                }
            ));

            // 6. Step 4: Game Library setup in Launcher
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "4. 在启动器中选择游戏目录 (Game Library)",
                    _ => "4. Set Game Library Path in Launcher"
                },
                lang switch
                {
                    "CN" => "打开 Warcraft Platform Manager 启动器：\n" +
                            "• 在【选择魔兽目录】中点击【浏览...】，定位到刚才解压包含 war3.exe 的根目录。\n" +
                            "• 点击旁边快捷按钮【↗】可立即打开此文件夹进行确认。",
                    _ => "Open Warcraft Platform Manager:\n" +
                         "• In 'Select War3 Folder', click 'Browse...' and choose the folder containing war3.exe.\n" +
                         "• Click '↗' to quickly open and verify the target game directory."
                }
            ));

            // 7. Step 5: Update & Plugins
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "5. 更新与安装最新 KKWE 插件",
                    _ => "5. Update & Select Latest KKWE Plugin"
                },
                lang switch
                {
                    "CN" => "• 点击右上角的【更新】按钮，在线拉取并更新最新 KKWE 插件版本。\n" +
                            "• 在【插件版本】下拉框中选择最新插件（系统将自动识别出具体版本，例如: KKWE 2.0.12.2606）。\n" +
                            "• 如需配置高阶插件特性，可点击【YDWE】打开高级配置中心。",
                    _ => "• Click 'Updates' in the top header to fetch and install the latest KKWE plugins.\n" +
                         "• In 'Plugin Version', select the newest profile (detected e.g. KKWE 2.0.12.2606).\n" +
                         "• Optional: Click 'YDWE' to fine-tune advanced plugin settings."
                }
            ));

            // 8. Step 6: Map & Save Slots
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "6. 存档管理、Load Save (VIP)、Clean Data、地图等级 (1-100) 与 Rank 1",
                    _ => "6. Save Profiles, Load Save (VIP), Clean Data, Map Level (1-100) & Rank 1"
                },
                lang switch
                {
                    "CN" => "• 在【选择地图】中选取想要体验的地图（亦可直接从桌面拖拽地图文件放入启动器）。\n" +
                            "• 创建与删除：点击【+ 新建槽】为当前角色创建专属独立存档；点击【删除】可将不再使用的槽移至回收目录。\n" +
                            "• 载入存档 (Load Save)：点击【Load Save】即可选取电脑中任意外部存档文件（VIP 存档、好友分享存档等），输入新名称后直接导入使用。\n" +
                            "• 清理数据 (Clean Data)：点击【清理数据】可自主选择清理游戏过程中积累的临时文件（Maps\\WPM 地图缓存、_Trash 存档垃圾、_History 历史快照、游戏报错日志等），一键释放磁盘空间。\n" +
                            "• 地图等级 (Map Level 1-100)：在输入框中填入期望等级（1 至 100）并点击【Lưu / Set】（或按 Enter 回车键），系统自动同步至 DzAPI 与 KKAPI 格式。\n" +
                            "• 切换 Rank 1：点击【Rank 1】按钮（激活时边框呈耀眼金黄色），一键开启地图最高天梯排名与特权。",
                    _ => "• Select your map from the dropdown (or drag & drop map files directly onto the launcher).\n" +
                         "• Create & Delete: Click '+ New Slot' to create an isolated save for each character; click 'Delete' to remove unwanted slots.\n" +
                         "• Load Save (Import VIP Save): Click 'Load Save' to pick any external save file (.ini, .txt, VIP shared saves), enter a new profile name, and immediately use it in the selected map.\n" +
                         "• Clean Data: Click 'Clean Data' to selectively clean temporary game files accumulated during play (staged map cache in Maps\\WPM, save trash & history snapshots, game error logs...) to reclaim disk space.\n" +
                         "• Map Level (1-100): Enter your desired level (1 to 100) in the input box and click 'Set' (or press Enter) to apply it across DzAPI and KKAPI standards.\n" +
                         "• Toggle Rank 1: Click 'Rank 1' (turns Amber Gold when active) to enable top-rank perks and leaderboard privileges in supported maps."
                }
            ));

            // 9. Step 7: In-Game Options
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "7. 游戏内置定制选项 (IN-GAME OPTIONS)",
                    _ => "7. Tune In-Game Options"
                },
                lang switch
                {
                    "CN" => "点击【⚙ 游戏内置选项】按钮：\n" +
                            "• 修改你的游戏用户名 (User Name) 作为RPG联机唯一标识。\n" +
                            "• 勾选【锁定鼠标】防止窗口模式下光标滑出视野。\n" +
                            "• 勾选【16:9宽屏】与【4:3原生比例修复】获得最舒适画质。\n" +
                            "• 勾选【快速加载】大幅缩短载入时间，并开启【防刷屏】屏蔽自动存档积分弹窗。",
                    _ => "Click '⚙ IN-GAME OPTIONS':\n" +
                         "• Set your in-game User Name for multiplayer and save identification.\n" +
                         "• Enable 'Lock Mouse' to keep cursor contained inside windowed mode.\n" +
                         "• Enable 'WideScreen 16:9' and 'Fix Aspect Ratio' for optimal visuals.\n" +
                         "• Enable 'Fast Load' and 'Mute Save Value' to block annoying score popups."
                }
            ));

            // 10. Step 8: Play & Auto Sync
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "8. 启动游戏与自动同步 (Play Map / Play War3)",
                    _ => "8. Launch Game & Automatic Sync (Play Map / Play War3)"
                },
                lang switch
                {
                    "CN" => "• 选择图形渲染 API (OpenGL / DirectX) 及全屏/无边框/窗口模式。\n" +
                            "• Play Map（直接进图）：一键载入所选地图与对应存档，并启用安全短路径缓冲。退出游戏时自动同步最新进度回存档槽中，安全可靠！\n" +
                            "• Play War3（主菜单）：携带完整插件进入魔兽3主界面，用于局域网联机或游戏内设置。",
                    _ => "• Choose Graphics (OpenGL / DirectX) and Display Mode (Full Screen / Borderless / Windowed).\n" +
                         "• Play Map: Directly launches the selected map with your active save slot mounted. Progress automatically syncs when exiting game.\n" +
                         "• Play War3: Launches Warcraft III with all active plugins directly to Main Menu for LAN games or settings."
                }
            ));
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
