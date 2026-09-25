using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class MainForm : Form
    {
        private readonly string _appDir;
        private readonly ConfigManager _config;
        private readonly PluginManager _pluginManager;
        private readonly KkweUpdater _updater;
        private readonly SaveValueMuter _saveValueMuter;
        private readonly MapSaveManager _mapSaves;
        private Process? _activeGame;
        private bool _launching;
        private readonly ToolTip _mapTip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 350, ReshowDelay = 150 };
        private readonly ToolTip _actionTip = new ToolTip { AutoPopDelay = 12000 };
        private ModernComboBox cboMaps;
        private ModernComboBox cboSlots;
        private ModernButton btnNewSlot;
        private ModernButton btnBackupSlot;
        private ModernButton btnDeleteSlot;
        private ModernButton btnRestoreSlot;
        private ModernButton btnBrowseMap;
        private ModernButton btnScanPlugins;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));
        private string HeaderText(string key) => _config.GetText(key, "EN");

        // UI Controls
        private ModernTextBox txtWar3Path;
        private ModernButton btnBrowse;
        private Label lblPathHint;
        private ModernComboBox cboPlugins;
        private ModernComboBox cboGraphic;
        private ModernComboBox cboDisplay;
        private Label lblLaunchHint;
        private ModernButton btnRunGame;
        private ModernButton btnCloseGame;
        private ModernButton btnOpenFolder;
        private ModernButton btnConfigYDWE;
        private ModernButton btnCheckUpdate;
        private ModernButton btnInGameOptions;
        private Label statusLabel;
        private Label lblServerInfo;
        private string? _cachedServerVersion;


        public MainForm()
        {
            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            _config = new ConfigManager(_appDir);
            _pluginManager = new PluginManager(_appDir);
            _updater = new KkweUpdater();
            _saveValueMuter = new SaveValueMuter();
            _mapSaves = new MapSaveManager(_appDir);

            InitializeComponent();
            LoadInitialData();
        }

        private ModernComboBox cboLanguage;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Label lblStatusTitle;
        private readonly Color accent = Color.FromArgb(48, 159, 255);

        private Label Label(string key, bool heading = false)
        {
            return new Label { Text = T(key), Tag = key, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true,
                UseMnemonic = false,
                ForeColor = heading ? Color.White : Color.FromArgb(163, 183, 207),
                Font = new Font("Segoe UI", heading ? 11f : 9f, heading ? FontStyle.Bold : FontStyle.Regular) };
        }

        private DarkCardPanel Card(string title, int height)
        {
            var card = new DarkCardPanel { Height = height, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 9), Padding = new Padding(16, 9, 16, 9), BorderColor = Color.FromArgb(39, 58, 80) };
            var caption = Label(title, true);
            caption.Dock = DockStyle.Top;
            caption.Height = 22;
            card.Controls.Add(caption);
            return card;
        }

        private static TableLayoutPanel Grid(int height, params int[] widths)
        {
            var grid = new TableLayoutPanel { Dock = DockStyle.Top, Height = height, ColumnCount = widths.Length,
                RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty };
            foreach (var w in widths) grid.ColumnStyles.Add(w == 0 ? new ColumnStyle(SizeType.Percent, 100) : new ColumnStyle(SizeType.Absolute, w));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return grid;
        }

        private ModernButton ActionButton(string key, EventHandler click, bool primary = false)
        {
            var btn = new ModernButton { Text = T(key), Tag = key, Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0),
                BorderRadius = 7, BackColorNormal = primary ? Color.FromArgb(24, 120, 234) : Color.FromArgb(33, 47, 68),
                BackColorHover = primary ? Color.FromArgb(48, 148, 255) : Color.FromArgb(48, 68, 95),
                BackColorPressed = Color.FromArgb(15, 70, 132),
                GradientEndColor = primary ? Color.FromArgb(12, 82, 189) : null,
                GradientEndColorHover = primary ? Color.FromArgb(20, 104, 213) : null,
                BorderColor = primary ? Color.FromArgb(48, 148, 255) : Color.FromArgb(55, 75, 103) };
            btn.Click += click;
            _actionTip.SetToolTip(btn, T(key));
            return btn;
        }

        private static void Place(TableLayoutPanel row, Control c, int col)
        {
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(col == 0 ? 0 : 6, 2, 0, 2);
            row.Controls.Add(c, col, 0);
        }

        private void InitializeComponent()
        {
            Text = HeaderText("appName");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(820, 648);
            MinimumSize = new Size(820, 648);
            MaximumSize = new Size(820, 648);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(11, 18, 29);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            Shown += (s, e) =>
            {
                Rectangle area = Screen.FromControl(this).WorkingArea;
                int x = Math.Max(area.Left, Math.Min(Location.X, area.Right - Width));
                int y = Math.Max(area.Top, Math.Min(Location.Y, area.Bottom - Height));
                Location = new Point(x, y);
            };

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty, Padding = Padding.Empty };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int h in new[] { 34, 76 }) shell.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            Controls.Add(shell);
            shell.Controls.Add(new ModernTitleBar(this, HeaderText("appName"), showMin: true, showMax: false) { Dock = DockStyle.Fill }, 0, 0);

            var hero = new DarkCardPanel { Dock = DockStyle.Fill, Margin = new Padding(14, 5, 14, 5), Padding = new Padding(16, 6, 16, 6),
                BackColor = Color.FromArgb(19, 36, 56), BorderColor = Color.FromArgb(44, 76, 110) };
            shell.Controls.Add(hero, 0, 1);
            var heroGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty };
            heroGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            heroGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 240));
            heroGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            hero.Controls.Add(heroGrid);
            var heroText = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty, Padding = Padding.Empty };
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            heroText.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblHeaderTitle = Label("heroTitle", true); lblHeaderTitle.Margin = Padding.Empty;
            lblHeaderTitle.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblHeaderSubtitle = Label("appDescription"); lblHeaderSubtitle.Margin = Padding.Empty;
            lblHeaderSubtitle.Font = new Font("Segoe UI", 8.75f);
            lblHeaderSubtitle.ForeColor = Color.FromArgb(170, 195, 225);
            heroText.Controls.Add(lblHeaderTitle, 0, 0);
            heroText.Controls.Add(lblHeaderSubtitle, 0, 1);
            heroGrid.Controls.Add(heroText, 0, 0);
            var heroActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty };
            heroActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            heroActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            heroActions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            cboLanguage = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 9, 6, 9) };
            cboLanguage.Items.AddRange(new object[] { "EN", "VN", "CN" });
            cboLanguage.SelectedIndex = LanguageIndex();
            cboLanguage.SelectedIndexChanged += (s, e) => ChangeLanguage();
            heroActions.Controls.Add(cboLanguage, 0, 0);
            btnCheckUpdate = ActionButton("btnCheckUpdate", BtnCheckUpdate_Click); btnCheckUpdate.Margin = new Padding(2, 9, 0, 9);
            heroActions.Controls.Add(btnCheckUpdate, 1, 0);
            heroGrid.Controls.Add(heroActions, 1, 0);

            var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
                Padding = new Padding(14, 0, 14, 0), Margin = Padding.Empty };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            shell.Controls.Add(workspace, 0, 2);
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, 6, 0) };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 134));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            workspace.Controls.Add(left, 0, 0);
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(6, 0, 0, 0) };
            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 134));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            workspace.Controls.Add(right, 1, 0);

            var game = Section("gameLibrary", out var gameBody); game.Margin = new Padding(0, 0, 0, 8); left.Controls.Add(game, 0, 0);
            gameBody.RowCount = 3;
            gameBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            gameBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            gameBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            gameBody.Controls.Add(Label("selectWar3Folder"), 0, 0);
            var pathRow = Grid(34, 0, 105, 46);
            txtWar3Path = new ModernTextBox(); Place(pathRow, txtWar3Path, 0);
            txtWar3Path.InnerTextBox.Leave += (s, e) =>
            {
                string selected = txtWar3Path.TextContent.Trim();
                if (_launching || _activeGame != null || !File.Exists(Path.Combine(selected, "war3.exe"))) return;
                if (selected.Equals(_config.GetSetting("War3Path"), StringComparison.OrdinalIgnoreCase)) return;
                _config.SetSetting("War3Path", selected); _config.SaveSettings();
                LoadProfilesList(selected); RefreshMaps();
            };
            btnBrowse = ActionButton("browse", BtnBrowse_Click); Place(pathRow, btnBrowse, 1);
            btnOpenFolder = ActionButton("btnOpenFolder", BtnOpenFolder_Click); btnOpenFolder.Text = "↗"; btnOpenFolder.Tag = null;
            Place(pathRow, btnOpenFolder, 2);
            gameBody.Controls.Add(pathRow, 0, 1);
            lblPathHint = Label("pathHintShort"); lblPathHint.ForeColor = Color.FromArgb(160, 185, 215); gameBody.Controls.Add(lblPathHint, 0, 2);

            var saves = Section("mapSaveTitle", out var saveBody); saves.Margin = Padding.Empty; left.Controls.Add(saves, 0, 1);
            saveBody.RowCount = 5;
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            saveBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            saveBody.Controls.Add(Label("selectMap"), 0, 0);
            var mapRow = Grid(34, 0, 105);
            cboMaps = new ModernComboBox();
            cboMaps.DisplayText = item => item is MapEntry entry ?
                entry.Name + "  ·  " + Path.GetFileName(Path.GetDirectoryName(entry.Path)) : item?.ToString() ?? "";
            cboMaps.SelectedIndexChanged += (s, e) =>
            {
                RefreshSlots(); _mapTip.SetToolTip(cboMaps, (cboMaps.SelectedItem as MapEntry)?.Path ?? "");
            };
            cboMaps.DropDown += (s, e) => cboMaps.DropDownWidth = Math.Min(
                Screen.FromControl(cboMaps).WorkingArea.Width - 24, Math.Max(cboMaps.Width, LogicalToDeviceUnits(480)));
            Place(mapRow, cboMaps, 0);
            btnBrowseMap = ActionButton("browseMap", BrowseMap); Place(mapRow, btnBrowseMap, 1);
            saveBody.Controls.Add(mapRow, 0, 1);
            saveBody.Controls.Add(Label("selectSlot"), 0, 2);
            cboSlots = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            saveBody.Controls.Add(cboSlots, 0, 3);
            var slotActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Margin = new Padding(0, 4, 0, 0) };
            slotActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            slotActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            slotActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            slotActions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            btnNewSlot = ActionButton("newSlot", NewSlot);
            btnNewSlot.BackColorNormal = Color.FromArgb(28, 52, 82); btnNewSlot.BackColorHover = Color.FromArgb(42, 78, 120);
            btnNewSlot.BorderColor = Color.FromArgb(50, 110, 180);
            btnBackupSlot = ActionButton("backupSlot", BackupSlot);
            btnBackupSlot.BackColorNormal = Color.FromArgb(28, 52, 82); btnBackupSlot.BackColorHover = Color.FromArgb(42, 78, 120);
            btnBackupSlot.BorderColor = Color.FromArgb(50, 110, 180);
            btnDeleteSlot = ActionButton("deleteSlot", DeleteSlot);
            btnDeleteSlot.BackColorHover = Color.FromArgb(100, 30, 30); btnDeleteSlot.BorderColor = Color.FromArgb(130, 45, 45);
            btnRestoreSlot = ActionButton("restoreSlot", RestoreSlot);
            foreach (var b in new[] { btnNewSlot, btnBackupSlot, btnDeleteSlot, btnRestoreSlot }) b.Margin = new Padding(2, 2, 2, 2);
            slotActions.Controls.Add(btnNewSlot, 0, 0); slotActions.Controls.Add(btnBackupSlot, 1, 0);
            slotActions.Controls.Add(btnDeleteSlot, 0, 1); slotActions.Controls.Add(btnRestoreSlot, 1, 1);
            saveBody.Controls.Add(slotActions, 0, 4);

            var plugins = Section("pluginOptions", out var pluginBody); plugins.Margin = new Padding(0, 0, 0, 8); right.Controls.Add(plugins, 0, 0);
            pluginBody.RowCount = 3;
            pluginBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            pluginBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            pluginBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pluginBody.Controls.Add(Label("selectPlugin"), 0, 0);
            cboPlugins = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            cboPlugins.DisplayText = item => item is PluginProfile p ?
                (p.IsClean ? T("cleanProfile") :
                 p.IsInstalled ? (p.DetectedVersion != null ? $"{T("installedProfile")} ({p.DetectedVersion})" : T("installedProfile")) :
                 p.DisplayName + " (" + T(p.Origin == "app" ? "appProfile" : p.Origin == "backup" ? "backupProfile" : "gameProfile") + ")") : item?.ToString() ?? "";
            cboPlugins.SelectedIndexChanged += CboPlugins_SelectedIndexChanged;
            pluginBody.Controls.Add(cboPlugins, 0, 1);
            var pluginActions = Grid(34, 0, 0);
            btnScanPlugins = ActionButton("scanPlugins", (s, e) => ScanPlugins()); Place(pluginActions, btnScanPlugins, 0);
            btnConfigYDWE = ActionButton("btnConfig", BtnConfigYDWE_Click); Place(pluginActions, btnConfigYDWE, 1);
            pluginBody.Controls.Add(pluginActions, 0, 2);

            var options = Section("quickOptions", out var optionBody); options.Margin = Padding.Empty; right.Controls.Add(options, 0, 1);
            optionBody.RowCount = 5;
            optionBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            optionBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            optionBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            optionBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            optionBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            optionBody.Controls.Add(Label("graphicType"), 0, 0);
            cboGraphic = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            cboGraphic.Items.AddRange(new object[] { T("openGL"), "DirectX" });
            cboGraphic.SelectedIndex = 0;
            optionBody.Controls.Add(cboGraphic, 0, 1);
            optionBody.Controls.Add(Label("displayMode"), 0, 2);
            cboDisplay = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            cboDisplay.Items.AddRange(new object[] { T("fullScreen"), T("borderless"), T("windowed") }); cboDisplay.SelectedIndex = 0;
            optionBody.Controls.Add(cboDisplay, 0, 3);
            btnInGameOptions = ActionButton("btnInGameOptions", BtnSettings_Click);
            btnInGameOptions.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnInGameOptions.Margin = new Padding(0, 6, 0, 2);
            optionBody.Controls.Add(btnInGameOptions, 0, 4);

            var play = Section("launchTitle", out var playBody); play.Margin = new Padding(14, 5, 14, 5);
            play.Padding = new Padding(15, 6, 15, 6);
            play.BackColor = Color.FromArgb(19, 35, 53); play.BorderColor = Color.FromArgb(43, 86, 126);
            shell.Controls.Add(play, 0, 3);
            playBody.RowCount = 2;
            playBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            playBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var playRow = Grid(42, 0, 140, 210);
            lblLaunchHint = Label("launchHint");
            lblLaunchHint.ForeColor = Color.FromArgb(160, 185, 215);
            lblLaunchHint.Font = new Font("Segoe UI", 9.25f);
            Place(playRow, lblLaunchHint, 0);
            btnCloseGame = ActionButton("btnCloseGame", BtnCloseGame_Click); Place(playRow, btnCloseGame, 1);
            btnRunGame = ActionButton("btnRunGame", BtnRunGame_Click, true); btnRunGame.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            Place(playRow, btnRunGame, 2);
            playBody.Controls.Add(playRow, 0, 0);
            lblServerInfo = Label("serverUnknown"); lblServerInfo.Tag = null;
            lblServerInfo.ForeColor = Color.FromArgb(170, 195, 225);
            lblServerInfo.Margin = new Padding(2, 2, 0, 0);
            playBody.Controls.Add(lblServerInfo, 0, 1);
            _actionTip.SetToolTip(txtWar3Path.InnerTextBox, txtWar3Path.TextContent);
            txtWar3Path.InnerTextBox.MouseEnter += (s, e) => _actionTip.SetToolTip(txtWar3Path.InnerTextBox, txtWar3Path.TextContent);
            UpdateActionTips();

            var status = new DarkCardPanel { Dock = DockStyle.Fill, BorderRadius = 0, BackColor = Color.FromArgb(13, 23, 37) };
            var statusRow = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16, 5, 16, 5), ColumnCount = 2, RowCount = 1 };
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            lblStatusTitle = Label("statusLabel", true); lblStatusTitle.ForeColor = accent;
            statusLabel = Label("ready"); statusLabel.Tag = null; statusLabel.ForeColor = Color.FromArgb(196, 216, 236);
            statusRow.Controls.Add(lblStatusTitle, 0, 0); statusRow.Controls.Add(statusLabel, 1, 0);
            status.Controls.Add(statusRow); shell.Controls.Add(status, 0, 4);

            AllowDrop = true;
            DragEnter += (s, e) => { if (!_launching && _activeGame == null && e.Data?.GetDataPresent(DataFormats.FileDrop) == true &&
                ((string[])e.Data.GetData(DataFormats.FileDrop)!).Any(MapSaveManager.IsMap)) e.Effect = DragDropEffects.Copy; };
            DragDrop += (s, e) => { var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
                var map = files?.FirstOrDefault(MapSaveManager.IsMap); if (!_launching && _activeGame == null && map != null) SelectMap(map); };
        }

        private DarkCardPanel Section(string key, out TableLayoutPanel body)
        {
            var card = new DarkCardPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, Padding = new Padding(13, 7, 13, 7),
                BorderColor = Color.FromArgb(42, 62, 86) };
            var frame = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            frame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            frame.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            frame.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            frame.Controls.Add(Label(key, true), 0, 0);
            body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            frame.Controls.Add(body, 0, 1);
            card.Controls.Add(frame);
            return card;
        }

        private int LanguageIndex() => _config.GetSetting("Language", "EN").ToUpperInvariant() switch
        { "VN" => 1, "CN" => 2, _ => 0 };

        private bool _suppressLanguageEvent;

        private void ChangeLanguage()
        {
            if (_suppressLanguageEvent || cboLanguage.SelectedIndex < 0) return;
            _config.SetSetting("Language", cboLanguage.SelectedItem?.ToString() ?? "EN");
            _config.SaveSettings();
            ApplyLanguage();
        }

        private void UpdateActionTips()
        {
            foreach (var button in new[] { btnBrowse, btnInGameOptions, btnCheckUpdate, btnBrowseMap,
                btnNewSlot, btnBackupSlot, btnDeleteSlot, btnRestoreSlot, btnScanPlugins,
                btnConfigYDWE, btnCloseGame, btnRunGame, btnOpenFolder })
                if (button.Tag is string key) _actionTip.SetToolTip(button, T(key));
            _actionTip.SetToolTip(btnInGameOptions, T("inGameOptionsHint"));
            _actionTip.SetToolTip(btnBrowseMap, T("browseMapHint"));
            _actionTip.SetToolTip(btnOpenFolder, T("openFolderHint"));
            _actionTip.SetToolTip(btnScanPlugins, T("scanPluginsHint"));
            _actionTip.SetToolTip(btnRestoreSlot, T("restoreSlotHint"));
            _actionTip.SetToolTip(btnDeleteSlot, T("deleteSlotHint"));
            _actionTip.SetToolTip(btnConfigYDWE, T("ydweSettingsHint"));
            if (cboPlugins.SelectedItem is PluginProfile profile)
            {
                string loader = Path.Combine(txtWar3Path.TextContent.Trim(), "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C", "bin", "YDWEConfig.exe");
                _actionTip.SetToolTip(cboPlugins, profile.IsInstalled && !File.Exists(loader) ? T("installedNeedsLoader") :
                    profile.SourcePath.Length > 0 ? profile.SourcePath : T("cleanProfile"));
            }
        }

        private void LoadInitialData()
        {
            string war3Path = _config.GetSetting("War3Path", "");
            if (string.IsNullOrEmpty(war3Path))
            {
                war3Path = RegistryHelper.GetWar3InstallPath();
            }

            if (string.IsNullOrEmpty(war3Path) || !Directory.Exists(war3Path))
            {
                string fallbackClean = @"D:\Game\Warcraft clean";
                string fallback32 = @"D:\Game\Warcraft3 1.27 DZ\Warcraft 3.2";
                if (Directory.Exists(fallbackClean)) war3Path = fallbackClean;
                else if (Directory.Exists(fallback32)) war3Path = fallback32;
            }

            txtWar3Path.TextContent = war3Path;

            LoadProfilesList(war3Path);
            RefreshMaps();

            // Display & Graphic
            string graphic = _config.GetSetting("GraphicType", "OpenGL");
            cboGraphic.SelectedIndex = graphic.Equals("DirectX", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            string display = _config.GetSetting("DisplayMode", "Full Screen");
            if (display.Contains("Window", StringComparison.OrdinalIgnoreCase) && !display.Contains("Borderless", StringComparison.OrdinalIgnoreCase))
                cboDisplay.SelectedIndex = 2; // Window
            else if (display.Contains("Borderless", StringComparison.OrdinalIgnoreCase))
                cboDisplay.SelectedIndex = 1; // Borderless
            else
                cboDisplay.SelectedIndex = 0; // Full Screen

            // Query KKWE version from server in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var ver = await _updater.CheckForUpdatesAsync();
                    if (ver != null && !this.IsDisposed)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            _cachedServerVersion = ver.Version;
                            lblServerInfo.Text = string.Format(T("serverVersion"), _cachedServerVersion);
                        }));
                    }
                }
                catch { }
            });
        }

        private void LoadProfilesList(string war3Dir)
        {
            cboPlugins.Items.Clear();
            var profiles = _pluginManager.GetAvailableProfiles(war3Dir);
            foreach (var p in profiles)
            {
                cboPlugins.Items.Add(p);
            }

            if (cboPlugins.Items.Count > 0)
            {
                string lastProfile = _config.GetSetting("LastPluginProfile", "");
                int selectIdx = profiles.FindIndex(p => p.IsInstalled);
                if (selectIdx < 0) selectIdx = 0;
                for (int i = 0; i < cboPlugins.Items.Count; i++)
                {
                    if (cboPlugins.Items[i] is PluginProfile prof)
                    {
                        if (prof.Id.Equals(lastProfile, StringComparison.OrdinalIgnoreCase) ||
                            (lastProfile.Length > 0 && prof.Id.EndsWith(":" + lastProfile, StringComparison.OrdinalIgnoreCase)) ||
                            (string.IsNullOrEmpty(lastProfile) && selectIdx == 0 && prof.Id.Contains("2606")))
                        {
                            selectIdx = i;
                            break;
                        }
                    }
                }
                cboPlugins.SelectedIndex = selectIdx;
            }
        }

        private void ScanPlugins()
        {
            string previous = (cboPlugins.SelectedItem as PluginProfile)?.Id ?? "";
            LoadProfilesList(txtWar3Path.TextContent.Trim());
            var selected = cboPlugins.Items.Cast<PluginProfile>().FirstOrDefault(p => p.Id == previous);
            if (selected != null) cboPlugins.SelectedItem = selected;
            statusLabel.Text = string.Format(T("profilesFound"), cboPlugins.Items.Count);
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = T("folderPrompt");
            fbd.SelectedPath = Directory.Exists(txtWar3Path.TextContent) ? txtWar3Path.TextContent : @"D:\Game";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                if (!File.Exists(Path.Combine(path, "war3.exe")))
                {
                    MessageBox.Show(this, T("invalidFolder"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtWar3Path.TextContent = path;
                _config.SetSetting("War3Path", path);
                _config.SaveSettings();

                LoadProfilesList(path);
                RefreshMaps();
                statusLabel.Text = T("folderUpdated");
                statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void CboPlugins_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboPlugins.SelectedItem is PluginProfile profile)
            {
                _config.SetSetting("LastPluginProfile", profile.Id);
                _config.SaveSettings();
                string loader = Path.Combine(txtWar3Path.TextContent.Trim(), "4_we_WorldEdit v1.2.9c", "WorldEdit v1.2.9C", "bin", "YDWEConfig.exe");
                _actionTip.SetToolTip(cboPlugins, profile.IsInstalled && !File.Exists(loader) ? T("installedNeedsLoader") :
                    profile.SourcePath.Length > 0 ? profile.SourcePath : T("cleanProfile"));
                if (profile.IsInstalled && !File.Exists(loader)) statusLabel.Text = T("installedNeedsLoader");
            }
        }

        private void RefreshMaps()
        {
            string previous = (cboMaps.SelectedItem as MapEntry)?.Path ?? _config.GetSetting("LastMapPath");
            cboMaps.Items.Clear();
            try { foreach (var map in _mapSaves.Scan(txtWar3Path.TextContent.Trim())) cboMaps.Items.Add(map); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            { statusLabel.Text = string.Format(T("mapScanFailed"), ex.Message); }
            if (File.Exists(previous) && MapSaveManager.IsMap(previous) &&
                !cboMaps.Items.Cast<MapEntry>().Any(m => m.Path.Equals(previous, StringComparison.OrdinalIgnoreCase)))
                cboMaps.Items.Add(new MapEntry(previous));
            var found = cboMaps.Items.Cast<MapEntry>().FirstOrDefault(m => m.Path.Equals(previous, StringComparison.OrdinalIgnoreCase));
            if (found != null) cboMaps.SelectedItem = found;
            else if (cboMaps.Items.Count > 0) cboMaps.SelectedIndex = 0;
            RefreshSlots();
        }

        private void SelectMap(string path)
        {
            if (!File.Exists(path) || !MapSaveManager.IsMap(path)) return;
            var map = cboMaps.Items.Cast<MapEntry>().FirstOrDefault(m => m.Path.Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase));
            if (map == null) { map = new MapEntry(path); cboMaps.Items.Add(map); }
            cboMaps.SelectedItem = map;
            _config.SetSetting("LastMapPath", map.Path);
            _config.SaveSettings();
        }

        private void BrowseMap(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "Warcraft III (*.w3x;*.w3m)|*.w3x;*.w3m", CheckFileExists = true,
                InitialDirectory = Directory.Exists(Path.Combine(txtWar3Path.TextContent.Trim(), "Maps")) ?
                    Path.Combine(txtWar3Path.TextContent.Trim(), "Maps") : txtWar3Path.TextContent.Trim() };
            if (dialog.ShowDialog(this) == DialogResult.OK) SelectMap(dialog.FileName);
        }

        private void RefreshSlots(string? select = null)
        {
            cboSlots.Items.Clear();
            if (cboMaps.SelectedItem is not MapEntry map)
            {
                btnDeleteSlot.Enabled = false;
                btnRestoreSlot.Enabled = false;
                return;
            }
            _config.SetSetting("LastMapPath", map.Path);
            _config.SaveSettings();
            System.Collections.Generic.List<SaveSlot> slots;
            try { slots = _mapSaves.Slots(map); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                statusLabel.Text = string.Format(T("savePathIssue"), ex.Message);
                btnDeleteSlot.Enabled = false;
                btnBackupSlot.Enabled = false;
                btnRestoreSlot.Enabled = false;
                return;
            }
            foreach (var slot in slots) cboSlots.Items.Add(slot);
            if (slots.Count > 0) cboSlots.SelectedItem = slots.FirstOrDefault(x => x.Path == select) ?? slots[0];
            btnDeleteSlot.Enabled = slots.Count > 0;
            btnBackupSlot.Enabled = slots.Count > 0;
            btnRestoreSlot.Enabled = _mapSaves.HasDeleted(map);
            string gameDir = txtWar3Path.TextContent.Trim();
            string relativeMap = Directory.Exists(gameDir) ? Path.GetRelativePath(gameDir, map.Path) : map.Name;
            if (relativeMap.Length >= 54 || map.Path.Length >= 240)
                statusLabel.Text = T("mapAliasWarning");
        }

        private void NewSlot(object? sender, EventArgs e)
        {
            if (cboMaps.SelectedItem is not MapEntry map) return;
            string? name = Microsoft.VisualBasic.Interaction.InputBox(T("slotPrompt"), T("newSlot"),
                T("defaultSlot") + " " + (cboSlots.Items.Count + 1));
            if (string.IsNullOrWhiteSpace(name)) return;
            try { var slot = _mapSaves.Create(map, name); RefreshSlots(slot.Path); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, T("errorTitle")); }
        }

        private void BackupSlot(object? sender, EventArgs e)
        {
            if (cboMaps.SelectedItem is not MapEntry map || cboSlots.SelectedItem is not SaveSlot slot) return;
            try { var backup = _mapSaves.Backup(map, slot); RefreshSlots(backup.Path); statusLabel.Text = T("backupDone"); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, T("errorTitle")); }
        }

        private void DeleteSlot(object? sender, EventArgs e)
        {
            if (cboMaps.SelectedItem is not MapEntry map || cboSlots.SelectedItem is not SaveSlot slot) return;
            if (MessageBox.Show(this, string.Format(T("deleteSlotConfirm"), slot), T("noticeTitle"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                _mapSaves.Delete(map, slot);
                RefreshSlots();
                statusLabel.Text = T("slotMovedToTrash");
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, T("errorTitle")); }
        }

        private void RestoreSlot(object? sender, EventArgs e)
        {
            if (cboMaps.SelectedItem is not MapEntry map) return;
            try
            {
                var slot = _mapSaves.RestoreLatestDeleted(map);
                RefreshSlots(slot.Path);
                statusLabel.Text = T("slotRestored");
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, T("errorTitle")); }
        }

        private async void BtnRunGame_Click(object? sender, EventArgs e)
        {
            string war3Dir = txtWar3Path.TextContent.Trim();
            if (!Directory.Exists(war3Dir) || !File.Exists(Path.Combine(war3Dir, "war3.exe")))
            {
                MessageBox.Show(this, T("invalidFolder"), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_launching || _activeGame != null) { MessageBox.Show(this, T("sessionBusy"), T("noticeTitle")); return; }
            if (cboMaps.SelectedItem is not MapEntry map || cboSlots.SelectedItem is not SaveSlot slot)
            { MessageBox.Show(this, T("selectMapSlot"), T("noticeTitle")); return; }
            if (!File.Exists(map.Path)) { MessageBox.Show(this, T("mapMissing"), T("errorTitle")); return; }
            if (!MapSaveManager.IsMap(map.Path)) { MessageBox.Show(this, T("mapMissing"), T("errorTitle")); return; }
            try
            {
                if (new FileInfo(map.Path).Length == 0)
                { MessageBox.Show(this, T("emptyMap"), T("errorTitle")); return; }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, T("errorTitle")); return; }
            if (Path.GetFullPath(Path.Combine(war3Dir, "Maps", "WPM")).Length > 225)
            { MessageBox.Show(this, T("gamePathTooLong"), T("errorTitle")); return; }
            if (map.Path.Length >= 240 && MessageBox.Show(this, T("sourcePathLong"), T("noticeTitle"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (Process.GetProcessesByName("war3").Any())
            { MessageBox.Show(this, T("sessionBusy"), T("noticeTitle")); return; }
            var profile = cboPlugins.SelectedItem as PluginProfile;

            string graphic = cboGraphic.SelectedIndex == 1 ? "DirectX" : "OpenGL";
            string display = cboDisplay.SelectedIndex switch
            {
                1 => "Borderless Windowed",
                2 => "Window",
                _ => "Full Screen"
            };
            int instances = 1; // one root INI cannot isolate simultaneous instances

            _config.SetSetting("War3Path", war3Dir);
            _config.SetSetting("GraphicType", graphic);
            _config.SetSetting("DisplayMode", display);
            _config.SetSetting("RunInstances", instances.ToString());
            _config.SaveSettings();

            var launchOpts = new LaunchOptions
            {
                War3Path = war3Dir,
                MapPath = map.Path,
                GraphicType = graphic,
                DisplayMode = display,
                Instances = instances,
                UserName = _config.GetSetting("UserName", "MrP"),
                LockMouse = _config.GetSetting("LockMouse", "1") == "1",
                FixRatio = _config.GetSetting("FixRatio", "1") == "1",
                WideScreen = _config.GetSetting("WideScreen", "1") == "1",
                FastLoad = _config.GetSetting("FastLoad", "1") == "1",
                MuteSaveValue = _config.GetSetting("MuteSaveValue", "1") == "1",
                IsCleanProfile = profile?.IsClean ?? false,
                IsInstalledProfile = profile?.IsInstalled ?? false
            };

            SaveSession? session = null;
            Process? startedGame = null;
            bool profileApplied = false;
            _launching = true;
            SetSessionControlsEnabled(false);
            statusLabel.Text = T("launching");
            try
            {
                if (profile != null && !profile.IsInstalled &&
                    !await Task.Run(() => _pluginManager.ApplyProfile(war3Dir, profile)))
                    throw new IOException(T("pluginFailed"));
                profileApplied = profile != null && !profile.IsInstalled;
                session = _mapSaves.Prepare(war3Dir, map, slot);
                bool ok = await Task.Run(() => War3Launcher.Launch(launchOpts, null,
                    process => startedGame = process));
                if (!ok || startedGame == null) throw new IOException(T("launchFailed"));
                _activeGame = startedGame;
            }
            catch (Exception ex)
            {
                // Preserve the selected slot if the loader failed before the game started.
                try { session?.Rollback(); } catch { }
                string recovery = "";
                if (profileApplied && !Process.GetProcessesByName("war3").Any())
                {
                    try { await Task.Run(() => _pluginManager.UndoLastRootSwitch(war3Dir)); }
                    catch (Exception restoreError) { recovery = "\n" + string.Format(T("pluginRestoreFailed"), restoreError.Message); }
                }
                CleanupStagedMap(launchOpts.StagedMapPath);
                _activeGame = null;
                MessageBox.Show(this, ex.Message + recovery + (session == null ? "" : "\n" + T("rootPreserved")), T("errorTitle"));
                return;
            }
            finally
            {
                _launching = false;
                if (_activeGame == null) SetSessionControlsEnabled(true);
            }
            statusLabel.Text = string.Format(T("gameRunning"), _activeGame.Id);
            _ = WatchGameAsync(_activeGame, session!, launchOpts.StagedMapPath);
            if (launchOpts.MuteSaveValue)
            {
                _saveValueMuter.Start(msg =>
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke(new Action(() => { if (!IsDisposed) statusLabel.Text = T("saveValueMuted"); }));
                    }
                    else
                    {
                        statusLabel.Text = T("saveValueMuted");
                    }
                });
            }
        }

        private void SetSessionControlsEnabled(bool enabled)
        {
            btnRunGame.Enabled = enabled;
            btnNewSlot.Enabled = enabled;
            btnBackupSlot.Enabled = enabled && cboSlots.Items.Count > 0;
            btnDeleteSlot.Enabled = enabled && cboSlots.Items.Count > 0;
            btnRestoreSlot.Enabled = enabled && cboMaps.SelectedItem is MapEntry map && _mapSaves.HasDeleted(map);
            btnScanPlugins.Enabled = enabled;
            btnConfigYDWE.Enabled = enabled;
            cboMaps.Enabled = enabled;
            cboPlugins.Enabled = enabled;
            btnInGameOptions.Enabled = enabled;
            cboSlots.Enabled = enabled;
            btnBrowseMap.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            btnOpenFolder.Enabled = enabled;
            txtWar3Path.Enabled = enabled;
            cboLanguage.Enabled = enabled;
            btnCheckUpdate.Enabled = enabled;
        }

        private static void CleanupStagedMap(string path)
        {
            try { if (path.Length > 0 && File.Exists(path)) File.Delete(path); }
            catch { /* An antivirus or game may still have the map open. */ }
        }

        private async Task WatchGameAsync(Process game, SaveSession session, string stagedMapPath)
        {
            try
            {
                await game.WaitForExitAsync();
                // Give the plugin a short moment to flush its INI after process exit.
                await Task.Delay(500);
                session.Sync();
                if (!IsDisposed) statusLabel.Text = T("syncDone");
            }
            catch (Exception ex)
            {
                if (!IsDisposed) MessageBox.Show(this, T("syncFailed") + "\n" + ex.Message,
                    T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _saveValueMuter.Stop();
                CleanupStagedMap(stagedMapPath);
                game.Dispose();
                _activeGame = null;
                if (!IsDisposed)
                {
                    try { LoadProfilesList(txtWar3Path.TextContent.Trim()); }
                    catch (IOException) { /* The game folder may have been removed while playing. */ }
                    SetSessionControlsEnabled(true);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_launching || _activeGame != null)
            {
                e.Cancel = true;
                MessageBox.Show(this, T("keepOpen"), T("noticeTitle"));
                return;
            }
            base.OnFormClosing(e);
        }

        private void BtnCloseGame_Click(object? sender, EventArgs e)
        {
            if (_activeGame != null && !_activeGame.HasExited) _activeGame.Kill();
            statusLabel.Text = T("gameClosed");
            statusLabel.ForeColor = Color.FromArgb(230, 126, 34);
        }

        private void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            string war3Dir = txtWar3Path.TextContent.Trim();
            if (Directory.Exists(war3Dir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{war3Dir}\"",
                    UseShellExecute = true
                });
            }
        }

        private void BtnConfigYDWE_Click(object? sender, EventArgs e)
        {
            string war3Dir = txtWar3Path.TextContent.Trim();
            if (Directory.Exists(war3Dir))
            {
                if (!War3Launcher.OpenYDWEConfig(war3Dir))
                {
                    MessageBox.Show(this, T("ydweMissing"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void BtnCheckUpdate_Click(object? sender, EventArgs e)
        {
            statusLabel.Text = T("checkingUpdate");
            statusLabel.ForeColor = Color.FromArgb(52, 152, 219);
            try
            {
                var info = await _updater.CheckForUpdatesAsync();
                if (info != null)
                {
                    _cachedServerVersion = info.Version;
                    lblServerInfo.Text = string.Format(T("serverVersion"), _cachedServerVersion);
                    using var uf = new UpdateForm(info, _updater, _appDir);
                    uf.ShowDialog(this);
                    LoadProfilesList(txtWar3Path.TextContent.Trim());
                    statusLabel.Text = T("updateChecked");
                    statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
                }
                else
                {
                    MessageBox.Show(this, T("updateUnavailable"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    statusLabel.Text = T("updateChecked");
                    statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(T("updateError"), ex.Message), T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyLanguage()
        {
            Text = T("appName");
            void Visit(Control root)
            {
                foreach (Control c in root.Controls)
                {
                    if (c.Tag is string key) c.Text = T(key);
                    Visit(c);
                }
            }
            Visit(this);
            int graphic = cboGraphic.SelectedIndex, display = cboDisplay.SelectedIndex;
            cboGraphic.Items[0] = T("openGL");
            cboDisplay.Items[0] = T("fullScreen");
            cboDisplay.Items[1] = T("borderless");
            cboDisplay.Items[2] = T("windowed");
            cboGraphic.SelectedIndex = graphic; cboDisplay.SelectedIndex = display;
            cboPlugins.Invalidate();
            UpdateActionTips();
            statusLabel.Text = T("ready");

            lblServerInfo.Text = _cachedServerVersion != null ?
                string.Format(T("serverVersion"), _cachedServerVersion) : T("serverUnknown");

            int expectedLang = LanguageIndex();
            if (cboLanguage != null && cboLanguage.SelectedIndex != expectedLang)
            {
                _suppressLanguageEvent = true;
                try { cboLanguage.SelectedIndex = expectedLang; }
                finally { _suppressLanguageEvent = false; }
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var sf = new SettingsForm(_config);
            if (sf.ShowDialog(this) == DialogResult.OK)
            {
                ApplyLanguage();
                statusLabel.Text = T("settingsSaved");
                statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mapTip.Dispose();
                _actionTip.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
