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
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "VN"));
        private string CurrentLang => _config.GetSetting("Language", "VN").ToUpperInvariant();

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
                    "EN" => "📖 Warcraft Platform - Setup & Quick Start Guide",
                    _ => "📖 HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG WARCRAFT PLATFORM"
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
                    "CN" => "准备纯净版游戏、更新KKWE插件以及安全管理RPG独立存档的分步教程。",
                    "EN" => "Step-by-step instructions for clean game setup, plugin updates, and RPG save slots.",
                    _ => "Chuẩn bị bộ game sạch 1.27.5, cập nhật plugin KKWE và tối ưu nạp map / đồng bộ Save Slot."
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
                "CN" => "1. 下载纯净版游戏 (Warcraft_1.27.5_clean.zip) 与必备工具",
                "EN" => "1. Download Clean Game (Warcraft_1.27.5_clean.zip) & Tools",
                _ => "1. Tải bộ game chuẩn sạch (Warcraft_1.27.5_clean.zip) & Công cụ"
            }, Color.FromArgb(64, 180, 255));
            var lblStep1Desc = CreateBodyText(lang switch
            {
                "CN" => "请从官方共享网盘下载完整纯净版 Warcraft_1.27.5_clean.zip 及所需配套工具：\n" + DRIVE_URL,
                "EN" => "Download the official clean Warcraft_1.27.5_clean.zip package and essential utilities from Google Drive:\n" + DRIVE_URL,
                _ => "Tải gói Warcraft_1.27.5_clean.zip nguyên bản và các công cụ hỗ trợ cần thiết tại Google Drive:\n" + DRIVE_URL
            });

            var driveActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            driveActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var btnCardDrive = new ModernButton
            {
                Text = lang switch
                {
                    "CN" => "🌐 打开网盘下载地址",
                    "EN" => "🌐 Open Google Drive Link",
                    _ => "🌐 Mở thư mục Google Drive tải Game"
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
                    "EN" => "📋 Copy Download Link",
                    _ => "📋 Sao chép liên kết tải"
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
                "EN" => "💬 Join Community & Video Channel (Discord & YouTube)",
                _ => "💬 THAM GIA CỘNG ĐỒNG GIAO LƯU & HỖ TRỢ (DISCORD & YOUTUBE)"
            }, Color.FromArgb(100, 190, 255));
            var lblCommDesc = CreateBodyText(lang switch
            {
                "CN" => "欢迎加入我们活跃的魔兽RPG玩家社群，交流游戏心得、组队开黑并观看官方视频教程：",
                "EN" => "Connect with our active Warcraft RPG community, share maps, find teammates, and watch tutorials:",
                _ => "Tham gia cộng đồng để cùng giao lưu với các game thủ RPG, chia sẻ map mới, tìm đồng đội và xem video hướng dẫn:"
            });

            var commActionRow = new TableLayoutPanel { Dock = DockStyle.Top, Height = 38, ColumnCount = 2, Margin = new Padding(0, 8, 0, 4) };
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            commActionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            var btnCardDiscord = new ModernButton
            {
                Text = lang switch
                {
                    "CN" => "💬 加入 Discord 社区",
                    "EN" => "💬 Join Discord Server",
                    _ => "💬 Tham gia Discord Cộng đồng"
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
                    "EN" => "▶ Watch YouTube Channel",
                    _ => "▶ Kênh YouTube Hướng dẫn"
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
                "EN" => "⚠️ CRITICAL WARNING: Map File Name Length",
                _ => "⚠️ CẢNH BÁO QUAN TRỌNG: ĐỘ DÀI TÊN FILE MAP"
            }, Color.FromArgb(255, 160, 40));
            var lblWarnBody = CreateBodyText(lang switch
            {
                "CN" => "魔兽争霸3引擎内部对地图启动路径存在54字节的最大缓冲区限制！\n" +
                        "• 切勿将地图名称保存过长，更不要包含复杂特殊符号或多层深层子目录。\n" +
                        "• 建议在放入游戏前将地图名重命名为简短纯英文字符（例如: kiemthe.w3x）。\n" +
                        "• 若地图名过长，启动时必定造成魔兽游戏崩溃 (Crash) 或存档无法正常读取！",
                "EN" => "The Warcraft III engine has a strict 54-byte internal buffer limit on map launch paths!\n" +
                        "• Never use overly long map filenames or deep subfolder paths.\n" +
                        "• Recommended: Rename your map to a short name before playing (e.g. kiemthe.w3x).\n" +
                        "• Overly long names WILL crash Warcraft III or fail to sync save slot data!",
                _ => "Engine gốc của Warcraft III có giới hạn bộ đệm tối đa 54 ký tự cho tham số nạp bản đồ!\n" +
                     "• Tuyệt đối KHÔNG để tên file map quá dài hoặc chứa ký tự đặc biệt, dấu tiếng Việt.\n" +
                     "• Khuyến nghị: Hãy đổi tên file map ngắn gọn trước khi chơi (ví dụ: kiemthe.w3x thay vì tên dài dòng).\n" +
                     "• Tên file quá dài sẽ làm Warcraft III bị văng (Crash) hoặc không nạp được dữ liệu Save Slot!"
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
                    "EN" => "2. Extract Clean Warcraft 1.27.5",
                    _ => "2. Giải nén Warcraft_1.27.5_clean.zip"
                },
                lang switch
                {
                    "CN" => "解压压缩包至电脑任意磁盘目录（例如: D:\\Warcraft_1.27.5 或 C:\\Games\\Warcraft3）。\n" +
                            "💡 提示：目录路径中切勿包含中文、特殊字符或过长目录层级，以防DLL加载异常。",
                    "EN" => "Extract the zip file to any folder on your computer (e.g. D:\\Warcraft_1.27.5 or C:\\Games\\Warcraft3).\n" +
                            "💡 Tip: Avoid non-ASCII characters or accented paths to prevent plugin loading issues.",
                    _ => "Giải nén tệp zip vào một thư mục tùy chọn trên máy tính (ví dụ: D:\\Warcraft_1.27.5 hoặc C:\\Games\\Warcraft3).\n" +
                         "💡 Mẹo: Không đặt trong thư mục có dấu tiếng Việt (ví dụ: D:\\Game Chơi\\...) để tránh lỗi nạp plugin DLL."
                }
            ));

            // 5. Step 3: Copy Maps
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "3. 将地图文件放入 Maps 文件夹",
                    "EN" => "3. Copy Maps into the Maps Directory",
                    _ => "3. Copy Map vào thư mục Maps trong game"
                },
                lang switch
                {
                    "CN" => "将你下载的 RPG 地图（.w3x 或 .w3m）复制到解压目录下的 Maps 文件夹中（例如: Maps\\Download\\）。\n" +
                            "💡 再次提醒：务必保持地图文件名简明短小。",
                    "EN" => "Copy custom RPG map files (.w3x or .w3m) into the Maps directory inside your game folder (e.g. Maps\\Download\\).\n" +
                            "💡 Reminder: Always keep map filenames short and clean.",
                    _ => "Copy các file bản đồ RPG (.w3x hoặc .w3m) vào thư mục con Maps bên trong thư mục game (ví dụ: Maps\\Download\\).\n" +
                         "💡 Nhắc lại: Hãy đổi tên file map ngắn gọn trước khi copy."
                }
            ));

            // 6. Step 4: Game Library setup in Launcher
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "4. 在启动器中选择游戏目录 (Game Library)",
                    "EN" => "4. Set Game Library Path in Launcher",
                    _ => "4. Chọn đường dẫn Thư viện Game trong Warcraft Platform"
                },
                lang switch
                {
                    "CN" => "打开 Warcraft Platform Manager 启动器：\n" +
                            "• 在【选择魔兽目录】中点击【浏览...】，定位到刚才解压包含 war3.exe 的根目录。\n" +
                            "• 点击旁边快捷按钮【↗】可立即打开此文件夹进行确认。",
                    "EN" => "Open Warcraft Platform Manager:\n" +
                            "• In 'Select War3 Folder', click 'Browse...' and choose the folder containing war3.exe.\n" +
                            "• Click '↗' to quickly open and verify the target game directory.",
                    _ => "Mở công cụ Warcraft Platform Manager:\n" +
                         "• Tại ô 'Chọn thư mục Warcraft 3', bấm nút 'Duyệt...' và trỏ đến thư mục game vừa giải nén (nơi có file war3.exe).\n" +
                         "• Bấm nút '↗' bên cạnh để mở nhanh thư mục kiểm tra."
                }
            ));

            // 7. Step 5: Update & Plugins
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "5. 更新与安装最新 KKWE 插件",
                    "EN" => "5. Update & Select Latest KKWE Plugin",
                    _ => "5. Cập nhật và cài đặt Plugin KKWE mới nhất"
                },
                lang switch
                {
                    "CN" => "• 点击右上角的【更新】按钮，在线拉取并更新最新 KKWE 插件版本。\n" +
                            "• 在【插件版本】下拉框中选择最新插件（系统将自动识别出具体版本，例如: KKWE 2.0.12.2606）。\n" +
                            "• 如需配置高阶插件特性，可点击【YDWE】打开高级配置中心。",
                    "EN" => "• Click 'Updates' in the top header to fetch and install the latest KKWE plugins.\n" +
                            "• In 'Plugin Version', select the newest profile (detected e.g. KKWE 2.0.12.2606).\n" +
                            "• Optional: Click 'YDWE' to fine-tune advanced plugin settings.",
                    _ => "• Bấm nút 'Cập nhật' (Updates) ở thanh trên cùng để tải các gói plugin mới nhất từ hệ thống.\n" +
                         "• Tại ô 'Cấu hình Plugin', chọn phiên bản plugin KKWE mới nhất (hệ thống sẽ tự nhận diện như KKWE 2.0.12.2606).\n" +
                         "• Tùy chọn: Bấm nút 'YDWE' nếu muốn mở trình cấu hình plugin nâng cao."
                }
            ));

            // 8. Step 6: Map & Save Slots
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "6. 选择地图与创建独立存档槽 (Save Slot)",
                    "EN" => "6. Select Map & Create Save Slots",
                    _ => "6. Chọn Map và tạo Hồ sơ Lưu điểm (Save Slots)"
                },
                lang switch
                {
                    "CN" => "• 在【选择地图】中选取想要体验的地图（亦可直接从桌面拖拽地图文件放入启动器）。\n" +
                            "• 点击【+ 新建槽】为当前角色或新开局创建专属独立存档。\n" +
                            "• 支持随时【备份】存档、误删【恢复】以及历史快照归档，换电脑也不丢进度。",
                    "EN" => "• Select your map from the dropdown (or drag & drop map files directly onto the launcher).\n" +
                            "• Click '+ New Slot' to create an isolated save slot for your hero or playthrough.\n" +
                            "• Use 'Backup', 'Delete', and 'Restore' anytime with automated snapshot history.",
                    _ => "• Tại ô 'Chọn Map', chọn bản đồ bạn muốn chơi (hoặc kéo thả file map trực tiếp vào cửa sổ phần mềm).\n" +
                         "• Bấm nút '+ Tạo Slot' để tạo hồ sơ lưu điểm riêng cho nhân vật hoặc lượt chơi của bạn.\n" +
                         "• Bạn có thể tạo nhiều slot, sao lưu dữ liệu với nút 'Sao lưu' hoặc khôi phục slot đã xóa với nút 'Khôi phục'."
                }
            ));

            // 9. Step 7: In-Game Options
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "7. 游戏内置定制选项 (IN-GAME OPTIONS)",
                    "EN" => "7. Tune In-Game Options",
                    _ => "7. Tùy biến trong game (IN-GAME OPTIONS)"
                },
                lang switch
                {
                    "CN" => "点击【⚙ 游戏内置选项】按钮：\n" +
                            "• 修改你的游戏用户名 (User Name) 作为RPG联机唯一标识。\n" +
                            "• 勾选【锁定鼠标】防止窗口模式下光标滑出视野。\n" +
                            "• 勾选【16:9宽屏】与【4:3原生比例修复】获得最舒适画质。\n" +
                            "• 勾选【快速加载】大幅缩短载入时间，并开启【防刷屏】屏蔽自动存档积分弹窗。",
                    "EN" => "Click '⚙ IN-GAME OPTIONS':\n" +
                            "• Set your in-game User Name for multiplayer and save identification.\n" +
                            "• Enable 'Lock Mouse' to keep cursor contained inside windowed mode.\n" +
                            "• Enable 'WideScreen 16:9' and 'Fix Aspect Ratio' for optimal visuals.\n" +
                            "• Enable 'Fast Load' and 'Mute Save Value' to block annoying score popups.",
                    _ => "Bấm nút '⚙ TÙY CHỌN TRONG GAME':\n" +
                         "• Đổi 'Tên người dùng trong game' (User Name) để làm định danh lưu điểm RPG.\n" +
                         "• Tích chọn: 'Khóa chuột trong cửa sổ' (chống trượt chuột ra ngoài màn hình khi lia map).\n" +
                         "• Tích chọn: 'Sửa tỉ lệ 4:3', 'Màn hình rộng 16:9', 'Tải map nhanh'.\n" +
                         "• Tích chọn: 'Tắt hiển thị thông báo Save Value' (chặn spam chữ hệ thống khi tự động lưu điểm)."
                }
            ));

            // 10. Step 8: Play & Auto Sync
            stack.Controls.Add(CreateStepCard(
                lang switch
                {
                    "CN" => "8. 一键启动并自动同步 (PLAY NOW)",
                    "EN" => "8. Launch & Automatic Sync (PLAY NOW)",
                    _ => "8. Khởi chạy game & Tự động đồng bộ (PLAY NOW)"
                },
                lang switch
                {
                    "CN" => "• 选择图形渲染 API (OpenGL / DirectX) 及全屏/无边框/窗口模式。\n" +
                            "• 点击【▶ 开始】：平台将使用安全短路径加载地图并挂载所选存档。\n" +
                            "• 游戏退出时，平台全自动拦截并同步最新进度回你的独立槽中，安全可靠！",
                    "EN" => "• Select Graphics (OpenGL / DirectX) and Display Mode (Full Screen / Borderless / Windowed).\n" +
                            "• Click '▶ PLAY NOW': The launcher stages the map safely and mounts your save slot.\n" +
                            "• Upon exiting game, progress is automatically synced back to your save profile!",
                    _ => "• Chọn loại đồ họa (OpenGL hoặc DirectX) và chế độ hiển thị (Full Screen, Borderless, Windowed).\n" +
                         "• Bấm '▶ CHƠI': Phần mềm sẽ tự động nạp map theo đường dẫn rút gọn an toàn (chống lỗi tràn bộ nhớ 54 ký tự), nạp hồ sơ Save Slot vào game.\n" +
                         "• Khi bạn thoát game, phần mềm sẽ tự động phát hiện và đồng bộ tiến trình mới nhất vào Save Slot của bạn!"
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
