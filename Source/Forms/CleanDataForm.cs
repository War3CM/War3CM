using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Phanmemwar3.Core;

namespace Phanmemwar3.Forms
{
    public class CleanDataForm : Form
    {
        private readonly ConfigManager _config;
        private readonly string _war3Dir;
        private readonly string _appDir;
        private string T(string key) => _config.GetText(key, _config.GetSetting("Language", "EN"));

        private List<CleanCategoryInfo> _categories = new();
        private readonly Dictionary<CleanCategory, ModernCheckBox> _checkMap = new();
        private readonly Dictionary<CleanCategory, Label> _sizeLabelMap = new();

        private ModernButton btnSelectAll;
        private ModernButton btnDeselectAll;
        private ModernButton btnClean;
        private ModernButton btnClose;

        public CleanDataForm(ConfigManager config, string war3Dir)
        {
            _config = config;
            _war3Dir = war3Dir;
            _appDir = AppDomain.CurrentDomain.BaseDirectory;
            Icon = AppIcons.GetAppIcon();
            InitializeComponent();
            ScanData();
        }

        private void InitializeComponent()
        {
            Text = T("cleanTitle");
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(540, 520);
            MinimumSize = new Size(540, 520);
            MaximumSize = new Size(540, 520);
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

            // Title Bar
            var titleBar = new ModernTitleBar(this, T("cleanTitle"), showMin: false, showMax: false) { Dock = DockStyle.Fill };
            shell.Controls.Add(titleBar, 0, 0);

            // Footer
            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 28, 43),
                Padding = new Padding(18, 10, 18, 10),
                ColumnCount = 2
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

            btnClose = new ModernButton
            {
                Text = T("btnClose"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 0),
                BorderColor = Color.FromArgb(50, 75, 105)
            };
            btnClose.Click += (s, e) => Close();

            btnClean = new ModernButton
            {
                Text = T("btnCleanNow"),
                Dock = DockStyle.Fill,
                Margin = new Padding(8, 0, 0, 0),
                BackColorNormal = Color.FromArgb(180, 42, 42),
                BackColorHover = Color.FromArgb(215, 55, 55),
                GradientEndColor = Color.FromArgb(130, 25, 25),
                BorderColor = Color.FromArgb(235, 75, 75),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnClean.Click += BtnClean_Click;

            footer.Controls.Add(btnClose, 0, 0);
            footer.Controls.Add(btnClean, 1, 0);
            shell.Controls.Add(footer, 0, 2);

            // Scroller Body
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

            // Banner Card
            var banner = new DarkCardPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(14),
                BackColor = Color.FromArgb(18, 28, 44),
                BorderColor = Color.FromArgb(38, 64, 98),
                BorderRadius = 8
            };
            var bannerLayout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            var lblTitle = new Label
            {
                Text = "🧹 " + T("cleanTitle"),
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 180, 255),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblSubtitle = new Label
            {
                Text = T("cleanSubtitle"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 225),
                AutoSize = true
            };
            bannerLayout.Controls.Add(lblTitle);
            bannerLayout.Controls.Add(lblSubtitle);
            banner.Controls.Add(bannerLayout);
            stack.Controls.Add(banner);

            // Quick Selection Toolbar
            var toolBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 34,
                ColumnCount = 3,
                Margin = new Padding(0, 0, 0, 10)
            };
            toolBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            toolBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            btnSelectAll = new ModernButton
            {
                Text = T("cleanSelectAll"),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 5, 0),
                BorderColor = Color.FromArgb(50, 75, 105),
                Font = new Font("Segoe UI", 8.75f)
            };
            btnSelectAll.Click += (s, e) => SetAllChecked(true);

            btnDeselectAll = new ModernButton
            {
                Text = T("cleanDeselectAll"),
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 0, 0, 0),
                BorderColor = Color.FromArgb(50, 75, 105),
                Font = new Font("Segoe UI", 8.75f)
            };
            btnDeselectAll.Click += (s, e) => SetAllChecked(false);

            toolBar.Controls.Add(btnSelectAll, 0, 0);
            toolBar.Controls.Add(btnDeselectAll, 1, 0);
            stack.Controls.Add(toolBar);

            // Items Container Card
            var itemsCard = new DarkCardPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(12),
                BackColor = Color.FromArgb(16, 25, 38),
                BorderColor = Color.FromArgb(32, 50, 75),
                BorderRadius = 8
            };
            var itemsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            itemsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Categories
            var categories = new[]
            {
                CleanCategory.MapCache,
                CleanCategory.SaveTrashAndHistory,
                CleanCategory.GameLogsAndTemp,
                CleanCategory.PluginBackups,
                CleanCategory.OldBackupSlots
            };

            foreach (var cat in categories)
            {
                var rowCard = CreateCategoryRow(cat);
                itemsLayout.Controls.Add(rowCard);
            }

            itemsCard.Controls.Add(itemsLayout);
            stack.Controls.Add(itemsCard);
        }

        private Control CreateCategoryRow(CleanCategory cat)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 8),
                Padding = new Padding(6)
            };

            var topRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 28,
                ColumnCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            topRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            topRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            string titleKey = cat switch
            {
                CleanCategory.MapCache => "cleanCatMapCache",
                CleanCategory.SaveTrashAndHistory => "cleanCatTrashHistory",
                CleanCategory.GameLogsAndTemp => "cleanCatGameTemp",
                CleanCategory.PluginBackups => "cleanCatPluginBackups",
                CleanCategory.OldBackupSlots => "cleanCatBackupSlots",
                _ => ""
            };

            var chk = new ModernCheckBox
            {
                Text = T(titleKey),
                Dock = DockStyle.Fill,
                Checked = true,
                ForeColor = Color.FromArgb(235, 245, 255),
                Font = new Font("Segoe UI", 9.25f, FontStyle.Bold)
            };
            _checkMap[cat] = chk;

            var lblSize = new Label
            {
                Text = "...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(245, 185, 45),
                Font = new Font("Segoe UI", 8.75f, FontStyle.Bold)
            };
            _sizeLabelMap[cat] = lblSize;

            topRow.Controls.Add(chk, 0, 0);
            topRow.Controls.Add(lblSize, 1, 0);

            string descKey = cat switch
            {
                CleanCategory.MapCache => "cleanCatMapCacheDesc",
                CleanCategory.SaveTrashAndHistory => "cleanCatTrashHistoryDesc",
                CleanCategory.GameLogsAndTemp => "cleanCatGameTempDesc",
                CleanCategory.PluginBackups => "cleanCatPluginBackupsDesc",
                CleanCategory.OldBackupSlots => "cleanCatBackupSlotsDesc",
                _ => ""
            };

            var lblDesc = new Label
            {
                Text = T(descKey),
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = Color.FromArgb(145, 170, 195),
                Font = new Font("Segoe UI", 8.5f),
                Margin = new Padding(24, 0, 0, 6)
            };

            panel.Controls.Add(lblDesc);
            panel.Controls.Add(topRow);
            return panel;
        }

        private void SetAllChecked(bool isChecked)
        {
            foreach (var chk in _checkMap.Values)
            {
                chk.Checked = isChecked;
            }
        }

        private void ScanData()
        {
            _categories = DataCleaner.Scan(_appDir, _war3Dir);
            foreach (var cat in _categories)
            {
                if (_sizeLabelMap.TryGetValue(cat.Category, out var lbl))
                {
                    lbl.Text = cat.FileCount > 0 ? $"{cat.FileCount} files ({cat.SizeDisplay})" : "0 files (0 B)";
                    lbl.ForeColor = cat.FileCount > 0 ? Color.FromArgb(245, 185, 45) : Color.FromArgb(120, 140, 160);
                }
            }
        }

        private void BtnClean_Click(object? sender, EventArgs e)
        {
            var selected = _categories.Where(c => _checkMap.TryGetValue(c.Category, out var chk) && chk.Checked).ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show(this, T("cleanNoSelection"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int totalFiles = selected.Sum(c => c.FileCount);
            if (totalFiles == 0)
            {
                MessageBox.Show(this, T("cleanZeroFiles"), T("noticeTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show(this, T("cleanConfirm"), T("cleanTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var report = DataCleaner.ExecuteClean(selected);
                MessageBox.Show(this, string.Format(T("cleanSuccess"), report.DeletedFiles, report.FreedSizeDisplay), T("cleanTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ScanData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, T("errorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
