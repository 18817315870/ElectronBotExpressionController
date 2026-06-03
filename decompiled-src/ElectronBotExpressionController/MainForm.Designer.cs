#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ElectronBotExpressionController;

partial class MainForm
{
	private Panel connectionCard;
	private Panel listenCard;
	private Panel intervalCard;
	private Panel thresholdCard;
	private Panel cooldownCard;
	private Panel expressionCard;
	private Panel currentCard;
	private Panel currentInfoPanel;
	private Panel rulesCard;
	private Panel logCard;
	private Panel statusCard;
	private Label connectionTitleLabel;
	private Label listenTitleLabel;
	private Label intervalTitleLabel;
	private Label thresholdTitleLabel;
	private Label cooldownTitleLabel;
	private Label expressionTitleLabel;
	private Label currentTitleLabel;
	private Label rulesTitleLabel;
	private Label logTitleLabel;
	private Label statusTitleLabel;
	private Label _connectionState;
	private Label _listenState;
	private Label _currentName;
	private Label _currentBadge;
	private Label _currentKeywords;
	private Label _currentPeak;
	private Label _currentTime;
	private Label _robotState;
	private Label _listenStateSmall;
	private Label _currentExpression;
	private Label _cooldownLabel;
	private Label _status;
	private Button _disconnectButton;
	private Button _startListenButton;
	private Button _stopListenButton;
	private Button _testNudgeButton;
	private Button _scheduleSettingsButton;
	private Button _addExpressionButton;
	private Button _deleteProfileButton;
	private Button _captureSourceButton;
	private Button _browseFaceButton;
	private Button _openConfigButton;
	private Button _reloadConfigButton;
	private Button _saveConfigButton;
	private Button _clearCurrentButton;
	private Button _categoryAllButton;
	private Button _categoryActionButton;
	private Button _categoryEmotionButton;
	private Button _categoryVoiceButton;
	private Button _categoryNotifyButton;
	private FlowLayoutPanel _expressionPanel;
	private PictureBox _preview;
	private PictureBox _currentIcon;
	private PictureBox _statusIcon;
	private ListBox _runtimeLog;
	private ProgressBar _cooldownProgress;
	private NumericUpDown _intervalBox;
	private NumericUpDown _thresholdBox;
	private NumericUpDown _cooldownBox;
	private CheckBox _enableServoBox;
	private CheckBox _startupBox;
	private DataGridView _profilesGrid;

    private void InitializeComponent()
    {
		connectionCard = new RoundedPanel();
        connectionTitleLabel = new Label();
        _connectionState = new Label();
        _disconnectButton = new Button();
		listenCard = new RoundedPanel();
        listenTitleLabel = new Label();
        _listenState = new Label();
        _startListenButton = new Button();
        _stopListenButton = new Button();
        _testNudgeButton = new Button();
		intervalCard = new RoundedPanel();
        intervalTitleLabel = new Label();
        _intervalBox = new NumericUpDown();
		thresholdCard = new RoundedPanel();
        thresholdTitleLabel = new Label();
        _thresholdBox = new NumericUpDown();
		cooldownCard = new RoundedPanel();
        cooldownTitleLabel = new Label();
	        _cooldownBox = new NumericUpDown();
	        _enableServoBox = new CheckBox();
	        _startupBox = new CheckBox();
			expressionCard = new RoundedPanel();
        expressionTitleLabel = new Label();
        _categoryAllButton = new Button();
        _categoryActionButton = new Button();
        _categoryEmotionButton = new Button();
        _categoryVoiceButton = new Button();
        _categoryNotifyButton = new Button();
        _scheduleSettingsButton = new Button();
        _addExpressionButton = new Button();
        _expressionPanel = new FlowLayoutPanel();
		currentCard = new RoundedPanel();
        currentTitleLabel = new Label();
        _clearCurrentButton = new Button();
        _preview = new PictureBox();
		currentInfoPanel = new RoundedPanel();
        _currentIcon = new PictureBox();
        _currentName = new Label();
        _currentBadge = new Label();
        _currentKeywords = new Label();
        _currentPeak = new Label();
        _currentTime = new Label();
		rulesCard = new RoundedPanel();
        rulesTitleLabel = new Label();
        _captureSourceButton = new Button();
        _browseFaceButton = new Button();
        _deleteProfileButton = new Button();
        _reloadConfigButton = new Button();
        _openConfigButton = new Button();
        _saveConfigButton = new Button();
        _profilesGrid = new DataGridView();
		logCard = new RoundedPanel();
        logTitleLabel = new Label();
        _runtimeLog = new ListBox();
		statusCard = new RoundedPanel();
        statusTitleLabel = new Label();
        _status = new Label();
        _listenStateSmall = new Label();
        _robotState = new Label();
        _currentExpression = new Label();
        _cooldownLabel = new Label();
        _cooldownProgress = new ProgressBar();
        _statusIcon = new PictureBox();
        connectionCard.SuspendLayout();
        listenCard.SuspendLayout();
        intervalCard.SuspendLayout();
        ((ISupportInitialize)_intervalBox).BeginInit();
        thresholdCard.SuspendLayout();
        ((ISupportInitialize)_thresholdBox).BeginInit();
        cooldownCard.SuspendLayout();
        ((ISupportInitialize)_cooldownBox).BeginInit();
        expressionCard.SuspendLayout();
        currentCard.SuspendLayout();
        ((ISupportInitialize)_preview).BeginInit();
        currentInfoPanel.SuspendLayout();
        ((ISupportInitialize)_currentIcon).BeginInit();
        rulesCard.SuspendLayout();
        ((ISupportInitialize)_profilesGrid).BeginInit();
        logCard.SuspendLayout();
        statusCard.SuspendLayout();
        ((ISupportInitialize)_statusIcon).BeginInit();
        SuspendLayout();
        // 
        // connectionCard
        // 
        connectionCard.BackColor = Color.White;
        connectionCard.Controls.Add(connectionTitleLabel);
        connectionCard.Controls.Add(_connectionState);
        connectionCard.Controls.Add(_disconnectButton);
        connectionCard.Location = new Point(15, 15);
        connectionCard.Name = "connectionCard";
        connectionCard.Size = new Size(260, 78);
        connectionCard.TabIndex = 0;
        // 
        // connectionTitleLabel
        // 
        connectionTitleLabel.ForeColor = Color.FromArgb(51, 65, 85);
        connectionTitleLabel.Location = new Point(18, 14);
        connectionTitleLabel.Name = "connectionTitleLabel";
        connectionTitleLabel.Size = new Size(102, 22);
        connectionTitleLabel.TabIndex = 0;
        connectionTitleLabel.Text = "连接状态";
        // 
        // _connectionState
        // 
        _connectionState.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
        _connectionState.Location = new Point(17, 38);
        _connectionState.Name = "_connectionState";
        _connectionState.Size = new Size(98, 24);
        _connectionState.TabIndex = 1;
        _connectionState.Text = "● 未连接";
        // 
        // _disconnectButton
        // 
        _disconnectButton.Location = new Point(140, 20);
        _disconnectButton.Name = "_disconnectButton";
        _disconnectButton.Size = new Size(100, 45);
        _disconnectButton.TabIndex = 2;
        _disconnectButton.Text = "连接";
        _disconnectButton.UseVisualStyleBackColor = true;
        // 
        // listenCard
        // 
        listenCard.BackColor = Color.White;
        listenCard.Controls.Add(listenTitleLabel);
        listenCard.Controls.Add(_listenState);
        listenCard.Controls.Add(_startListenButton);
        listenCard.Controls.Add(_stopListenButton);
        listenCard.Controls.Add(_testNudgeButton);
        listenCard.Location = new Point(285, 15);
        listenCard.Name = "listenCard";
        listenCard.Size = new Size(390, 78);
        listenCard.TabIndex = 1;
        // 
        // listenTitleLabel
        // 
        listenTitleLabel.ForeColor = Color.FromArgb(51, 65, 85);
        listenTitleLabel.Location = new Point(18, 14);
        listenTitleLabel.Name = "listenTitleLabel";
        listenTitleLabel.Size = new Size(92, 22);
        listenTitleLabel.TabIndex = 0;
        listenTitleLabel.Text = "监听状态";
        // 
        // _listenState
        // 
        _listenState.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
        _listenState.Location = new Point(18, 36);
        _listenState.Name = "_listenState";
        _listenState.Size = new Size(88, 24);
        _listenState.TabIndex = 1;
        _listenState.Text = "未监听";
        // 
        // _startListenButton
        // 
        _startListenButton.Location = new Point(115, 26);
        _startListenButton.Name = "_startListenButton";
        _startListenButton.Size = new Size(82, 34);
        _startListenButton.TabIndex = 2;
        _startListenButton.Text = "开始监听";
        _startListenButton.UseVisualStyleBackColor = true;
        // 
        // _stopListenButton
        // 
        _stopListenButton.Location = new Point(207, 26);
        _stopListenButton.Name = "_stopListenButton";
        _stopListenButton.Size = new Size(82, 34);
        _stopListenButton.TabIndex = 3;
        _stopListenButton.Text = "停止监听";
        _stopListenButton.UseVisualStyleBackColor = true;
        // 
        // _testNudgeButton
        // 
        _testNudgeButton.Location = new Point(300, 26);
        _testNudgeButton.Name = "_testNudgeButton";
        _testNudgeButton.Size = new Size(82, 34);
        _testNudgeButton.TabIndex = 4;
        _testNudgeButton.Text = "测试提醒";
        _testNudgeButton.UseVisualStyleBackColor = true;
        // 
        // intervalCard
        // 
        intervalCard.BackColor = Color.White;
        intervalCard.Controls.Add(intervalTitleLabel);
        intervalCard.Controls.Add(_intervalBox);
        intervalCard.Location = new Point(690, 15);
        intervalCard.Name = "intervalCard";
        intervalCard.Size = new Size(110, 78);
        intervalCard.TabIndex = 2;
        // 
        // intervalTitleLabel
        // 
        intervalTitleLabel.ForeColor = Color.FromArgb(51, 65, 85);
        intervalTitleLabel.Location = new Point(16, 14);
        intervalTitleLabel.Name = "intervalTitleLabel";
        intervalTitleLabel.Size = new Size(90, 22);
        intervalTitleLabel.TabIndex = 0;
        intervalTitleLabel.Text = "帧间隔 (ms)";
        // 
        // _intervalBox
        // 
        _intervalBox.Location = new Point(18, 44);
        _intervalBox.Name = "_intervalBox";
        _intervalBox.Size = new Size(76, 27);
        _intervalBox.TabIndex = 1;
        _intervalBox.Value = new decimal(new int[] { 100, 0, 0, 0 });
        // 
        // thresholdCard
        // 
        thresholdCard.BackColor = Color.White;
        thresholdCard.Controls.Add(thresholdTitleLabel);
        thresholdCard.Controls.Add(_thresholdBox);
        thresholdCard.Location = new Point(818, 15);
        thresholdCard.Name = "thresholdCard";
        thresholdCard.Size = new Size(110, 78);
        thresholdCard.TabIndex = 3;
        // 
        // thresholdTitleLabel
        // 
        thresholdTitleLabel.ForeColor = Color.FromArgb(51, 65, 85);
        thresholdTitleLabel.Location = new Point(16, 14);
        thresholdTitleLabel.Name = "thresholdTitleLabel";
        thresholdTitleLabel.Size = new Size(90, 22);
        thresholdTitleLabel.TabIndex = 0;
        thresholdTitleLabel.Text = "阈值 (%)";
        // 
        // _thresholdBox
        // 
        _thresholdBox.Location = new Point(18, 44);
        _thresholdBox.Name = "_thresholdBox";
        _thresholdBox.Size = new Size(76, 27);
        _thresholdBox.TabIndex = 1;
        _thresholdBox.Value = new decimal(new int[] { 2, 0, 0, 0 });
        // 
        // cooldownCard
        // 
        cooldownCard.BackColor = Color.White;
        cooldownCard.Controls.Add(cooldownTitleLabel);
        cooldownCard.Controls.Add(_cooldownBox);
        cooldownCard.Location = new Point(946, 15);
        cooldownCard.Name = "cooldownCard";
        cooldownCard.Size = new Size(110, 78);
        cooldownCard.TabIndex = 4;
        // 
        // cooldownTitleLabel
        // 
        cooldownTitleLabel.ForeColor = Color.FromArgb(51, 65, 85);
        cooldownTitleLabel.Location = new Point(16, 14);
        cooldownTitleLabel.Name = "cooldownTitleLabel";
        cooldownTitleLabel.Size = new Size(90, 22);
        cooldownTitleLabel.TabIndex = 0;
        cooldownTitleLabel.Text = "冷却时间 (s)";
        // 
        // _cooldownBox
        // 
        _cooldownBox.Location = new Point(18, 44);
        _cooldownBox.Name = "_cooldownBox";
        _cooldownBox.Size = new Size(76, 27);
        _cooldownBox.TabIndex = 1;
        _cooldownBox.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // _enableServoBox
        // 
        _enableServoBox.AutoSize = true;
        _enableServoBox.Location = new Point(1078, 56);
        _enableServoBox.Name = "_enableServoBox";
        _enableServoBox.Size = new Size(91, 24);
        _enableServoBox.TabIndex = 5;
	        _enableServoBox.Text = "启用舵机";
	        _enableServoBox.UseVisualStyleBackColor = true;
	        // 
	        // _startupBox
	        // 
	        _startupBox.AutoSize = true;
	        _startupBox.Location = new Point(1078, 24);
	        _startupBox.Name = "_startupBox";
	        _startupBox.Size = new Size(91, 24);
	        _startupBox.TabIndex = 6;
	        _startupBox.Text = "开机启动";
	        _startupBox.UseVisualStyleBackColor = true;
	        // 
	        // expressionCard
        // 
        expressionCard.BackColor = Color.White;
        expressionCard.Controls.Add(expressionTitleLabel);
        expressionCard.Controls.Add(_categoryAllButton);
        expressionCard.Controls.Add(_categoryActionButton);
        expressionCard.Controls.Add(_categoryEmotionButton);
        expressionCard.Controls.Add(_categoryVoiceButton);
        expressionCard.Controls.Add(_categoryNotifyButton);
        expressionCard.Controls.Add(_scheduleSettingsButton);
        expressionCard.Controls.Add(_addExpressionButton);
        expressionCard.Controls.Add(_expressionPanel);
        expressionCard.Location = new Point(15, 115);
        expressionCard.Name = "expressionCard";
        expressionCard.Size = new Size(510, 470);
        expressionCard.TabIndex = 6;
        // 
        // expressionTitleLabel
        // 
        expressionTitleLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        expressionTitleLabel.Location = new Point(12, 10);
        expressionTitleLabel.Name = "expressionTitleLabel";
        expressionTitleLabel.Size = new Size(180, 22);
        expressionTitleLabel.TabIndex = 0;
        expressionTitleLabel.Text = "表情库";
        // 
        // _categoryAllButton
        // 
        _categoryAllButton.Location = new Point(14, 46);
        _categoryAllButton.Name = "_categoryAllButton";
        _categoryAllButton.Size = new Size(54, 28);
        _categoryAllButton.TabIndex = 1;
        _categoryAllButton.Text = "全部";
        _categoryAllButton.UseVisualStyleBackColor = true;
        // 
        // _categoryActionButton
        // 
        _categoryActionButton.Location = new Point(78, 46);
        _categoryActionButton.Name = "_categoryActionButton";
        _categoryActionButton.Size = new Size(54, 28);
        _categoryActionButton.TabIndex = 2;
        _categoryActionButton.Text = "动作";
        _categoryActionButton.UseVisualStyleBackColor = true;
        // 
        // _categoryEmotionButton
        // 
        _categoryEmotionButton.Location = new Point(142, 46);
        _categoryEmotionButton.Name = "_categoryEmotionButton";
        _categoryEmotionButton.Size = new Size(54, 28);
        _categoryEmotionButton.TabIndex = 3;
        _categoryEmotionButton.Text = "情绪";
        _categoryEmotionButton.UseVisualStyleBackColor = true;
        // 
        // _categoryVoiceButton
        // 
        _categoryVoiceButton.Location = new Point(206, 46);
        _categoryVoiceButton.Name = "_categoryVoiceButton";
        _categoryVoiceButton.Size = new Size(82, 28);
        _categoryVoiceButton.TabIndex = 4;
        _categoryVoiceButton.Text = "语音互动";
        _categoryVoiceButton.UseVisualStyleBackColor = true;
        // 
        // _categoryNotifyButton
        // 
        _categoryNotifyButton.Location = new Point(303, 46);
        _categoryNotifyButton.Name = "_categoryNotifyButton";
        _categoryNotifyButton.Size = new Size(82, 28);
        _categoryNotifyButton.TabIndex = 5;
        _categoryNotifyButton.Text = "通知提醒";
        _categoryNotifyButton.UseVisualStyleBackColor = true;
        // 
        // _scheduleSettingsButton
        // 
        _scheduleSettingsButton.Location = new Point(396, 43);
        _scheduleSettingsButton.Name = "_scheduleSettingsButton";
        _scheduleSettingsButton.Size = new Size(84, 30);
        _scheduleSettingsButton.TabIndex = 6;
        _scheduleSettingsButton.Text = "定时表情";
        _scheduleSettingsButton.UseVisualStyleBackColor = true;
        // 
        // _addExpressionButton
        // 
        _addExpressionButton.Location = new Point(396, 10);
        _addExpressionButton.Name = "_addExpressionButton";
        _addExpressionButton.Size = new Size(88, 30);
        _addExpressionButton.TabIndex = 7;
        _addExpressionButton.Text = "+ 添加表情";
        _addExpressionButton.UseVisualStyleBackColor = true;
        // _expressionPanel
        // 
        _expressionPanel.AutoScroll = true;
        _expressionPanel.BackColor = Color.White;
        _expressionPanel.Location = new Point(15, 80);
        _expressionPanel.Name = "_expressionPanel";
        _expressionPanel.Size = new Size(480, 380);
        _expressionPanel.TabIndex = 8;
        // 
        // currentCard
        // 
        currentCard.BackColor = Color.White;
        currentCard.Controls.Add(currentTitleLabel);
        currentCard.Controls.Add(_clearCurrentButton);
        currentCard.Controls.Add(_preview);
        currentCard.Controls.Add(currentInfoPanel);
        currentCard.Location = new Point(540, 115);
        currentCard.Name = "currentCard";
        currentCard.Size = new Size(645, 210);
        currentCard.TabIndex = 7;
        // 
        // currentTitleLabel
        // 
        currentTitleLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        currentTitleLabel.Location = new Point(12, 10);
        currentTitleLabel.Name = "currentTitleLabel";
        currentTitleLabel.Size = new Size(180, 22);
        currentTitleLabel.TabIndex = 0;
        currentTitleLabel.Text = "当前触发表情";
        // 
        // _clearCurrentButton
        // 
        _clearCurrentButton.Location = new Point(545, 16);
        _clearCurrentButton.Name = "_clearCurrentButton";
        _clearCurrentButton.Size = new Size(84, 28);
        _clearCurrentButton.TabIndex = 1;
        _clearCurrentButton.Text = "清除";
        _clearCurrentButton.UseVisualStyleBackColor = true;
        // 
        // _preview
        // 
        _preview.BackColor = Color.Black;
        _preview.Location = new Point(18, 56);
        _preview.Name = "_preview";
        _preview.Size = new Size(290, 136);
        _preview.TabIndex = 2;
        _preview.TabStop = false;
        // 
        // currentInfoPanel
        // 
        currentInfoPanel.BackColor = Color.White;
        currentInfoPanel.Controls.Add(_currentIcon);
        currentInfoPanel.Controls.Add(_currentName);
        currentInfoPanel.Controls.Add(_currentBadge);
        currentInfoPanel.Controls.Add(_currentKeywords);
        currentInfoPanel.Controls.Add(_currentPeak);
        currentInfoPanel.Controls.Add(_currentTime);
        currentInfoPanel.Location = new Point(330, 56);
        currentInfoPanel.Name = "currentInfoPanel";
        currentInfoPanel.Size = new Size(300, 136);
        currentInfoPanel.TabIndex = 3;
        // 
        // _currentIcon
        // 
        _currentIcon.BackColor = Color.Black;
        _currentIcon.Location = new Point(22, 24);
        _currentIcon.Name = "_currentIcon";
        _currentIcon.Size = new Size(40, 40);
        _currentIcon.TabIndex = 0;
        _currentIcon.TabStop = false;
        // 
        // _currentName
        // 
        _currentName.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
        _currentName.Location = new Point(78, 24);
        _currentName.Name = "_currentName";
        _currentName.Size = new Size(115, 28);
        _currentName.TabIndex = 1;
        _currentName.Text = "等待通知";
        // 
        // _currentBadge
        // 
        _currentBadge.BackColor = Color.FromArgb(220, 252, 231);
        _currentBadge.ForeColor = Color.FromArgb(22, 163, 74);
        _currentBadge.Location = new Point(200, 26);
        _currentBadge.Name = "_currentBadge";
        _currentBadge.Size = new Size(70, 24);
        _currentBadge.TabIndex = 2;
        _currentBadge.Text = "未触发";
        _currentBadge.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // _currentKeywords
        // 
        _currentKeywords.Location = new Point(22, 78);
        _currentKeywords.Name = "_currentKeywords";
        _currentKeywords.Size = new Size(110, 22);
        _currentKeywords.TabIndex = 3;
        _currentKeywords.Text = "关键词：-";
        // 
        // _currentPeak
        // 
        _currentPeak.Location = new Point(172, 78);
        _currentPeak.Name = "_currentPeak";
        _currentPeak.Size = new Size(110, 22);
        _currentPeak.TabIndex = 4;
        _currentPeak.Text = "峰值：-";
        // 
        // _currentTime
        // 
        _currentTime.Location = new Point(22, 100);
        _currentTime.Name = "_currentTime";
        _currentTime.Size = new Size(260, 22);
        _currentTime.TabIndex = 5;
        _currentTime.Text = "最后触发：-";
        // 
        // rulesCard
        // 
        rulesCard.BackColor = Color.White;
        rulesCard.Controls.Add(rulesTitleLabel);
        rulesCard.Controls.Add(_captureSourceButton);
        rulesCard.Controls.Add(_browseFaceButton);
        rulesCard.Controls.Add(_deleteProfileButton);
        rulesCard.Controls.Add(_reloadConfigButton);
        rulesCard.Controls.Add(_openConfigButton);
        rulesCard.Controls.Add(_saveConfigButton);
        rulesCard.Controls.Add(_profilesGrid);
        rulesCard.Location = new Point(540, 335);
        rulesCard.Name = "rulesCard";
        rulesCard.Size = new Size(645, 250);
        rulesCard.TabIndex = 8;
        // 
        // rulesTitleLabel
        // 
        rulesTitleLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        rulesTitleLabel.Location = new Point(12, 10);
        rulesTitleLabel.Name = "rulesTitleLabel";
        rulesTitleLabel.Size = new Size(180, 22);
        rulesTitleLabel.TabIndex = 0;
        rulesTitleLabel.Text = "通知规则";
        // 
        // _captureSourceButton
        // 
        _captureSourceButton.Location = new Point(212, 38);
        _captureSourceButton.Name = "_captureSourceButton";
        _captureSourceButton.Size = new Size(70, 30);
        _captureSourceButton.TabIndex = 1;
        _captureSourceButton.Text = "+ 新增";
        _captureSourceButton.UseVisualStyleBackColor = true;
        // 
        // _browseFaceButton
        // 
        _browseFaceButton.Location = new Point(290, 38);
        _browseFaceButton.Name = "_browseFaceButton";
        _browseFaceButton.Size = new Size(58, 30);
        _browseFaceButton.TabIndex = 2;
        _browseFaceButton.Text = "编辑";
        _browseFaceButton.UseVisualStyleBackColor = true;
        // 
        // _deleteProfileButton
        // 
        _deleteProfileButton.Location = new Point(356, 38);
        _deleteProfileButton.Name = "_deleteProfileButton";
        _deleteProfileButton.Size = new Size(58, 30);
        _deleteProfileButton.TabIndex = 3;
        _deleteProfileButton.Text = "删除";
        _deleteProfileButton.UseVisualStyleBackColor = true;
        // 
        // _reloadConfigButton
        // 
        _reloadConfigButton.Location = new Point(422, 38);
        _reloadConfigButton.Name = "_reloadConfigButton";
        _reloadConfigButton.Size = new Size(58, 30);
        _reloadConfigButton.TabIndex = 4;
        _reloadConfigButton.Text = "导入";
        _reloadConfigButton.UseVisualStyleBackColor = true;
        // 
        // _openConfigButton
        // 
        _openConfigButton.Location = new Point(488, 38);
        _openConfigButton.Name = "_openConfigButton";
        _openConfigButton.Size = new Size(58, 30);
        _openConfigButton.TabIndex = 5;
        _openConfigButton.Text = "导出";
        _openConfigButton.UseVisualStyleBackColor = true;
        // 
        // _saveConfigButton
        // 
        _saveConfigButton.Location = new Point(554, 38);
        _saveConfigButton.Name = "_saveConfigButton";
        _saveConfigButton.Size = new Size(58, 30);
        _saveConfigButton.TabIndex = 6;
        _saveConfigButton.Text = "保存";
        _saveConfigButton.UseVisualStyleBackColor = true;
        // 
        // _profilesGrid
        // 
        _profilesGrid.ColumnHeadersHeight = 29;
        _profilesGrid.Location = new Point(15, 82);
        _profilesGrid.Name = "_profilesGrid";
        _profilesGrid.RowHeadersWidth = 51;
        _profilesGrid.Size = new Size(615, 156);
        _profilesGrid.TabIndex = 7;
        // 
        // logCard
        // 
        logCard.BackColor = Color.White;
        logCard.Controls.Add(logTitleLabel);
        logCard.Controls.Add(_runtimeLog);
        logCard.Location = new Point(15, 600);
        logCard.Name = "logCard";
        logCard.Size = new Size(760, 175);
        logCard.TabIndex = 9;
        // 
        // logTitleLabel
        // 
        logTitleLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        logTitleLabel.Location = new Point(12, 10);
        logTitleLabel.Name = "logTitleLabel";
        logTitleLabel.Size = new Size(180, 22);
        logTitleLabel.TabIndex = 0;
        logTitleLabel.Text = "运行日志";
        // 
        // _runtimeLog
        // 
        _runtimeLog.BackColor = Color.White;
        _runtimeLog.BorderStyle = BorderStyle.None;
        _runtimeLog.Font = new Font("Microsoft YaHei UI", 8.5F);
        _runtimeLog.Location = new Point(18, 48);
        _runtimeLog.Name = "_runtimeLog";
        _runtimeLog.Size = new Size(725, 100);
        _runtimeLog.TabIndex = 1;
        // 
        // statusCard
        // 
        statusCard.BackColor = Color.White;
        statusCard.Controls.Add(statusTitleLabel);
        statusCard.Controls.Add(_status);
        statusCard.Controls.Add(_listenStateSmall);
        statusCard.Controls.Add(_robotState);
        statusCard.Controls.Add(_currentExpression);
        statusCard.Controls.Add(_cooldownLabel);
        statusCard.Controls.Add(_cooldownProgress);
        statusCard.Controls.Add(_statusIcon);
        statusCard.Location = new Point(790, 600);
        statusCard.Name = "statusCard";
        statusCard.Size = new Size(395, 175);
        statusCard.TabIndex = 10;
        // 
        // statusTitleLabel
        // 
        statusTitleLabel.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        statusTitleLabel.Location = new Point(12, 10);
        statusTitleLabel.Name = "statusTitleLabel";
        statusTitleLabel.Size = new Size(180, 22);
        statusTitleLabel.TabIndex = 0;
        statusTitleLabel.Text = "状态信息";
        // 
        // _status
        // 
        _status.Location = new Point(147, 10);
        _status.Name = "_status";
        _status.Size = new Size(215, 22);
        _status.TabIndex = 1;
        _status.Visible = false;
        // 
        // _listenStateSmall
        // 
        _listenStateSmall.Location = new Point(24, 48);
        _listenStateSmall.Name = "_listenStateSmall";
        _listenStateSmall.Size = new Size(160, 22);
        _listenStateSmall.TabIndex = 2;
        _listenStateSmall.Text = "监听状态：-";
        // 
        // _robotState
        // 
        _robotState.Location = new Point(24, 76);
        _robotState.Name = "_robotState";
        _robotState.Size = new Size(160, 22);
        _robotState.TabIndex = 3;
        _robotState.Text = "机器人状态：-";
        // 
        // _currentExpression
        // 
        _currentExpression.Location = new Point(24, 104);
        _currentExpression.Name = "_currentExpression";
        _currentExpression.Size = new Size(160, 22);
        _currentExpression.TabIndex = 4;
        _currentExpression.Text = "当前表情：-";
        // 
        // _cooldownLabel
        // 
        _cooldownLabel.Location = new Point(28, 126);
        _cooldownLabel.Name = "_cooldownLabel";
        _cooldownLabel.Size = new Size(70, 22);
        _cooldownLabel.TabIndex = 5;
        // 
        // _cooldownProgress
        // 
        _cooldownProgress.Location = new Point(147, 129);
        _cooldownProgress.Name = "_cooldownProgress";
        _cooldownProgress.Size = new Size(114, 10);
        _cooldownProgress.TabIndex = 6;
        _cooldownProgress.Value = 65;
        // 
        // _statusIcon
        // 
        _statusIcon.BackColor = Color.Black;
        _statusIcon.Location = new Point(298, 88);
        _statusIcon.Name = "_statusIcon";
        _statusIcon.Size = new Size(44, 44);
        _statusIcon.TabIndex = 7;
        _statusIcon.TabStop = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(247, 250, 252);
        ClientSize = new Size(1200, 800);
        Controls.Add(connectionCard);
        Controls.Add(listenCard);
        Controls.Add(intervalCard);
        Controls.Add(thresholdCard);
	        Controls.Add(cooldownCard);
	        Controls.Add(_startupBox);
	        Controls.Add(_enableServoBox);
        Controls.Add(expressionCard);
        Controls.Add(currentCard);
        Controls.Add(rulesCard);
        Controls.Add(logCard);
        Controls.Add(statusCard);
        Font = new Font("Microsoft YaHei UI", 9F);
        MinimumSize = new Size(1100, 720);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ElectronBot 通知声音监听 V3";
        connectionCard.ResumeLayout(false);
        listenCard.ResumeLayout(false);
        intervalCard.ResumeLayout(false);
        ((ISupportInitialize)_intervalBox).EndInit();
        thresholdCard.ResumeLayout(false);
        ((ISupportInitialize)_thresholdBox).EndInit();
        cooldownCard.ResumeLayout(false);
        ((ISupportInitialize)_cooldownBox).EndInit();
        expressionCard.ResumeLayout(false);
        currentCard.ResumeLayout(false);
        ((ISupportInitialize)_preview).EndInit();
        currentInfoPanel.ResumeLayout(false);
        ((ISupportInitialize)_currentIcon).EndInit();
        rulesCard.ResumeLayout(false);
        ((ISupportInitialize)_profilesGrid).EndInit();
        logCard.ResumeLayout(false);
        statusCard.ResumeLayout(false);
        ((ISupportInitialize)_statusIcon).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

}
