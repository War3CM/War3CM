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
        private ModernComboBox cboInstances;
        private ModernComboBox cboDisplay;
        private ModernCheckBox chkMuteSaveValue;
        private ModernButton btnRunGame;
        private ModernButton btnCloseGame;
        private ModernButton btnOpenFolder;
        private ModernButton btnConfigYDWE;
        private ModernButton btnCheckUpdate;
        private ModernButton btnSettings;
        private Label statusLabel;
        private Label lblServerInfo;
        private Panel contentViewport;
        private FlowLayoutPanel cardStack;
        private DarkCardPanel saveCard;
        private TableLayoutPanel saveBody;
        private TableLayoutPanel slotRow;
        private TableLayoutPanel playRow;
        private DarkCardPanel playCard;
        private bool compactLayout;
        private bool updatingLayout;

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
            Text = T("appName");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(760, 704);
            MinimumSize = new Size(580, 470);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(12, 18, 29);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            Shown += (s, e) =>
            {
                Rectangle area = Screen.FromControl(this).WorkingArea;
                int width = Math.Max(400, area.Width - 16);
                int height = Math.Max(400, area.Height - 16);
                MinimumSize = new Size(Math.Min(580, width), Math.Min(470, height));
                Size = new Size(Math.Min(Width, width), Math.Min(Height, height));
                Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
            };

            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty, Padding = Padding.Empty };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            Controls.Add(shell);
            var titleBar = new ModernTitleBar(this, T("appName"), showMin: true, showMax: true);
            shell.Controls.Add(titleBar, 0, 0);
            var status = new DarkCardPanel { Dock = DockStyle.Fill, BorderRadius = 0, BackColor = Color.FromArgb(13, 23, 37) };
            var statusRow = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(17, 5, 14, 5), ColumnCount = 2, RowCount = 1 };
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            statusRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            statusRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lblStatusTitle = new Label { Text = T("statusLabel"), Tag = "statusLabel", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, ForeColor = accent, UseMnemonic = false,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            statusLabel = new Label { Text = T("ready"), AutoEllipsis = true, UseMnemonic = false, ForeColor = Color.FromArgb(193, 215, 237),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            statusRow.Controls.Add(lblStatusTitle, 0, 0);
            statusRow.Controls.Add(statusLabel, 1, 0);
            status.Controls.Add(statusRow);
            shell.Controls.Add(status, 0, 3);

            var content = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BackColor, Padding = new Padding(16, 0, 16, 4) };
            contentViewport = content;
            shell.Controls.Add(content, 0, 1);
            var stack = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false,
                AutoScroll = false, Dock = DockStyle.Top, Padding = Padding.Empty, Margin = Padding.Empty };
            content.Controls.Add(stack);
            cardStack = stack;
            content.Resize += (s, e) => {
                int width = Math.Max(0, content.ClientSize.Width - content.Padding.Horizontal - 2);
                stack.Width = width;
                foreach (Control item in stack.Controls) item.Width = width;
            };

            var hero = new DarkCardPanel { Height = 82, Margin = new Padding(0, 9, 0, 9), Padding = new Padding(16, 7, 16, 7), BackColor = Color.FromArgb(19, 36, 58), BorderColor = Color.FromArgb(40, 75, 110) };
            stack.Controls.Add(hero);
            var heroGrid = Grid(66, 0, 218);
            hero.Controls.Add(heroGrid);
            var heroText = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
            heroText.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            lblHeaderTitle = Label("heroTitle", true); lblHeaderTitle.Text = HeaderText("heroTitle"); lblHeaderTitle.Tag = null;
            lblHeaderTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblHeaderSubtitle = Label("appDescription"); lblHeaderSubtitle.Text = HeaderText("appDescription"); lblHeaderSubtitle.Tag = null;
            var chip = Label("heroHint"); chip.Text = HeaderText("heroHint"); chip.Tag = null; chip.ForeColor = accent;
            heroText.Controls.Add(lblHeaderTitle, 0, 0); heroText.Controls.Add(lblHeaderSubtitle, 0, 1); heroText.Controls.Add(chip, 0, 2);
            Place(heroGrid, heroText, 0);
            var heroTools = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
            heroTools.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); heroTools.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            heroTools.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); heroTools.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            cboLanguage = new ModernComboBox { Dock = DockStyle.Fill, Margin = new Padding(4) };
            cboLanguage.Items.AddRange(new object[] { "EN", "VN", "CN" });
            cboLanguage.SelectedIndex = LanguageIndex();
            cboLanguage.SelectedIndexChanged += (s, e) => ChangeLanguage();
            heroTools.Controls.Add(cboLanguage, 0, 0);
            btnSettings = ActionButton("btnSettings", BtnSettings_Click); heroTools.Controls.Add(btnSettings, 1, 0);
            btnCheckUpdate = ActionButton("btnCheckUpdate", BtnCheckUpdate_Click); heroTools.Controls.Add(btnCheckUpdate, 0, 1);
            heroTools.SetColumnSpan(btnCheckUpdate, 2);
            Place(heroGrid, heroTools, 1);

            var game = Card("gameLibrary", 110); stack.Controls.Add(game);
            var gameBody = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            gameBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            gameBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            gameBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            game.Controls.Add(gameBody); gameBody.BringToFront();
            gameBody.Controls.Add(Label("selectWar3Folder"), 0, 0);
            var pathRow = Grid(34, 0, 90, 103);
            txtWar3Path = new ModernTextBox(); Place(pathRow, txtWar3Path, 0);
            txtWar3Path.InnerTextBox.Leave += (s, e) =>
            {
                string selected = txtWar3Path.TextContent.Trim();
                if (_launching || _activeGame != null || !File.Exists(Path.Combine(selected, "war3.exe"))) return;
                if (selected.Equals(_config.GetSetting("War3Path"), StringComparison.OrdinalIgnoreCase)) return;
                _config.SetSetting("War3Path", selected);
                _config.SaveSettings();
                LoadProfilesList(selected);
                RefreshMaps();
            };
            btnBrowse = ActionButton("browse", BtnBrowse_Click); Place(pathRow, btnBrowse, 1);
            btnOpenFolder = ActionButton("btnOpenFolder", BtnOpenFolder_Click); Place(pathRow, btnOpenFolder, 2);
            gameBody.Controls.Add(pathRow, 0, 1);
            lblPathHint = Label("pathHint"); gameBody.Controls.Add(lblPathHint, 0, 2);

            var saves = Card("mapSaveTitle", 144); saveCard = saves; stack.Controls.Add(saves);
            saveBody = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            saveBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); saveBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            saves.Controls.Add(saveBody); saveBody.BringToFront();
            saveBody.Controls.Add(Label("selectMap"), 0, 0);
            var mapRow = Grid(32, 0, 112);
            cboMaps = new ModernComboBox();
            cboMaps.DisplayText = item => item is MapEntry entry ?
                entry.Name + "  ·  " + Path.GetFileName(Path.GetDirectoryName(entry.Path)) : item?.ToString() ?? "";
            cboMaps.SelectedIndexChanged += (s, e) => {
                RefreshSlots();
                _mapTip.SetToolTip(cboMaps, (cboMaps.SelectedItem as MapEntry)?.Path ?? "");
            };
            cboMaps.DropDown += (s, e) => cboMaps.DropDownWidth = Math.Min(
                Screen.FromControl(cboMaps).WorkingArea.Width - 24, Math.Max(cboMaps.Width, LogicalToDeviceUnits(480)));
            Place(mapRow, cboMaps, 0);
            btnBrowseMap = ActionButton("browseMap", BrowseMap); Place(mapRow, btnBrowseMap, 1);
            saveBody.Controls.Add(mapRow, 0, 1);
            saveBody.Controls.Add(Label("selectSlot"), 0, 2);
            slotRow = Grid(34, 0, 80, 86, 82, 91);
            cboSlots = new ModernComboBox(); Place(slotRow, cboSlots, 0);
            btnNewSlot = ActionButton("newSlot", NewSlot); Place(slotRow, btnNewSlot, 1);
            btnBackupSlot = ActionButton("backupSlot", BackupSlot); Place(slotRow, btnBackupSlot, 2);
            btnDeleteSlot = ActionButton("deleteSlot", DeleteSlot); Place(slotRow, btnDeleteSlot, 3);
            btnRestoreSlot = ActionButton("restoreSlot", RestoreSlot); Place(slotRow, btnRestoreSlot, 4);
            saveBody.Controls.Add(slotRow, 0, 3);

            var plugins = Card("pluginOptions", 142); stack.Controls.Add(plugins);
            var pluginBody = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
            pluginBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); pluginBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            pluginBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 18)); pluginBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            plugins.Controls.Add(pluginBody); pluginBody.BringToFront();
            pluginBody.Controls.Add(Label("selectPlugin"), 0, 0);
            var pluginRow = Grid(32, 0, 85, 104);
            cboPlugins = new ModernComboBox(); cboPlugins.DisplayText = item => item is PluginProfile p ?
                (p.IsClean ? T("cleanProfile") : p.IsInstalled ? T("installedProfile") : p.DisplayName + " (" +
                    T(p.Origin == "app" ? "appProfile" : p.Origin == "backup" ? "backupProfile" : "gameProfile") + ")") : item?.ToString() ?? "";
            cboPlugins.SelectedIndexChanged += CboPlugins_SelectedIndexChanged; Place(pluginRow, cboPlugins, 0);
            btnScanPlugins = ActionButton("scanPlugins", (s, e) => ScanPlugins()); Place(pluginRow, btnScanPlugins, 1);
            btnConfigYDWE = ActionButton("btnConfig", BtnConfigYDWE_Click); Place(pluginRow, btnConfigYDWE, 2);
            pluginBody.Controls.Add(pluginRow, 0, 1);
            pluginBody.Controls.Add(Label("quickOptions"), 0, 2);
            var optionRow = Grid(32, 0, 0, 108);
            cboGraphic = new ModernComboBox(); cboGraphic.Items.AddRange(new object[] { T("openGL"), "DirectX" }); cboGraphic.SelectedIndex = 0; Place(optionRow, cboGraphic, 0);
            cboDisplay = new ModernComboBox(); cboDisplay.Items.AddRange(new object[] { T("fullScreen"), T("borderless"), T("windowed") }); cboDisplay.SelectedIndex = 0; Place(optionRow, cboDisplay, 1);
            cboInstances = new ModernComboBox(); cboInstances.Items.AddRange(new object[] { string.Format(T("instance"), 1) }); cboInstances.SelectedIndex = 0;
            cboInstances.Enabled = false; Place(optionRow, cboInstances, 2);
            new ToolTip().SetToolTip(cboInstances, T("instanceHint"));
            pluginBody.Controls.Add(optionRow, 0, 3);

            var play = Card("launchTitle", 86); playCard = play;
            play.Margin = new Padding(16, 3, 16, 4);
            play.BackColor = Color.FromArgb(19, 35, 53);
            play.BorderColor = Color.FromArgb(42, 80, 117);
            shell.Controls.Add(play, 0, 2);
            play.Dock = DockStyle.Fill;
            var playBody = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            playBody.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            playBody.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            play.Controls.Add(playBody); playBody.BringToFront();
            playRow = Grid(38, 0, 142, 202);
            chkMuteSaveValue = new ModernCheckBox { Text = T("muteShort"), Tag = "muteShort", Checked = true };
            Place(playRow, chkMuteSaveValue, 0);
            btnCloseGame = ActionButton("btnCloseGame", BtnCloseGame_Click); Place(playRow, btnCloseGame, 1);
            btnRunGame = ActionButton("btnRunGame", BtnRunGame_Click, true); btnRunGame.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            Place(playRow, btnRunGame, 2);
            playBody.Controls.Add(playRow, 0, 0);
            lblServerInfo = Label("serverUnknown");
            playBody.Controls.Add(lblServerInfo, 0, 1);
            _actionTip.SetToolTip(chkMuteSaveValue, T("muteShort"));
            _actionTip.SetToolTip(txtWar3Path.InnerTextBox, txtWar3Path.TextContent);
            txtWar3Path.InnerTextBox.MouseEnter += (s, e) =>
                _actionTip.SetToolTip(txtWar3Path.InnerTextBox, txtWar3Path.TextContent);

            UpdateActionTips();

            AllowDrop = true;
            DragEnter += (s, e) => { if (!_launching && _activeGame == null && e.Data?.GetDataPresent(DataFormats.FileDrop) == true &&
                ((string[])e.Data.GetData(DataFormats.FileDrop)!).Any(MapSaveManager.IsMap)) e.Effect = DragDropEffects.Copy; };
            DragDrop += (s, e) => { var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
                var map = files?.FirstOrDefault(MapSaveManager.IsMap); if (!_launching && _activeGame == null && map != null) SelectMap(map); };
            stack.Width = ClientSize.Width - 36;
            foreach (Control item in stack.Controls) item.Width = stack.Width;
            stack.Height = stack.Controls.Cast<Control>().Sum(c => c.Height + c.Margin.Vertical) + 2;
            content.Resize += (s, e) => UpdateResponsiveLayout();
            UpdateResponsiveLayout();
        }

        private int LanguageIndex() => _config.GetSetting("Language", "EN").ToUpperInvariant() switch
        { "VN" => 1, "CN" => 2, _ => 0 };

        private void UpdateResponsiveLayout()
        {
            if (updatingLayout || cardStack == null || slotRow == null) return;
            updatingLayout = true;
            try
            {
                int width = Math.Max(1, contentViewport.ClientSize.Width - contentViewport.Padding.Horizontal - 2);
                bool compact = width < LogicalToDeviceUnits(700);
                if (compact != compactLayout)
                {
                    cardStack.SuspendLayout();
                    slotRow.SuspendLayout();
                    compactLayout = compact;
                    slotRow.RowCount = compact ? 2 : 1;
                    slotRow.RowStyles.Clear();
                    slotRow.RowStyles.Add(new RowStyle(SizeType.Percent, compact ? 50 : 100));
                    if (compact) slotRow.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                    if (compact)
                    {
                        slotRow.ColumnStyles[1].Width = LogicalToDeviceUnits(88);
                        slotRow.ColumnStyles[2].Width = LogicalToDeviceUnits(100);
                        slotRow.ColumnStyles[3].Width = 0;
                        slotRow.ColumnStyles[4].Width = 0;
                        slotRow.SetCellPosition(btnDeleteSlot, new TableLayoutPanelCellPosition(1, 1));
                        slotRow.SetCellPosition(btnRestoreSlot, new TableLayoutPanelCellPosition(2, 1));
                    }
                    else
                    {
                        slotRow.ColumnStyles[1].Width = LogicalToDeviceUnits(80);
                        slotRow.ColumnStyles[2].Width = LogicalToDeviceUnits(86);
                        slotRow.ColumnStyles[3].Width = LogicalToDeviceUnits(82);
                        slotRow.ColumnStyles[4].Width = LogicalToDeviceUnits(91);
                        slotRow.SetCellPosition(btnDeleteSlot, new TableLayoutPanelCellPosition(3, 0));
                        slotRow.SetCellPosition(btnRestoreSlot, new TableLayoutPanelCellPosition(4, 0));
                    }
                    slotRow.Height = LogicalToDeviceUnits(compact ? 72 : 34);
                    saveCard.Height = LogicalToDeviceUnits(compact ? 182 : 144);
                    slotRow.ResumeLayout(true);
                    cardStack.ResumeLayout(true);
                }
                cardStack.Width = width;
                foreach (Control item in cardStack.Controls) item.Width = width;
                cardStack.Height = cardStack.Controls.Cast<Control>().Sum(c => c.Height + c.Margin.Vertical) + 2;
            }
            finally { updatingLayout = false; }
        }

        private void ChangeLanguage()
        {
            if (cboLanguage.SelectedIndex < 0) return;
            _config.SetSetting("Language", cboLanguage.SelectedItem?.ToString() ?? "EN");
            _config.SaveSettings();
            ApplyLanguage();
        }

        private void UpdateActionTips()
        {
            foreach (var button in new[] { btnBrowse, btnSettings, btnCheckUpdate, btnBrowseMap,
                btnNewSlot, btnBackupSlot, btnDeleteSlot, btnRestoreSlot, btnScanPlugins,
                btnConfigYDWE, btnCloseGame, btnRunGame, btnOpenFolder })
                if (button.Tag is string key) _actionTip.SetToolTip(button, T(key));
            _actionTip.SetToolTip(chkMuteSaveValue, T("muteShort"));
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

            int instances = 1;
            int.TryParse(_config.GetSetting("RunInstances", "1"), out instances);
            if (instances >= 1 && instances <= 4)
                cboInstances.SelectedIndex = instances - 1;

            chkMuteSaveValue.Checked = _config.GetSetting("MuteSaveValue", "1") == "1";

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
                            lblServerInfo.Text = string.Format(T("serverVersion"), ver.Version);
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
            _config.SetSetting("MuteSaveValue", chkMuteSaveValue.Checked ? "1" : "0");
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
                MuteSaveValue = chkMuteSaveValue.Checked,
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
            btnSettings.Enabled = enabled;
            cboSlots.Enabled = enabled;
            cboInstances.Enabled = enabled;
            btnBrowseMap.Enabled = enabled;
            btnBrowse.Enabled = enabled;
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
                    lblServerInfo.Text = string.Format(T("serverVersion"), info.Version);
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
            cboInstances.Items[0] = string.Format(T("instance"), 1);
            cboGraphic.SelectedIndex = graphic; cboDisplay.SelectedIndex = display;
            cboPlugins.Invalidate();
            UpdateActionTips();
            statusLabel.Text = T("ready");
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var sf = new SettingsForm(_config);
            if (sf.ShowDialog(this) == DialogResult.OK)
            {
                chkMuteSaveValue.Checked = _config.GetSetting("MuteSaveValue", "1") == "1";
                ApplyLanguage();
                statusLabel.Text = T("settingsSaved");
                statusLabel.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != 0x0084 || WindowState != FormWindowState.Normal || m.Result != (IntPtr)1) return;
            long point = m.LParam.ToInt64();
            Point client = PointToClient(new Point(unchecked((short)point), unchecked((short)(point >> 16))));
            int edge = LogicalToDeviceUnits(7);
            bool left = client.X < edge, right = client.X >= ClientSize.Width - edge;
            bool top = client.Y < edge, bottom = client.Y >= ClientSize.Height - edge;
            if (top && left) m.Result = (IntPtr)13;
            else if (top && right) m.Result = (IntPtr)14;
            else if (bottom && left) m.Result = (IntPtr)16;
            else if (bottom && right) m.Result = (IntPtr)17;
            else if (left) m.Result = (IntPtr)10;
            else if (right) m.Result = (IntPtr)11;
            else if (top) m.Result = (IntPtr)12;
            else if (bottom) m.Result = (IntPtr)15;
        }
    }
}
