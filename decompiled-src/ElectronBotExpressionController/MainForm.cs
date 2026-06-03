using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ElectronBot.DotNet;
using ElectronBot.DotNet.LibUsb;
using ElectronBot.DotNet.WinUsb;
using Microsoft.Extensions.Logging.Abstractions;

namespace ElectronBotExpressionController;

internal sealed partial class MainForm : Form
{
	private const string DefaultNotificationActionTitle = "默认提醒动作";
	private const string CustomNotificationActionTitle = "自定义动作";

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool DestroyIcon(IntPtr hIcon);

	private sealed class RoundedPanel : Panel
	{
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CornerRadius { get; set; } = 10;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color BorderColor { get; set; } = Color.FromArgb(226, 232, 240);

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
			Region = new Region(path);
			using Pen pen = new Pen(BorderColor);
			e.Graphics.DrawPath(pen, path);
		}

		private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
		{
			int diameter = Math.Max(1, radius * 2);
			GraphicsPath path = new GraphicsPath();
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
			path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
			path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
			path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
			path.CloseFigure();
			return path;
		}
	}

	private readonly Dictionary<string, Button> _categoryButtons = new Dictionary<string, Button>();
	private readonly System.Windows.Forms.Timer _audioTimer = new System.Windows.Forms.Timer { Interval = 80 };
	private readonly System.Windows.Forms.Timer _scheduleTimer = new System.Windows.Forms.Timer { Interval = 30000 };
	private readonly CancellationTokenSource _closing = new CancellationTokenSource();
	private readonly List<ExpressionItem> _expressions;
	private readonly AudioSignalDetector _audioDetector = new AudioSignalDetector();
	private readonly NotifyIcon _trayIcon = new NotifyIcon();
	private readonly bool _startInBackground = Environment.GetCommandLineArgs().Any(arg => arg.Equals("--background", StringComparison.OrdinalIgnoreCase));
	private readonly HashSet<string> _scheduleHits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	private NotificationSettings _settings = NotificationSettings.LoadOrCreate();
	private IElectronLowLevel? _bot;
	private bool _isPlaying;
	private bool _audioMonitorEnabled;
	private bool _audioScanBusy;
		private bool _scheduleScanBusy;
		private bool _exitRequested;
		private bool _syncingStartupBox;
		private DateTime _lastSoundNudge = DateTime.MinValue;
		private string _selectedCategory = "全部";

	public MainForm()
	{
		InitializeComponent();
		Text = "ElectronBot 通知声音监听 V3";
		ClientSize = new Size(1200, 800);
		MinimumSize = new Size(1100, 720);
		StartPosition = FormStartPosition.CenterScreen;
		Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular);
		BackColor = Color.FromArgb(247, 250, 252);
		_expressions = ExpressionCatalog.Load();
		Icon = LoadAppIcon();
		if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
		{
			return;
		}
		ConfigureRuntimeLayout();
		LoadExpressionButtons();
		InitializeTrayIcon();
		_audioTimer.Tick += async delegate { await ScanAudioSignalsAsync(); };
		_scheduleTimer.Tick += async delegate { await ScanScheduledActionsAsync(); };
		UpdateStatus(_settings.LoadWarning ?? "未连接。连接后，监听到配置的通知声音就会让机器人动一下。");
	}

		protected override async void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (_startInBackground)
			{
				HideToTray();
		}
		await ConnectBotAsync();
		StartAudioMonitor();
		StartScheduleMonitor();
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (!_exitRequested && e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			HideToTray();
			return;
		}
		_closing.Cancel();
		_audioTimer.Stop();
		_scheduleTimer.Stop();
		_audioDetector.Dispose();
		_trayIcon.Visible = false;
		_trayIcon.Dispose();
		DisconnectBot();
		base.OnFormClosing(e);
	}

	private void ConfigureRuntimeLayout()
	{
		_intervalBox.Minimum = 20;
		_intervalBox.Maximum = 1000;
		_intervalBox.Value = ClampToNumericRange(_settings.FrameIntervalMs, _intervalBox);
		_thresholdBox.Minimum = 1;
		_thresholdBox.Maximum = 100;
		_thresholdBox.Value = ClampToNumericRange(_settings.SoundThresholdPercent, _thresholdBox);
		_cooldownBox.Minimum = 0;
			_cooldownBox.Maximum = 60;
			_cooldownBox.Value = ClampToNumericRange(_settings.CooldownSeconds, _cooldownBox);
			_enableServoBox.Checked = _settings.EnableServo;
			_syncingStartupBox = true;
			_startupBox.Checked = IsStartupShortcutEnabled();
			_syncingStartupBox = false;
			_startupBox.CheckedChanged += delegate
			{
				if (_syncingStartupBox)
				{
					return;
				}
				bool enabled = _startupBox.Checked;
				bool success = SetStartupShortcutEnabled(enabled);
				_syncingStartupBox = true;
				_startupBox.Checked = IsStartupShortcutEnabled();
				_syncingStartupBox = false;
				UpdateStatus(success
					? (enabled ? "已启用开机启动。" : "已关闭开机启动。")
					: "开机启动设置失败，请查看日志。");
			};

		StylePrimaryButton(_disconnectButton);
		MakeRoundedButton(_disconnectButton, 8);
		_disconnectButton.Click += async delegate
		{
			if (_bot?.IsConnected ?? false)
			{
				DisconnectBot();
			}
			else
			{
				await ConnectBotAsync();
			}
		};

		StyleLightButton(_startListenButton);
		MakeRoundedButton(_startListenButton, 6);
		_startListenButton.Click += delegate { StartAudioMonitor(); };
		StyleDangerButton(_stopListenButton);
		MakeRoundedButton(_stopListenButton, 6);
		_stopListenButton.Click += delegate { StopAudioMonitor(); };
		StyleLightButton(_testNudgeButton);
		MakeRoundedButton(_testNudgeButton, 6);
		_testNudgeButton.Click += async delegate { await PlaySoundNudgeAsync(GetTestSignal()); };

		_categoryButtons.Clear();
		RegisterCategoryButton(_categoryAllButton, "全部");
		RegisterCategoryButton(_categoryActionButton, "动作");
		RegisterCategoryButton(_categoryEmotionButton, "情绪");
		RegisterCategoryButton(_categoryVoiceButton, "语音互动");
		RegisterCategoryButton(_categoryNotifyButton, "通知提醒");
		StyleLightButton(_addExpressionButton);
		_addExpressionButton.Click += delegate { OpenExpressionEditor(); };
		StyleLightButton(_scheduleSettingsButton);
		_scheduleSettingsButton.Click += delegate { OpenScheduleEditor(); };

		StyleLightButton(_clearCurrentButton);
		_clearCurrentButton.Click += delegate
		{
			_currentName.Text = "等待通知";
			_currentBadge.Text = "未触发";
			_currentKeywords.Text = "关键词：-";
			_currentPeak.Text = "峰值：-";
			_currentTime.Text = "最后触发：-";
		};
		_preview.SizeMode = PictureBoxSizeMode.Zoom;
		_currentIcon.SizeMode = PictureBoxSizeMode.Zoom;
		_statusIcon.SizeMode = PictureBoxSizeMode.Zoom;

		Button[] ruleButtons = { _captureSourceButton, _browseFaceButton, _deleteProfileButton, _reloadConfigButton, _openConfigButton, _saveConfigButton };
		string[] ruleButtonTexts = { "+ 新增规则", "编辑规则", "删除", "重载", "打开配置", "保存" };
		EventHandler[] ruleHandlers =
		{
			delegate { AddProfileRow(openEditor: true); },
			delegate { EditSelectedProfileRow(); },
			delegate { DeleteSelectedProfileRow(); },
			delegate { ReloadSettingsFromFile(); },
			delegate { OpenSettingsFile(); },
			delegate { SaveSettingsFromUi(); }
		};
		int[] ruleButtonWidths = { 82, 82, 58, 58, 82, 58 };
		int ruleButtonX = 175;
		for (int i = 0; i < ruleButtons.Length; i++)
		{
			StyleLightButton(ruleButtons[i]);
			ruleButtons[i].Text = ruleButtonTexts[i];
			ruleButtons[i].SetBounds(ruleButtonX, ruleButtons[i].Top, ruleButtonWidths[i], ruleButtons[i].Height);
			ruleButtonX += ruleButtonWidths[i] + 8;
			ruleButtons[i].Click += ruleHandlers[i];
		}

		BuildProfilesGrid();
		LoadProfilesToGrid();
		UpdateCategoryButtons();
		UpdateConnectionBadge();
		UpdateListenBadge();
	}

	private void RegisterCategoryButton(Button button, string category)
	{
		button.Click += delegate
		{
			_selectedCategory = category;
			RefreshExpressionButtons();
		};
		_categoryButtons[category] = button;
	}

	private static Icon LoadAppIcon()
	{
		try
		{
			string iconImagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Emoji", "normal.png");
			if (!File.Exists(iconImagePath))
			{
				return SystemIcons.Application;
			}
			using Image source = Image.FromFile(iconImagePath);
			using Bitmap bitmap = new Bitmap(32, 32);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Color.Transparent);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(source, new Rectangle(0, 0, 32, 32));
			}
			IntPtr handle = bitmap.GetHicon();
			try
			{
				return (Icon)Icon.FromHandle(handle).Clone();
			}
			finally
			{
				DestroyIcon(handle);
			}
		}
		catch
		{
			return SystemIcons.Application;
		}
	}

	private static void StylePrimaryButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.BackColor = Color.FromArgb(37, 99, 235);
		button.ForeColor = Color.White;
		button.FlatAppearance.BorderColor = Color.FromArgb(37, 99, 235);
		button.FlatAppearance.BorderSize = 0;
	}

	private static void StyleLightButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.BackColor = Color.White;
		button.ForeColor = Color.FromArgb(15, 23, 42);
		button.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
	}

	private static void StyleDangerButton(Button button)
	{
		button.FlatStyle = FlatStyle.Flat;
		button.BackColor = Color.FromArgb(255, 245, 245);
		button.ForeColor = Color.FromArgb(220, 38, 38);
		button.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
	}

	private static void MakeRoundedButton(Button button, int radius)
	{
		button.Resize += delegate { ApplyRoundedRegion(button, radius); };
		ApplyRoundedRegion(button, radius);
	}

	private static void ApplyRoundedRegion(Control control, int radius)
	{
		Rectangle bounds = new Rectangle(0, 0, control.Width, control.Height);
		using GraphicsPath path = new GraphicsPath();
		int diameter = Math.Max(1, radius * 2);
		path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
		path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
		path.AddArc(bounds.Right - diameter - 1, bounds.Bottom - diameter - 1, diameter, diameter, 0, 90);
		path.AddArc(bounds.Left, bounds.Bottom - diameter - 1, diameter, diameter, 90, 90);
		path.CloseFigure();
		control.Region = new Region(path);
	}

	private void BuildProfilesGrid()
	{
		_profilesGrid.AllowUserToAddRows = false;
		_profilesGrid.AllowUserToDeleteRows = false;
		_profilesGrid.RowHeadersVisible = false;
		_profilesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		_profilesGrid.MultiSelect = false;
		_profilesGrid.BackgroundColor = Color.White;
		_profilesGrid.BorderStyle = BorderStyle.None;
		_profilesGrid.GridColor = Color.FromArgb(226, 232, 240);
		_profilesGrid.EnableHeadersVisualStyles = false;
		_profilesGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
		_profilesGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
		_profilesGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8f);
		_profilesGrid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 8.5f);
		_profilesGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(239, 246, 255);
		_profilesGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
		_profilesGrid.RowTemplate.Height = 44;
		_profilesGrid.Columns.Clear();
		_profilesGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "启用", Width = 44 });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "应用名称", Width = 82 });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Keywords", HeaderText = "声音关键词", Width = 155 });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FacePath", HeaderText = "显示表情", Width = 135 });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActionTitle", HeaderText = "触发动作", Width = 92 });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActionKey", HeaderText = "ActionKey", Visible = false });
		_profilesGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActionPath", HeaderText = "ActionPath", Visible = false });
		_profilesGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "操作", Text = "编辑", UseColumnTextForButtonValue = true, Width = 64 });
		_profilesGrid.CellContentClick += delegate(object? sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && _profilesGrid.Columns[e.ColumnIndex].Name == "Action")
			{
				EditProfileRow(_profilesGrid.Rows[e.RowIndex]);
			}
		};
	}

	private void InitializeTrayIcon()
	{
		ContextMenuStrip menu = new ContextMenuStrip();
		menu.Items.Add("显示窗口", null, delegate { ShowFromTray(); });
		menu.Items.Add("连接机器人", null, async delegate { await ConnectBotAsync(); });
		menu.Items.Add("开始/停止监听", null, delegate { ToggleAudioMonitor(); });
		menu.Items.Add(new ToolStripSeparator());
		menu.Items.Add("退出", null, delegate { ExitApplication(); });
		_trayIcon.Icon = Icon;
		_trayIcon.Text = "ElectronBot 通知声音监听 V3";
		_trayIcon.Visible = true;
		_trayIcon.ContextMenuStrip = menu;
		_trayIcon.DoubleClick += delegate { ShowFromTray(); };
	}

	private void HideToTray()
	{
		ShowInTaskbar = false;
		Hide();
		AppendLog("window hidden to tray");
	}

	private void ShowFromTray()
	{
		ShowInTaskbar = true;
		Show();
		WindowState = FormWindowState.Normal;
		Activate();
	}

	private void ExitApplication()
	{
		_exitRequested = true;
		Close();
	}

		private static string StartupShortcutPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ElectronBotExpressionController.lnk");

		private static bool IsStartupShortcutEnabled()
		{
			string shortcutPath = StartupShortcutPath;
			if (!File.Exists(shortcutPath))
			{
				return false;
			}
			try
			{
				Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
				if (shellType == null)
				{
					return true;
				}
				dynamic shell = Activator.CreateInstance(shellType)!;
				dynamic shortcut = shell.CreateShortcut(shortcutPath);
				string targetPath = Convert.ToString(shortcut.TargetPath) ?? "";
				return !string.IsNullOrWhiteSpace(targetPath)
					&& string.Equals(Path.GetFullPath(targetPath), Path.GetFullPath(Application.ExecutablePath), StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return true;
			}
		}

		private static bool SetStartupShortcutEnabled(bool enabled)
		{
			try
			{
				string shortcutPath = StartupShortcutPath;
				if (!enabled)
				{
					if (File.Exists(shortcutPath))
					{
						File.Delete(shortcutPath);
					}
					AppendLog("startup shortcut removed: " + shortcutPath);
					return true;
				}
				Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
				if (shellType == null)
				{
					AppendLog("startup shortcut skipped: WScript.Shell unavailable");
					return false;
				}
				dynamic shell = Activator.CreateInstance(shellType)!;
				dynamic shortcut = shell.CreateShortcut(shortcutPath);
			shortcut.TargetPath = Application.ExecutablePath;
			shortcut.Arguments = "--background";
			shortcut.WorkingDirectory = AppContext.BaseDirectory;
			shortcut.Description = "ElectronBot notification sound listener V3";
				shortcut.IconLocation = Application.ExecutablePath;
				shortcut.Save();
				AppendLog("startup shortcut ensured: " + shortcutPath);
				return true;
			}
			catch (Exception ex)
			{
				AppendLog($"startup shortcut failed: {ex}");
				return false;
			}
		}

	private void OpenSettingsFile()
	{
		try
		{
			SaveSettingsFromUi();
			Process.Start(new ProcessStartInfo("notepad.exe", Quote(NotificationSettings.SettingsPath))
			{
				UseShellExecute = true
			});
			UpdateStatus("已打开配置文件：" + NotificationSettings.SettingsPath);
		}
		catch (Exception ex)
		{
			UpdateStatus("打开配置失败：" + ex.Message);
			AppendLog($"open settings failed: {ex}");
		}
	}

	private void ReloadSettingsFromFile()
	{
		_settings = NotificationSettings.LoadOrCreate();
		LoadProfilesToGrid();
		_intervalBox.Value = ClampToNumericRange(_settings.FrameIntervalMs, _intervalBox);
		_thresholdBox.Value = ClampToNumericRange(_settings.SoundThresholdPercent, _thresholdBox);
		_cooldownBox.Value = ClampToNumericRange(_settings.CooldownSeconds, _cooldownBox);
		_enableServoBox.Checked = _settings.EnableServo;
		_audioDetector.ResetBaseline(TimeSpan.FromMilliseconds(500));
		_scheduleHits.Clear();
		UpdateStatus(_settings.LoadWarning ?? "配置已重载。");
	}

	private void SaveSettingsFromUi()
	{
		try
		{
			CaptureUiSettings();
			_settings.Save();
			LoadProfilesToGrid();
			UpdateStatus("配置已保存：" + NotificationSettings.SettingsPath);
		}
		catch (Exception ex)
		{
			UpdateStatus("保存配置失败：" + ex.Message);
			AppendLog($"save settings failed: {ex}");
		}
	}

	private void LoadProfilesToGrid()
	{
		_profilesGrid.Rows.Clear();
		foreach (NotificationProfile profile in _settings.Profiles)
		{
			int rowIndex = _profilesGrid.Rows.Add(profile.Enabled, profile.Name, string.Join(";", profile.Keywords), profile.FacePath, ResolveProfileActionTitle(profile), profile.ActionKey, profile.ActionPath, "编辑");
			_profilesGrid.Rows[rowIndex].Tag = profile;
		}
	}

	private void CaptureUiSettings()
	{
		_profilesGrid.EndEdit();
		_settings.FrameIntervalMs = (int)_intervalBox.Value;
		_settings.SoundThresholdPercent = (int)_thresholdBox.Value;
		_settings.CooldownSeconds = (int)_cooldownBox.Value;
		_settings.EnableServo = _enableServoBox.Checked;
			_settings.Profiles = ReadProfilesFromGrid();
			_settings.SyncAudioTargetsFromProfiles();
	}

	private List<NotificationProfile> ReadProfilesFromGrid()
	{
		List<NotificationProfile> profiles = new List<NotificationProfile>();
		foreach (DataGridViewRow row in _profilesGrid.Rows)
		{
			bool enabled = Convert.ToBoolean(row.Cells["Enabled"].Value ?? true);
			string name = Convert.ToString(row.Cells["Name"].Value)?.Trim() ?? "";
			string keywordsText = Convert.ToString(row.Cells["Keywords"].Value)?.Trim() ?? "";
			string facePath = Convert.ToString(row.Cells["FacePath"].Value)?.Trim() ?? "";
			string actionTitle = Convert.ToString(row.Cells["ActionTitle"].Value)?.Trim() ?? "";
			string actionKey = Convert.ToString(row.Cells["ActionKey"].Value)?.Trim() ?? "";
			string actionPath = Convert.ToString(row.Cells["ActionPath"].Value)?.Trim() ?? "";
			List<string> keywords = SplitKeywords(keywordsText);
			ExpressionItem? action = ResolveProfileAction(actionTitle, actionKey, actionPath);
			NotificationProfile? existing = row.Tag as NotificationProfile;
			if (string.IsNullOrWhiteSpace(name) && keywords.Count == 0 && string.IsNullOrWhiteSpace(facePath) && action == null)
			{
				continue;
			}
			NotificationProfile profile = new NotificationProfile
			{
				Enabled = enabled,
				Name = string.IsNullOrWhiteSpace(name) ? "未命名监听" : name,
				Keywords = keywords,
				FacePath = facePath,
				ActionKey = action?.Key ?? "",
				ActionPath = action != null ? MakeRelativeIfUnderBaseDirectory(action.ActionPath) : ""
			};
			if (action == null && string.Equals(actionTitle, CustomNotificationActionTitle, StringComparison.OrdinalIgnoreCase) && existing?.Frames?.Count > 0)
			{
				profile.Frames = CloneActionFrames(existing.Frames);
			}
			profiles.Add(profile);
		}
		return profiles;
	}

	private void AddProfileRow(bool openEditor = false)
	{
		int rowIndex = _profilesGrid.Rows.Add(true, "新监听", "", "Assets/Emoji/excited.png", DefaultNotificationActionTitle, "", "", "编辑");
		_profilesGrid.ClearSelection();
		_profilesGrid.Rows[rowIndex].Selected = true;
		_profilesGrid.CurrentCell = _profilesGrid.Rows[rowIndex].Cells["Name"];
		_profilesGrid.Rows[rowIndex].Tag = new NotificationProfile
		{
			Name = "新监听",
			FacePath = "Assets/Emoji/excited.png"
		};
		UpdateStatus("已新增监听项。编辑后可填写声音关键词、显示表情和触发动作。");
		if (openEditor)
		{
			EditProfileRow(_profilesGrid.Rows[rowIndex]);
		}
	}

	private void DeleteSelectedProfileRow()
	{
		if (_profilesGrid.CurrentRow == null)
		{
			return;
		}
		_profilesGrid.Rows.Remove(_profilesGrid.CurrentRow);
		UpdateStatus("已删除选中的监听项，记得保存配置。");
	}

	private void EditSelectedProfileRow()
	{
		EditProfileRow(EnsureSelectedProfileRow());
	}

	private void EditProfileRow(DataGridViewRow row)
	{
		using Form dialog = new Form
		{
			Text = "编辑通知规则",
			ClientSize = new Size(560, 430),
			StartPosition = FormStartPosition.CenterParent,
			Font = Font,
			BackColor = Color.FromArgb(247, 250, 252),
			Icon = Icon
		};
		Label nameLabel = new Label { Text = "应用名称", Bounds = new Rectangle(22, 22, 90, 24) };
		TextBox nameBox = new TextBox { Bounds = new Rectangle(112, 20, 230, 26), Text = Convert.ToString(row.Cells["Name"].Value)?.Trim() ?? "" };
		CheckBox enabledBox = new CheckBox { Text = "启用", Bounds = new Rectangle(370, 22, 80, 24), Checked = Convert.ToBoolean(row.Cells["Enabled"].Value ?? true) };
		Label keywordsLabel = new Label { Text = "声音关键词", Bounds = new Rectangle(22, 62, 90, 24) };
		TextBox keywordsBox = new TextBox
		{
			Bounds = new Rectangle(112, 60, 390, 76),
			Multiline = true,
			ScrollBars = ScrollBars.Vertical,
			Text = Convert.ToString(row.Cells["Keywords"].Value)?.Trim() ?? ""
		};
		Button captureButton = new Button { Text = "捕获当前声音", Bounds = new Rectangle(390, 142, 112, 30) };
		Label faceLabel = new Label { Text = "显示表情", Bounds = new Rectangle(22, 188, 90, 24) };
		TextBox faceBox = new TextBox { Bounds = new Rectangle(112, 186, 330, 26), Text = Convert.ToString(row.Cells["FacePath"].Value)?.Trim() ?? "" };
		Button faceButton = new Button { Text = "选择图片", Bounds = new Rectangle(452, 185, 88, 28) };
		PictureBox facePreview = new PictureBox
		{
			Bounds = new Rectangle(112, 222, 160, 88),
			BackColor = Color.Black,
			SizeMode = PictureBoxSizeMode.Zoom
		};
		Label actionLabel = new Label { Text = "触发动作", Bounds = new Rectangle(22, 330, 90, 24) };
		ComboBox actionBox = new ComboBox { Bounds = new Rectangle(112, 328, 230, 28), DropDownStyle = ComboBoxStyle.DropDownList };
		actionBox.Items.Add(DefaultNotificationActionTitle);
		foreach (string title in _expressions.Select(item => item.Title).Distinct())
		{
			actionBox.Items.Add(title);
		}
		string currentActionTitle = Convert.ToString(row.Cells["ActionTitle"].Value)?.Trim() ?? DefaultNotificationActionTitle;
		if (!ContainsComboItem(actionBox, currentActionTitle))
		{
			actionBox.Items.Add(currentActionTitle);
		}
		actionBox.SelectedItem = string.IsNullOrWhiteSpace(currentActionTitle) ? DefaultNotificationActionTitle : currentActionTitle;
		Button saveButton = new Button { Text = "保存", Bounds = new Rectangle(356, 378, 82, 32) };
		Button cancelButton = new Button { Text = "取消", Bounds = new Rectangle(450, 378, 82, 32) };
		StylePrimaryButton(saveButton);
		StyleLightButton(cancelButton);
		StyleLightButton(captureButton);
		StyleLightButton(faceButton);

		void RefreshFacePreview()
		{
			facePreview.Image?.Dispose();
			facePreview.Image = null;
			string previewPath = ResolveConfiguredPath(faceBox.Text.Trim());
			if (File.Exists(previewPath))
			{
					facePreview.Image = LoadImageSnapshot(previewPath);
			}
		}

		captureButton.Click += delegate
		{
			AudioSessionInfo? best = CaptureBestAudioSession();
			if (best == null)
			{
				MessageBox.Show(dialog, "暂时没捕获到明显发声的程序。请让目标软件响一声再点。", "捕获声音", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			AddKeywordToTextBox(keywordsBox, best.Name);
			if (string.IsNullOrWhiteSpace(nameBox.Text))
			{
				nameBox.Text = best.Name;
			}
			AppendLog("captured audio source: " + best.Name);
		};
		faceButton.Click += delegate
		{
			using OpenFileDialog fileDialog = new OpenFileDialog
			{
				Title = "选择机器人显示图标",
				Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*",
				InitialDirectory = Path.Combine(AppContext.BaseDirectory, "Assets")
			};
			if (fileDialog.ShowDialog(dialog) == DialogResult.OK)
			{
				faceBox.Text = MakeRelativeIfUnderBaseDirectory(fileDialog.FileName);
				RefreshFacePreview();
			}
		};
		faceBox.Leave += delegate { RefreshFacePreview(); };
		saveButton.Click += delegate
		{
			List<string> keywords = SplitKeywords(keywordsBox.Text);
			if (keywords.Count == 0)
			{
				MessageBox.Show(dialog, "至少填写一个声音关键词，或先让目标软件发声后点“捕获当前声音”。", "通知规则", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			string selectedActionTitle = Convert.ToString(actionBox.SelectedItem)?.Trim() ?? DefaultNotificationActionTitle;
			ExpressionItem? selectedAction = ResolveProfileAction(selectedActionTitle, "", "");
			NotificationProfile? existingProfile = row.Tag as NotificationProfile;
			bool keepCustomFrames = selectedAction == null
				&& string.Equals(selectedActionTitle, CustomNotificationActionTitle, StringComparison.OrdinalIgnoreCase)
				&& existingProfile?.Frames?.Count > 0;
			string facePath = MakeRelativeIfUnderBaseDirectory(faceBox.Text.Trim());
			row.Cells["Enabled"].Value = enabledBox.Checked;
			row.Cells["Name"].Value = string.IsNullOrWhiteSpace(nameBox.Text) ? "未命名监听" : nameBox.Text.Trim();
			row.Cells["Keywords"].Value = string.Join(";", keywords);
			row.Cells["FacePath"].Value = facePath;
			row.Cells["ActionTitle"].Value = selectedAction?.Title ?? (keepCustomFrames ? CustomNotificationActionTitle : DefaultNotificationActionTitle);
			row.Cells["ActionKey"].Value = selectedAction?.Key ?? "";
			row.Cells["ActionPath"].Value = selectedAction != null ? MakeRelativeIfUnderBaseDirectory(selectedAction.ActionPath) : "";
			NotificationProfile updatedProfile = new NotificationProfile
			{
				Enabled = enabledBox.Checked,
				Name = Convert.ToString(row.Cells["Name"].Value) ?? "",
				Keywords = keywords,
				FacePath = facePath,
				ActionKey = selectedAction?.Key ?? "",
				ActionPath = selectedAction != null ? MakeRelativeIfUnderBaseDirectory(selectedAction.ActionPath) : ""
			};
			if (keepCustomFrames && existingProfile != null)
			{
				updatedProfile.Frames = CloneActionFrames(existingProfile.Frames);
			}
			row.Tag = updatedProfile;
			UpdateStatus("通知规则已更新，记得点“保存”写入配置。");
			dialog.DialogResult = DialogResult.OK;
			dialog.Close();
		};
		cancelButton.Click += delegate { dialog.Close(); };
		dialog.Controls.AddRange(new Control[] { nameLabel, nameBox, enabledBox, keywordsLabel, keywordsBox, captureButton, faceLabel, faceBox, faceButton, facePreview, actionLabel, actionBox, saveButton, cancelButton });
		RefreshFacePreview();
		dialog.ShowDialog(this);
	}

	private AudioSessionInfo? CaptureBestAudioSession()
	{
		float threshold = Math.Max(0.003f, (float)_thresholdBox.Value / 200f);
		return _audioDetector.CaptureActiveSessions(threshold)
			.Where(session => !IsIgnoredAudioSession(session))
			.OrderByDescending(session => session.Peak)
			.FirstOrDefault();
	}

	private DataGridViewRow EnsureSelectedProfileRow()
	{
		if (_profilesGrid.CurrentRow != null)
		{
			return _profilesGrid.CurrentRow;
		}
		AddProfileRow();
		return _profilesGrid.CurrentRow!;
	}

	private static bool IsIgnoredAudioSession(AudioSessionInfo session)
	{
		return session.Name.Contains("ElectronBot", StringComparison.OrdinalIgnoreCase)
			|| session.Name.Contains("System Sounds", StringComparison.OrdinalIgnoreCase);
	}

	private static void AddKeywordToTextBox(TextBox box, string keyword)
	{
		List<string> keywords = SplitKeywords(box.Text);
		if (!keywords.Any(item => item.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
		{
			keywords.Add(keyword);
		}
		box.Text = string.Join(";", keywords);
	}

	private static string MakeRelativeIfUnderBaseDirectory(string path)
	{
		string baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (path.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
		{
			return path[baseDir.Length..].Replace(Path.DirectorySeparatorChar, '/');
		}
		return path;
	}

	private string ResolveProfileActionTitle(NotificationProfile profile)
	{
		ExpressionItem? action = ResolveProfileAction("", profile.ActionKey, profile.ActionPath);
		if (action != null)
		{
			return action.Title;
		}
		return profile.Frames?.Count > 0 ? CustomNotificationActionTitle : DefaultNotificationActionTitle;
	}

	private ExpressionItem? ResolveProfileAction(string actionTitle, string actionKey, string actionPath)
	{
		if (string.Equals(actionTitle, DefaultNotificationActionTitle, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(actionTitle, CustomNotificationActionTitle, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		if (!string.IsNullOrWhiteSpace(actionTitle))
		{
			ExpressionItem? byTitle = _expressions.FirstOrDefault(item => item.Title.Equals(actionTitle, StringComparison.OrdinalIgnoreCase));
			if (byTitle != null)
			{
				return byTitle;
			}
		}
		if (!string.IsNullOrWhiteSpace(actionKey))
		{
			ExpressionItem? byKey = _expressions.FirstOrDefault(item => item.Key.Equals(actionKey, StringComparison.OrdinalIgnoreCase));
			if (byKey != null)
			{
				return byKey;
			}
		}
		if (!string.IsNullOrWhiteSpace(actionPath))
		{
			return _expressions.FirstOrDefault(item => PathsEqual(item.ActionPath, actionPath));
		}
		return null;
	}

	private static bool ContainsComboItem(ComboBox comboBox, string value)
	{
		foreach (object? item in comboBox.Items)
		{
			if (string.Equals(Convert.ToString(item), value, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static bool PathsEqual(string left, string right)
	{
		if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
		{
			return false;
		}
		return string.Equals(Path.GetFullPath(ResolveConfiguredPath(left)), Path.GetFullPath(ResolveConfiguredPath(right)), StringComparison.OrdinalIgnoreCase);
	}

		private static string ResolveConfiguredPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return "";
			}
			return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
		}

		private static Image LoadImageSnapshot(string path)
		{
			using Image source = Image.FromFile(path);
			return new Bitmap(source);
		}

	private static List<ExpressionAction> CloneActionFrames(IEnumerable<ExpressionAction> frames)
	{
		return frames.Where(frame => frame != null)
			.Select(frame => new ExpressionAction
			{
				J1 = frame.J1,
				J2 = frame.J2,
				J3 = frame.J3,
				J4 = frame.J4,
				J5 = frame.J5,
				J6 = frame.J6
			})
			.ToList();
	}

	private static List<string> SplitKeywords(string text)
	{
		return text.Split(new[] { ';', '；', ',', '，', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	private string GetTestSignal()
	{
		CaptureUiSettings();
		return _settings.Profiles.Where(profile => profile.Enabled)
			.SelectMany(profile => profile.Keywords.DefaultIfEmpty(profile.Name))
			.FirstOrDefault(text => !string.IsNullOrWhiteSpace(text)) ?? "manual test";
	}

	private static decimal ClampToNumericRange(int value, NumericUpDown box)
	{
		return Math.Min(box.Maximum, Math.Max(box.Minimum, value));
	}

	private static string Quote(string path)
	{
		return "\"" + path + "\"";
	}

	private void LoadExpressionButtons()
	{
		RefreshExpressionButtons();
		ExpressionItem? first = _expressions.FirstOrDefault();
		if (first != null)
		{
			SetPreviewImage(first.ImagePath);
		}
	}

	private void RefreshExpressionButtons()
	{
		_expressionPanel.Controls.Clear();
		IEnumerable<ExpressionItem> items = _selectedCategory == "全部"
			? _expressions
			: _expressions.Where(item => item.Category.Equals(_selectedCategory, StringComparison.OrdinalIgnoreCase));
		foreach (ExpressionItem item in items)
		{
			ExpressionButton button = new ExpressionButton(item);
			button.Click += async delegate { await PlayExpressionAsync(item); };
			_expressionPanel.Controls.Add(button);
		}
		UpdateCategoryButtons();
	}

	private void UpdateCategoryButtons()
	{
		foreach (KeyValuePair<string, Button> pair in _categoryButtons)
		{
			bool selected = pair.Key == _selectedCategory;
			pair.Value.BackColor = selected ? Color.FromArgb(239, 246, 255) : Color.White;
			pair.Value.ForeColor = selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(15, 23, 42);
		}
	}

	private void OpenExpressionEditor()
	{
		using Form dialog = new Form
		{
			Text = "添加表情",
			ClientSize = new Size(560, 360),
			StartPosition = FormStartPosition.CenterParent,
			Font = Font,
			BackColor = Color.FromArgb(247, 250, 252),
			Icon = Icon
		};
		Label titleLabel = new Label { Text = "表情名称", Bounds = new Rectangle(22, 24, 90, 24) };
		TextBox titleBox = new TextBox { Bounds = new Rectangle(112, 22, 240, 26) };
		Label categoryLabel = new Label { Text = "分类", Bounds = new Rectangle(22, 64, 90, 24) };
		ComboBox categoryBox = new ComboBox { Bounds = new Rectangle(112, 62, 180, 28), DropDownStyle = ComboBoxStyle.DropDownList };
		foreach (string category in _expressions.Select(item => item.Category).Where(category => !string.IsNullOrWhiteSpace(category)).Distinct())
		{
			categoryBox.Items.Add(category);
		}
		string defaultCategory = _selectedCategory == "全部" ? "动作" : _selectedCategory;
		if (!ContainsComboItem(categoryBox, defaultCategory))
		{
			categoryBox.Items.Add(defaultCategory);
		}
		categoryBox.SelectedItem = defaultCategory;
		Label imageLabel = new Label { Text = "表情图片", Bounds = new Rectangle(22, 106, 90, 24) };
		TextBox imageBox = new TextBox { Bounds = new Rectangle(112, 104, 330, 26) };
		Button imageButton = new Button { Text = "选择图片", Bounds = new Rectangle(452, 103, 88, 28) };
		PictureBox imagePreview = new PictureBox
		{
			Bounds = new Rectangle(112, 144, 160, 88),
			BackColor = Color.Black,
			SizeMode = PictureBoxSizeMode.Zoom
		};
		Label actionLabel = new Label { Text = "动作文件", Bounds = new Rectangle(22, 252, 90, 24) };
		TextBox actionBox = new TextBox { Bounds = new Rectangle(112, 250, 330, 26), Text = Path.Combine(AppContext.BaseDirectory, "Assets", "Emoji", "normal.json") };
		Button actionButton = new Button { Text = "选择动作", Bounds = new Rectangle(452, 249, 88, 28) };
		Button saveButton = new Button { Text = "保存", Bounds = new Rectangle(356, 310, 82, 32) };
		Button cancelButton = new Button { Text = "取消", Bounds = new Rectangle(450, 310, 82, 32) };
		StyleLightButton(imageButton);
		StyleLightButton(actionButton);
		StylePrimaryButton(saveButton);
		StyleLightButton(cancelButton);

		void RefreshPreview()
		{
			imagePreview.Image?.Dispose();
			imagePreview.Image = null;
			string previewPath = ResolveConfiguredPath(imageBox.Text.Trim());
			if (File.Exists(previewPath))
			{
					imagePreview.Image = LoadImageSnapshot(previewPath);
			}
		}

		imageButton.Click += delegate
		{
			using OpenFileDialog fileDialog = new OpenFileDialog
			{
				Title = "选择表情图片",
				Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*",
				InitialDirectory = Path.Combine(AppContext.BaseDirectory, "Assets")
			};
			if (fileDialog.ShowDialog(dialog) == DialogResult.OK)
			{
				imageBox.Text = fileDialog.FileName;
				RefreshPreview();
			}
		};
		actionButton.Click += delegate
		{
			using OpenFileDialog fileDialog = new OpenFileDialog
			{
				Title = "选择动作 JSON",
				Filter = "动作文件|*.json|所有文件|*.*",
				InitialDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Emoji")
			};
			if (fileDialog.ShowDialog(dialog) == DialogResult.OK)
			{
				actionBox.Text = fileDialog.FileName;
			}
		};
		imageBox.Leave += delegate { RefreshPreview(); };
		saveButton.Click += delegate
		{
			string title = titleBox.Text.Trim();
			string category = Convert.ToString(categoryBox.SelectedItem)?.Trim() ?? "动作";
			string imagePath = ResolveConfiguredPath(imageBox.Text.Trim());
			string actionPath = ResolveConfiguredPath(actionBox.Text.Trim());
			if (string.IsNullOrWhiteSpace(title))
			{
				MessageBox.Show(dialog, "请填写表情名称。", "添加表情", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (!File.Exists(imagePath))
			{
				MessageBox.Show(dialog, "请选择存在的表情图片。", "添加表情", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			if (!File.Exists(actionPath))
			{
				MessageBox.Show(dialog, "请选择存在的动作 JSON 文件。", "添加表情", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}
			try
			{
					using Image validationImage = LoadImageSnapshot(imagePath);
				ExpressionAction.Load(actionPath);
				ExpressionItem item = ExpressionCatalog.AddCustom(title, category, imagePath, actionPath);
				_expressions.Add(item);
				_selectedCategory = item.Category;
				RefreshExpressionButtons();
				SetPreviewImage(item.ImagePath);
				UpdateStatus("已添加表情：" + item.Title);
				dialog.DialogResult = DialogResult.OK;
				dialog.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(dialog, "添加失败：" + ex.Message, "添加表情", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		};
		cancelButton.Click += delegate { dialog.Close(); };
		dialog.Controls.AddRange(new Control[] { titleLabel, titleBox, categoryLabel, categoryBox, imageLabel, imageBox, imageButton, imagePreview, actionLabel, actionBox, actionButton, saveButton, cancelButton });
		dialog.ShowDialog(this);
	}

	private void OpenScheduleEditor()
	{
		CaptureUiSettings();
		using Form dialog = new Form
		{
			Text = "定时表情设置",
			ClientSize = new Size(700, 460),
			StartPosition = FormStartPosition.CenterParent,
			Font = Font,
			BackColor = Color.FromArgb(247, 250, 252),
			Icon = Icon
		};
		DataGridView grid = new DataGridView
		{
			Bounds = new Rectangle(16, 16, 668, 360),
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			RowHeadersVisible = false,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			MultiSelect = false,
			BackgroundColor = Color.White,
			BorderStyle = BorderStyle.FixedSingle,
			RowTemplate = { Height = 34 }
		};
		grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Enabled", HeaderText = "启用", Width = 56 });
		grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "时间(HH:mm)", Width = 105 });
		grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "名称", Width = 140 });
		DataGridViewComboBoxColumn expressionColumn = new DataGridViewComboBoxColumn
		{
			Name = "Expression",
			HeaderText = "表情",
			Width = 180,
			FlatStyle = FlatStyle.Flat
		};
		expressionColumn.Items.AddRange(_expressions.Select(item => item.Title).Distinct().Cast<object>().ToArray());
		grid.Columns.Add(expressionColumn);
		grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActionKey", HeaderText = "Key", Width = 120, Visible = false });
		grid.DataError += delegate(object? sender, DataGridViewDataErrorEventArgs e)
		{
			e.ThrowException = false;
		};
		dialog.Controls.Add(grid);

		foreach (ScheduledAction action in _settings.ScheduledActions)
		{
			ExpressionItem? expression = ResolveScheduledExpression(action);
			string title = expression?.Title ?? _expressions.FirstOrDefault()?.Title ?? "";
			grid.Rows.Add(action.Enabled, action.Time, action.Name, title, expression?.Key ?? action.ActionKey);
		}

		Button addButton = new Button { Text = "+ 新增", Bounds = new Rectangle(324, 394, 78, 34) };
		Button deleteButton = new Button { Text = "删除", Bounds = new Rectangle(412, 394, 78, 34) };
		Button saveButton = new Button { Text = "保存", Bounds = new Rectangle(500, 394, 78, 34) };
		Button closeButton = new Button { Text = "关闭", Bounds = new Rectangle(588, 394, 78, 34) };
		StyleLightButton(addButton);
		StyleLightButton(deleteButton);
		StylePrimaryButton(saveButton);
		StyleLightButton(closeButton);
		addButton.Click += delegate
		{
			ExpressionItem fallback = _expressions.FirstOrDefault()!;
			grid.Rows.Add(true, DateTime.Now.AddMinutes(1).ToString("HH:mm"), fallback.Title, fallback.Title, fallback.Key);
		};
		deleteButton.Click += delegate
		{
			if (grid.CurrentRow != null)
			{
				grid.Rows.Remove(grid.CurrentRow);
			}
		};
		saveButton.Click += delegate
		{
			grid.EndEdit();
			List<ScheduledAction> actions = new List<ScheduledAction>();
			foreach (DataGridViewRow row in grid.Rows)
			{
				string timeText = Convert.ToString(row.Cells["Time"].Value)?.Trim() ?? "";
				if (!TimeSpan.TryParse(timeText, out TimeSpan time))
				{
					continue;
				}
				string expressionTitle = Convert.ToString(row.Cells["Expression"].Value)?.Trim() ?? "";
				ExpressionItem? expression = _expressions.FirstOrDefault(item => item.Title.Equals(expressionTitle, StringComparison.OrdinalIgnoreCase));
				if (expression == null)
				{
					continue;
				}
				string name = Convert.ToString(row.Cells["Name"].Value)?.Trim() ?? "";
				actions.Add(new ScheduledAction
				{
					Enabled = Convert.ToBoolean(row.Cells["Enabled"].Value ?? true),
					Time = $"{time.Hours:00}:{time.Minutes:00}",
					Name = string.IsNullOrWhiteSpace(name) ? expression.Title : name,
					ActionKey = expression.Key,
					FacePath = MakeRelativeIfUnderBaseDirectory(expression.ImagePath),
					ActionPath = MakeRelativeIfUnderBaseDirectory(expression.ActionPath)
				});
			}
			_settings.ScheduledActions = actions;
			_settings.Save();
			_scheduleHits.Clear();
			UpdateStatus("定时表情已保存。");
			dialog.DialogResult = DialogResult.OK;
			dialog.Close();
		};
		closeButton.Click += delegate { dialog.Close(); };
		dialog.Controls.Add(addButton);
		dialog.Controls.Add(deleteButton);
		dialog.Controls.Add(saveButton);
		dialog.Controls.Add(closeButton);
		dialog.ShowDialog(this);
	}

	private ExpressionItem? ResolveScheduledExpression(ScheduledAction action)
	{
		ExpressionItem? expression = null;
		if (!string.IsNullOrWhiteSpace(action.ActionKey))
		{
			expression = _expressions.FirstOrDefault(item => item.Key.Equals(action.ActionKey, StringComparison.OrdinalIgnoreCase));
		}
		if (expression != null)
		{
			return expression;
		}
		if (!string.IsNullOrWhiteSpace(action.Name))
		{
			expression = _expressions.FirstOrDefault(item => item.Title.Equals(action.Name, StringComparison.OrdinalIgnoreCase));
		}
		return expression ?? _expressions.FirstOrDefault();
	}

	private async Task ConnectBotAsync()
	{
		if (_bot?.IsConnected ?? false)
		{
			UpdateConnectionBadge();
			return;
		}
		_disconnectButton.Enabled = false;
		UpdateStatus("正在连接，先尝试 WinUSB 新固件，再尝试 LibUSB 旧固件...");
		await Task.Run(delegate
		{
			IElectronLowLevel[] drivers =
			{
				new WinUsbElectronLowLevel(NullLogger<WinUsbElectronLowLevel>.Instance),
				new LibUsbElectronLowLevel(NullLogger<LibUsbElectronLowLevel>.Instance)
			};
			foreach (IElectronLowLevel driver in drivers)
			{
				try
				{
					if (driver.Connect())
					{
						_bot = driver;
						break;
					}
					driver.Disconnect();
				}
				catch
				{
					try
					{
						driver.Disconnect();
					}
					catch
					{
					}
				}
			}
		});
		UpdateConnectionBadge();
		string botName = _bot?.GetType().Name ?? "ElectronBot";
		UpdateStatus((_bot?.IsConnected ?? false) ? "已连接：" + botName + "。点“测试提醒”可验证动作。" : "没有连接成功。请确认机器人已插入、驱动/固件匹配。");
	}

	private void DisconnectBot()
	{
		try
		{
			_bot?.Disconnect();
		}
		catch
		{
		}
		finally
		{
			_bot = null;
			UpdateConnectionBadge();
			UpdateStatus("已断开。");
		}
	}

	private void UpdateConnectionBadge()
	{
		bool connected = _bot?.IsConnected ?? false;
		_connectionState.Text = connected ? "● 已连接" : "● 未连接";
		_connectionState.ForeColor = connected ? Color.FromArgb(22, 163, 74) : Color.FromArgb(239, 68, 68);
		_robotState.Text = "机器人状态： " + (connected ? "已连接" : "未连接");
		_disconnectButton.Enabled = true;
		_disconnectButton.Text = connected ? "断开连接" : "连接机器人";
	}

	private void ToggleAudioMonitor()
	{
		if (_audioMonitorEnabled)
		{
			StopAudioMonitor();
		}
		else
		{
			StartAudioMonitor();
		}
	}

	private void StartAudioMonitor()
	{
		if (_audioMonitorEnabled)
		{
			return;
		}
		CaptureUiSettings();
		_audioDetector.ResetBaseline();
		_audioMonitorEnabled = true;
		_audioTimer.Start();
		UpdateListenBadge();
		UpdateStatus("声音监听已开启：检测 " + string.Join(";", _settings.AudioTargets) + " 的音频峰值。");
		AppendLog("audio monitor started");
	}

	private void StopAudioMonitor()
	{
		if (!_audioMonitorEnabled)
		{
			return;
		}
		_audioTimer.Stop();
		_audioMonitorEnabled = false;
		UpdateListenBadge();
		UpdateStatus("已停止声音监听。");
		AppendLog("audio monitor stopped");
	}

	private void UpdateListenBadge()
	{
		_listenState.Text = _audioMonitorEnabled ? "↔ 监听中" : "● 已停止";
		_listenState.ForeColor = _audioMonitorEnabled ? Color.FromArgb(22, 163, 74) : Color.FromArgb(100, 116, 139);
		_listenStateSmall.Text = "监听状态： " + (_audioMonitorEnabled ? "监听中" : "已停止");
		_startListenButton.Enabled = !_audioMonitorEnabled;
		_stopListenButton.Enabled = _audioMonitorEnabled;
	}

	private void StartScheduleMonitor()
	{
		if (!_scheduleTimer.Enabled)
		{
			_scheduleTimer.Start();
			AppendLog("schedule monitor started");
		}
		_ = ScanScheduledActionsAsync();
	}

	private async Task ScanScheduledActionsAsync()
	{
		if (_scheduleScanBusy)
		{
			return;
		}
		_scheduleScanBusy = true;
		try
		{
			DateTime now = DateTime.Now;
			foreach (ScheduledAction action in _settings.ScheduledActions.Where(action => action.IsDue(now)))
			{
				string hitKey = action.HitKey(now);
				if (!_scheduleHits.Add(hitKey))
				{
					continue;
				}
				ExpressionItem? expression = action.ResolveExpression(_expressions);
				if (expression == null || _isPlaying)
				{
					continue;
				}
				UpdateStatus("定时动作：" + action.Time + " " + action.Name);
				AppendLog("scheduled action due: " + action.Time + " " + action.Name);
				await PlayExpressionAsync(expression);
			}
			RemoveOldScheduleHits(now);
		}
		catch (Exception ex)
		{
			AppendLog($"schedule scan failed: {ex}");
		}
		finally
		{
			_scheduleScanBusy = false;
		}
	}

	private void RemoveOldScheduleHits(DateTime now)
	{
		string todayPrefix = now.ToString("yyyy-MM-dd") + "|";
		foreach (string key in _scheduleHits.Where(key => !key.StartsWith(todayPrefix, StringComparison.OrdinalIgnoreCase)).ToList())
		{
			_scheduleHits.Remove(key);
		}
	}

	private async Task ScanAudioSignalsAsync()
	{
		if (_audioScanBusy)
		{
			return;
		}
		_audioScanBusy = true;
		try
		{
			CaptureUiSettings();
			float threshold = (float)_thresholdBox.Value / 100f;
			List<string> signals = _audioDetector.Scan(_settings.AudioTargets, threshold, _settings.CooldownSeconds).ToList();
			foreach (string signal in signals)
			{
				UpdateStatus("声音信号：" + signal);
				AppendLog("audio signal: " + signal);
				await PlaySoundNudgeAsync(signal);
			}
		}
		catch (Exception ex)
		{
			AppendLog($"audio scan failed: {ex}");
			UpdateStatus("声音扫描失败：" + ex.Message);
		}
		finally
		{
			_audioScanBusy = false;
		}
	}

	private async Task PlaySoundNudgeAsync(string reason)
	{
		ExpressionItem normal = _expressions.FirstOrDefault(x => x.Key == "normal") ?? _expressions[0];
		CaptureUiSettings();
		NotificationPlan plan = _settings.ResolvePlan(reason, normal.ImagePath);
		UpdateCurrentTrigger(plan, reason);
		IElectronLowLevel? bot = _bot;
		if (bot == null || !bot.IsConnected)
		{
			UpdateStatus("命中声音，但机器人未连接：" + reason);
			AppendLog("audio matched but robot disconnected: " + reason);
			return;
		}
		if (_isPlaying || DateTime.Now - _lastSoundNudge < TimeSpan.FromSeconds(_settings.CooldownSeconds))
		{
			return;
		}
		byte[] notificationFaceBuffer = ImageFrameConverter.ToBotBgrBuffer(plan.FacePath);
		byte[] normalFaceBuffer = ImageFrameConverter.ToBotBgrBuffer(normal.ImagePath);
		bool enableServo = _enableServoBox.Checked;
		_isPlaying = true;
		_lastSoundNudge = DateTime.Now;
		SetExpressionButtonsEnabled(false);
		SetPreviewImage(plan.FacePath);
		try
		{
			await Task.Run(async delegate
			{
				int delay = (int)_intervalBox.Value;
				foreach (ExpressionAction frame in plan.Frames)
				{
					_closing.Token.ThrowIfCancellationRequested();
					bot.SetImageSrc(notificationFaceBuffer);
					bot.SetJointAngles(frame.J1, frame.J2, frame.J3, frame.J4, frame.J5, frame.J6, enableServo);
					bot.Sync();
					await Task.Delay(delay, _closing.Token);
				}
				bot.SetImageSrc(normalFaceBuffer);
				bot.SetJointAngles(0f, 0f, 0f, 0f, 0f, 0f, enableServo);
				bot.Sync();
			}, _closing.Token);
			AppendLog("robot sound nudge sent: " + plan.Name + "; " + reason);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex)
		{
			AppendLog($"robot sound nudge failed: {ex}");
			UpdateStatus("声音动作发送失败：" + ex.Message);
		}
		finally
		{
			SetPreviewImage(normal.ImagePath);
			_isPlaying = false;
			SetExpressionButtonsEnabled(true);
		}
	}

	private void UpdateCurrentTrigger(NotificationPlan plan, string reason)
	{
		_currentName.Text = plan.Name;
		_currentBadge.Text = "匹配成功";
		_currentKeywords.Text = "关键词：" + reason.Split(" peak=", StringSplitOptions.None)[0];
		string peak = reason.Contains(" peak=", StringComparison.OrdinalIgnoreCase) ? reason.Split(" peak=", StringSplitOptions.None).Last() : "-";
		_currentPeak.Text = "峰值：" + peak;
		_currentTime.Text = "最后触发：" + DateTime.Now.ToString("HH:mm:ss");
		_currentExpression.Text = "当前表情： " + plan.Name;
		_cooldownLabel.Text = Math.Max(0, _settings.CooldownSeconds) + "s";
		SetSmallImage(_currentIcon, plan.FacePath);
		SetSmallImage(_statusIcon, plan.FacePath);
	}

	private async Task PlayExpressionAsync(ExpressionItem item)
	{
		if (_isPlaying)
		{
			UpdateStatus("上一段表情还在播放，稍等一下。");
			return;
		}
		SetPreviewImage(item.ImagePath);
		_currentExpression.Text = "当前表情： " + item.Title;
		IElectronLowLevel? bot = _bot;
		if (bot == null || !bot.IsConnected)
		{
			UpdateStatus("预览：" + item.Title + "。当前未连接机器人。");
			return;
		}
		_isPlaying = true;
		SetExpressionButtonsEnabled(false);
		UpdateStatus("正在播放：" + item.Title);
		try
		{
			byte[] faceBuffer = ImageFrameConverter.ToBotBgrBuffer(item.ImagePath);
			List<ExpressionAction> actions = await ExpressionAction.LoadAsync(item.ActionPath);
			int delay = (int)_intervalBox.Value;
			bool enableServo = _enableServoBox.Checked;
			await Task.Run(async delegate
			{
				foreach (ExpressionAction action in actions)
				{
					_closing.Token.ThrowIfCancellationRequested();
					bot.SetImageSrc(faceBuffer);
					bot.SetJointAngles(action.J1, action.J2, action.J3, action.J4, action.J5, action.J6, enableServo);
					bot.Sync();
					await Task.Delay(delay, _closing.Token);
				}
			}, _closing.Token);
			UpdateStatus("播放完成：" + item.Title);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex)
		{
			UpdateStatus("播放失败：" + ex.Message);
		}
		finally
		{
			_isPlaying = false;
			SetExpressionButtonsEnabled(true);
		}
	}

	private void SetExpressionButtonsEnabled(bool enabled)
	{
		foreach (Control control in _expressionPanel.Controls)
		{
			control.Enabled = enabled;
		}
	}

	private void UpdateStatus(string text)
	{
		if (InvokeRequired)
		{
			BeginInvoke(delegate { UpdateStatus(text); });
			return;
		}
		_status.Text = $"{DateTime.Now:HH:mm:ss}  {text}";
		_runtimeLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}  {text}");
		while (_runtimeLog.Items.Count > 80)
		{
			_runtimeLog.Items.RemoveAt(_runtimeLog.Items.Count - 1);
		}
	}

	private void SetPreviewImage(string path)
	{
		if (InvokeRequired)
		{
			BeginInvoke(delegate { SetPreviewImage(path); });
			return;
		}
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return;
		}
		_preview.Image?.Dispose();
		_preview.Image = LoadImageSnapshot(path);
		SetSmallImage(_currentIcon, path);
		SetSmallImage(_statusIcon, path);
	}

	private static void SetSmallImage(PictureBox box, string path)
	{
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return;
		}
		box.Image?.Dispose();
		box.Image = LoadImageSnapshot(path);
	}

	private static void AppendLog(string text)
	{
		try
		{
			File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "listener.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {text}{Environment.NewLine}", Encoding.UTF8);
		}
		catch
		{
		}
	}
}
