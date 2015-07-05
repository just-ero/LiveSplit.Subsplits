using Fetze.WinFormsColor;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public partial class SplitsSettings : UserControl
    {
        private int _VisualSplitCount { get; set; }
        public int VisualSplitCount
        {
            get { return _VisualSplitCount; }
            set
            {
                _VisualSplitCount = value;
                var max = Math.Max(0, _VisualSplitCount - (AlwaysShowLastSplit ? 2 : 1));
                if (dmnUpcomingSegments.Value > max)
                    dmnUpcomingSegments.Value = max;
                dmnUpcomingSegments.Maximum = max;
            }
        }
        static public ISegment HilightSplit { get; set; }
        static public ISegment SectionSplit { get; set; }

        public Color CurrentSplitTopColor { get; set; }
        public Color CurrentSplitBottomColor { get; set; }
        public int SplitPreviewCount { get; set; }
        public float SplitWidth { get; set; }
        public float SplitHeight { get; set; }
        public float ScaledSplitHeight { get { return SplitHeight * 10f; } set { SplitHeight = value / 10f; } }
        public float IconSize { get; set; }

        public bool Display2Rows { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }

        public ExtendedGradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (ExtendedGradientType)Enum.Parse(typeof(ExtendedGradientType), value); }
        }

        public String Comparison { get; set; }
        public LiveSplitState CurrentState { get; set; }

        public bool DisplayIcons { get; set; }
        public bool IconShadows { get; set; }
        public bool HideBlankIcons { get; set; }
        public bool ShowThinSeparators { get; set; }
        public bool AlwaysShowLastSplit { get; set; }
        public bool ShowSplitTimes { get; set; }
        public bool ShowBlankSplits { get; set; }
        public bool LockLastSplit { get; set; }
        public bool SeparatorLastSplit { get; set; }

        public bool IndentSubsplits { get; set; }
        public bool HideSubsplits { get; set; }
        public bool ShowSubsplits { get; set; }
        public bool CurrentSectionOnly { get; set; }
        public bool OverrideSubsplitColor { get; set; }
        public Color SubsplitTopColor { get; set; }
        public Color SubsplitBottomColor { get; set; }
        public GradientType SubsplitGradient { get; set; }
        public String SubsplitGradientString
        {
            get { return SubsplitGradient.ToString(); }
            set { SubsplitGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public bool ShowHeader { get; set; }
        public bool IndentSectionSplit { get; set; }
        public bool ShowIconSectionSplit { get; set; }
        public bool ShowSectionIcon { get; set; }
        public Color HeaderTopColor { get; set; }
        public Color HeaderBottomColor { get; set; }
        public GradientType HeaderGradient { get; set; }
        public String HeaderGradientString
        {
            get { return HeaderGradient.ToString(); }
            set { HeaderGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }
        public bool OverrideHeaderColor { get; set; }
        public Color HeaderTextColor { get; set; }
        public bool HeaderText { get; set; }
        public Color HeaderTimesColor { get; set; }
        public bool HeaderTimes { get; set; }
        public TimeAccuracy HeaderAccuracy { get; set; }
        public bool SectionTimer { get; set; }
        public Color SectionTimerColor { get; set; }
        public bool SectionTimerGradient { get; set; }
        public TimeAccuracy SectionTimerAccuracy { get; set; }
        
        public bool DropDecimals { get; set; }
        public TimeAccuracy DeltasAccuracy { get; set; }

        public bool OverrideDeltasColor { get; set; }
        public Color DeltasColor { get; set; }

        public Color BeforeNamesColor { get; set; }
        public Color CurrentNamesColor { get; set; }
        public Color AfterNamesColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color BeforeTimesColor { get; set; }
        public Color CurrentTimesColor { get; set; }
        public Color AfterTimesColor { get; set; }
        public bool OverrideTimesColor { get; set; }

        public TimeAccuracy SplitTimesAccuracy { get; set; }
        public GradientType CurrentSplitGradient { get; set; }
        public String SplitGradientString { get { return CurrentSplitGradient.ToString(); } 
            set { CurrentSplitGradient = (GradientType)Enum.Parse(typeof(GradientType), value); } }

        public event EventHandler SplitLayoutChanged;

        public LayoutMode Mode { get; set; }

        public SplitsSettings()
        {
            InitializeComponent();
            VisualSplitCount = 8;
            SplitPreviewCount = 1;
            DisplayIcons = true;
            IconShadows = true;
            ShowThinSeparators = false;
            AlwaysShowLastSplit = true;
            ShowSplitTimes = false;
            ShowBlankSplits = true;
            LockLastSplit = true;
            SeparatorLastSplit = true;
            SplitTimesAccuracy = TimeAccuracy.Seconds;
            CurrentSplitTopColor = Color.FromArgb(51, 115, 244);
            CurrentSplitBottomColor = Color.FromArgb(21, 53, 116);
            SplitWidth = 20;
            SplitHeight = 3.6f;
            ScaledSplitHeight = 60;
            IconSize = 24f;
            BeforeNamesColor = Color.FromArgb(255, 255, 255);
            CurrentNamesColor = Color.FromArgb(255, 255, 255);
            AfterNamesColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            BeforeTimesColor = Color.FromArgb(255, 255, 255);
            CurrentTimesColor = Color.FromArgb(255, 255, 255);
            AfterTimesColor = Color.FromArgb(255, 255, 255);
            OverrideTimesColor = false;
            CurrentSplitGradient = GradientType.Vertical;
            cmbSplitGradient.SelectedIndexChanged += cmbSplitGradient_SelectedIndexChanged;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.FromArgb(1, 255, 255, 255);
            BackgroundGradient = ExtendedGradientType.Alternating;
            DropDecimals = true;
            DeltasAccuracy = TimeAccuracy.Tenths;
            OverrideDeltasColor = false;
            DeltasColor = Color.FromArgb(255, 255, 255);
            Comparison = "Current Comparison";
            Display2Rows = false;

            HideBlankIcons = true;
            IndentSubsplits = true;
            HideSubsplits = false;
            ShowSubsplits = false;
            CurrentSectionOnly = false;
            OverrideSubsplitColor = false;
            SubsplitTopColor = Color.FromArgb(0x8D, 0x00, 0x00, 0x00);
            SubsplitBottomColor = Color.Transparent;
            SubsplitGradient = GradientType.Plain;
            ShowHeader = true;
            IndentSectionSplit = true;
            ShowIconSectionSplit = true;
            ShowSectionIcon = true;
            HeaderTopColor = Color.FromArgb(0x2B, 0xFF, 0xFF, 0xFF);
            HeaderBottomColor = Color.FromArgb(0xD8, 0x00, 0x00, 0x00);
            HeaderGradient = GradientType.Vertical;
            OverrideHeaderColor = false;
            HeaderTextColor = Color.FromArgb(255, 255, 255);
            HeaderText = true;
            HeaderTimesColor = Color.FromArgb(255, 255, 255);
            HeaderTimes = true;
            HeaderAccuracy = TimeAccuracy.Tenths;
            SectionTimer = true;
            SectionTimerColor = Color.FromArgb(0x77, 0x77, 0x77);
            SectionTimerGradient = true;
            SectionTimerAccuracy = TimeAccuracy.Tenths;

            dmnTotalSegments.DataBindings.Add("Value", this, "VisualSplitCount", false, DataSourceUpdateMode.OnPropertyChanged);
            dmnUpcomingSegments.DataBindings.Add("Value", this, "SplitPreviewCount", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTopColor.DataBindings.Add("BackColor", this, "CurrentSplitTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBottomColor.DataBindings.Add("BackColor", this, "CurrentSplitBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBeforeNamesColor.DataBindings.Add("BackColor", this, "BeforeNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnCurrentNamesColor.DataBindings.Add("BackColor", this, "CurrentNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnAfterNamesColor.DataBindings.Add("BackColor", this, "AfterNamesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnBeforeTimesColor.DataBindings.Add("BackColor", this, "BeforeTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnCurrentTimesColor.DataBindings.Add("BackColor", this, "CurrentTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnAfterTimesColor.DataBindings.Add("BackColor", this, "AfterTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDisplayIcons.DataBindings.Add("Checked", this, "DisplayIcons", false, DataSourceUpdateMode.OnPropertyChanged);
            chkIconShadows.DataBindings.Add("Checked", this, "IconShadows", false, DataSourceUpdateMode.OnPropertyChanged);
            chkHideBlankIcons.DataBindings.Add("Checked", this, "HideBlankIcons", false, DataSourceUpdateMode.OnPropertyChanged);
            chkThinSeparators.DataBindings.Add("Checked", this, "ShowThinSeparators", false, DataSourceUpdateMode.OnPropertyChanged);
            chkLastSplit.DataBindings.Add("Checked", this, "AlwaysShowLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowTimes.DataBindings.Add("Checked", this, "ShowSplitTimes", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTimesColor.DataBindings.Add("Checked", this, "OverrideTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowBlankSplits.DataBindings.Add("Checked", this, "ShowBlankSplits", false, DataSourceUpdateMode.OnPropertyChanged);
            chkLockLastSplit.DataBindings.Add("Checked", this, "LockLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSeparatorLastSplit.DataBindings.Add("Checked", this, "SeparatorLastSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkDropDecimals.DataBindings.Add("Checked", this, "DropDecimals", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideDeltaColor.DataBindings.Add("Checked", this, "OverrideDeltasColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnDeltaColor.DataBindings.Add("BackColor", this, "DeltasColor", false, DataSourceUpdateMode.OnPropertyChanged);

            chkIndentSubsplits.DataBindings.Add("Checked", this, "IndentSubsplits", false, DataSourceUpdateMode.OnPropertyChanged);
            chkCurrentSectionOnly.DataBindings.Add("Checked", this, "CurrentSectionOnly", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideSubsplitColor.DataBindings.Add("Checked", this, "OverrideSubsplitColor", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbSubsplitGradient.DataBindings.Add("SelectedItem", this, "SubsplitGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSubsplitTopColor.DataBindings.Add("BackColor", this, "SubsplitTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSubsplitBottomColor.DataBindings.Add("BackColor", this, "SubsplitBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);

            chkShowHeader.DataBindings.Add("Checked", this, "ShowHeader", false, DataSourceUpdateMode.OnPropertyChanged);
            chkIndentSectionSplit.DataBindings.Add("Checked", this, "IndentSectionSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowIconSectionSplit.DataBindings.Add("Checked", this, "ShowIconSectionSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowSectionIcon.DataBindings.Add("Checked", this, "ShowSectionIcon", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTopColor.DataBindings.Add("BackColor", this, "HeaderTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderBottomColor.DataBindings.Add("BackColor", this, "HeaderBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbHeaderGradient.DataBindings.Add("SelectedItem", this, "HeaderGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideHeaderColor.DataBindings.Add("Checked", this, "OverrideHeaderColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTextColor.DataBindings.Add("BackColor", this, "HeaderTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkHeaderText.DataBindings.Add("Checked", this, "HeaderText", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTimesColor.DataBindings.Add("BackColor", this, "HeaderTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkHeaderTimes.DataBindings.Add("Checked", this, "HeaderTimes", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSectionTimer.DataBindings.Add("Checked", this, "SectionTimer", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSectionTimerColor.DataBindings.Add("BackColor", this, "SectionTimerColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSectionTimerGradient.DataBindings.Add("Checked", this, "SectionTimerGradient", false, DataSourceUpdateMode.OnPropertyChanged);

            trkIconSize.DataBindings.Add("Value", this, "IconSize", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbSplitGradient.DataBindings.Add("SelectedItem", this, "SplitGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        void chkDisplayIcons_CheckedChanged(object sender, EventArgs e)
        {
            trkIconSize.Enabled = label5.Enabled = chkIconShadows.Enabled = chkDisplayIcons.Checked;
        }

        void chkHideBlankIcons_CheckedChanged(object sender, EventArgs e)
        {
            SplitLayoutChanged(this, null);
        }

        void chkOverrideTimesColor_CheckedChanged(object sender, EventArgs e)
        {
            label6.Enabled = label9.Enabled = label7.Enabled = btnBeforeTimesColor.Enabled
                = btnCurrentTimesColor.Enabled = btnAfterTimesColor.Enabled = chkOverrideTimesColor.Checked;
        }

        void chkOverrideDeltaColor_CheckedChanged(object sender, EventArgs e)
        {
            label8.Enabled = btnDeltaColor.Enabled = chkOverrideDeltaColor.Checked;
        }

        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label3.Enabled = label10.Enabled = label13.Enabled = btnBeforeNamesColor.Enabled
            = btnCurrentNamesColor.Enabled = btnAfterNamesColor.Enabled = chkOverrideTextColor.Checked;
        }
        void cmbComparison_SelectedIndexChanged(object sender, EventArgs e)
        {
            Comparison = cmbComparison.SelectedItem.ToString();
        }

        void rdoDeltaTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDeltaAccuracy();
        }

        void rdoDeltaSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDeltaAccuracy();
        }

        void chkSeparatorLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            SeparatorLastSplit = chkSeparatorLastSplit.Checked;
            SplitLayoutChanged(this, null);
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        void cmbSplitGradient_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnTopColor.Visible = cmbSplitGradient.SelectedItem.ToString() != "Plain";
            btnBottomColor.DataBindings.Clear();
            btnBottomColor.DataBindings.Add("BackColor", this, btnTopColor.Visible ? "CurrentSplitBottomColor" : "CurrentSplitTopCOlor", false, DataSourceUpdateMode.OnPropertyChanged);
            SplitGradientString = cmbSplitGradient.SelectedItem.ToString();
        }

        void chkLockLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            LockLastSplit = chkLockLastSplit.Checked;
            SplitLayoutChanged(this, null);
        }

        void chkShowBlankSplits_CheckedChanged(object sender, EventArgs e)
        {
            ShowBlankSplits = chkLockLastSplit.Enabled = chkShowBlankSplits.Checked;
            SplitLayoutChanged(this, null);
        }

        void rdoTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void UpdateSubsplitVisibility()
        {
            if (rdoShowSubsplits.Checked)
            {
                ShowSubsplits = true;
                HideSubsplits = false;
                CurrentSectionOnly = false;
                chkCurrentSectionOnly.Enabled = false;
            }
            else if (rdoHideSubsplits.Checked)
            {
                ShowSubsplits = false;
                HideSubsplits = true;
                CurrentSectionOnly = chkCurrentSectionOnly.Checked;
                chkCurrentSectionOnly.Enabled = true;
            }
            else
            {
                ShowSubsplits = false;
                HideSubsplits = false;
                CurrentSectionOnly = chkCurrentSectionOnly.Checked;
                chkCurrentSectionOnly.Enabled = true;
            }
        }

        void UpdateAccuracy()
        {
            if (rdoSeconds.Checked)
                SplitTimesAccuracy = TimeAccuracy.Seconds;
            else if (rdoTenths.Checked)
                SplitTimesAccuracy = TimeAccuracy.Tenths;
            else
                SplitTimesAccuracy = TimeAccuracy.Hundredths;
        }

        void UpdateDeltaAccuracy()
        {
            if (rdoDeltaSeconds.Checked)
                DeltasAccuracy = TimeAccuracy.Seconds;
            else if (rdoDeltaTenths.Checked)
                DeltasAccuracy = TimeAccuracy.Tenths;
            else
                DeltasAccuracy = TimeAccuracy.Hundredths;
        }

        void chkLastSplit_CheckedChanged(object sender, EventArgs e)
        {
            AlwaysShowLastSplit = chkLastSplit.Checked;
            VisualSplitCount = VisualSplitCount;
            SplitLayoutChanged(this, null);
        }

        void chkThinSeparators_CheckedChanged(object sender, EventArgs e)
        {
            ShowThinSeparators = chkThinSeparators.Checked;
            SplitLayoutChanged(this, null);
        }

        void SplitsSettings_Load(object sender, EventArgs e)
        {
            chkOverrideDeltaColor_CheckedChanged(null, null);
            chkOverrideTextColor_CheckedChanged(null, null);
            chkOverrideTimesColor_CheckedChanged(null, null);
            chkOverrideSubsplitColor_CheckedChanged(null, null);
            chkShowHeader_CheckedChanged(null, null);
            chkOverrideHeaderColor_CheckedChanged(null, null);
            chkSectionTimer_CheckedChanged(null, null);
            chkDisplayIcons_CheckedChanged(null, null);

            chkLockLastSplit.Enabled = chkShowBlankSplits.Checked;
            cmbComparison.Items.Clear();
            cmbComparison.Items.Add("Current Comparison");
            cmbComparison.Items.AddRange(CurrentState.Run.Comparisons.Where(x => x != NoneComparisonGenerator.ComparisonName).ToArray());
            if (!cmbComparison.Items.Contains(Comparison))
                cmbComparison.Items.Add(Comparison);

            rdoHideSubsplits.Checked = !ShowSubsplits && HideSubsplits;
            rdoShowSubsplits.Checked = ShowSubsplits && !HideSubsplits;
            rdoNormalSubsplits.Checked = !ShowSubsplits && !HideSubsplits;

            rdoSeconds.Checked = SplitTimesAccuracy == TimeAccuracy.Seconds;
            rdoTenths.Checked = SplitTimesAccuracy == TimeAccuracy.Tenths;
            rdoHundredths.Checked = SplitTimesAccuracy == TimeAccuracy.Hundredths;

            rdoDeltaSeconds.Checked = DeltasAccuracy == TimeAccuracy.Seconds;
            rdoDeltaTenths.Checked = DeltasAccuracy == TimeAccuracy.Tenths;
            rdoDeltaHundredths.Checked = DeltasAccuracy == TimeAccuracy.Hundredths;

            rdoHeaderAccuracySeconds.Checked = HeaderAccuracy == TimeAccuracy.Seconds;
            rdoHeaderAccuracyTenths.Checked = HeaderAccuracy == TimeAccuracy.Tenths;
            rdoHeaderAccuracyHundreths.Checked = HeaderAccuracy == TimeAccuracy.Hundredths;

            rdoSectionTimerAccuracySeconds.Checked = SectionTimerAccuracy == TimeAccuracy.Seconds;
            rdoSectionTimerAccuracyTenths.Checked = SectionTimerAccuracy == TimeAccuracy.Tenths;
            rdoSectionTimerAccuracyHundreths.Checked = SectionTimerAccuracy == TimeAccuracy.Hundredths;

            if (Mode == LayoutMode.Horizontal)
            {
                trkSize.DataBindings.Clear();
                trkSize.Minimum = 5;
                trkSize.Maximum = 120;
                SplitWidth = Math.Min(Math.Max(trkSize.Minimum, SplitWidth), trkSize.Maximum);
                trkSize.DataBindings.Add("Value", this, "SplitWidth", false, DataSourceUpdateMode.OnPropertyChanged);
                lblSplitSize.Text = "Split Width:";
                chkDisplayRows.Enabled = false;
                chkDisplayRows.DataBindings.Clear();
                chkDisplayRows.Checked = true;
            }
            else
            {
                trkSize.DataBindings.Clear();
                trkSize.Minimum = 0;
                trkSize.Maximum = 250;
                ScaledSplitHeight = Math.Min(Math.Max(trkSize.Minimum, ScaledSplitHeight), trkSize.Maximum);
                trkSize.DataBindings.Add("Value", this, "ScaledSplitHeight", false, DataSourceUpdateMode.OnPropertyChanged);
                lblSplitSize.Text = "Split Height:";
                chkDisplayRows.Enabled = true;
                chkDisplayRows.DataBindings.Clear();
                chkDisplayRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version = SettingsHelper.ParseVersion(element["Version"]);

            CurrentSplitTopColor = SettingsHelper.ParseColor(element["CurrentSplitTopColor"], Color.FromArgb(51, 115, 244));
            CurrentSplitBottomColor = SettingsHelper.ParseColor(element["CurrentSplitBottomColor"], Color.FromArgb(21, 53, 116));
            VisualSplitCount = SettingsHelper.ParseInt(element["VisualSplitCount"], 8);
            SplitPreviewCount = SettingsHelper.ParseInt(element["SplitPreviewCount"], 1);
            DisplayIcons = SettingsHelper.ParseBool(element["DisplayIcons"], true);
            ShowThinSeparators = SettingsHelper.ParseBool(element["ShowThinSeparators"], false);
            AlwaysShowLastSplit = SettingsHelper.ParseBool(element["AlwaysShowLastSplit"], true);
            SplitWidth = SettingsHelper.ParseFloat(element["SplitWidth"], 20);
            HideBlankIcons = SettingsHelper.ParseBool(element["HideBlankIcons"], true);
            IndentSubsplits = SettingsHelper.ParseBool(element["IndentSubsplits"], true);
            HideSubsplits = SettingsHelper.ParseBool(element["HideSubsplits"], false);
            ShowSubsplits = SettingsHelper.ParseBool(element["ShowSubsplits"], false);
            CurrentSectionOnly = SettingsHelper.ParseBool(element["CurrentSectionOnly"], false);
            OverrideSubsplitColor = SettingsHelper.ParseBool(element["OverrideSubsplitColor"], false);
            SubsplitTopColor = SettingsHelper.ParseColor(element["SubsplitTopColor"], Color.FromArgb(0x8D, 0x00, 0x00, 0x00));
            SubsplitBottomColor = SettingsHelper.ParseColor(element["SubsplitBottomColor"], Color.Transparent);
            SubsplitGradientString = SettingsHelper.ParseString(element["SubsplitGradient"], GradientType.Plain.ToString());
            ShowHeader = SettingsHelper.ParseBool(element["ShowHeader"], true);
            IndentSectionSplit = SettingsHelper.ParseBool(element["IndentSectionSplit"], true);
            ShowIconSectionSplit = SettingsHelper.ParseBool(element["ShowIconSectionSplit"], true);
            ShowSectionIcon = SettingsHelper.ParseBool(element["ShowSectionIcon"], true);
            HeaderTopColor = SettingsHelper.ParseColor(element["HeaderTopColor"], Color.FromArgb(0x2B, 0xFF, 0xFF, 0xFF));
            HeaderBottomColor = SettingsHelper.ParseColor(element["HeaderBottomColor"], Color.FromArgb(0xD8, 0x00, 0x00, 0x00));
            HeaderGradientString = SettingsHelper.ParseString(element["HeaderGradient"], GradientType.Vertical.ToString());
            OverrideHeaderColor = SettingsHelper.ParseBool(element["OverrideHeaderColor"], false);
            HeaderTextColor = SettingsHelper.ParseColor(element["HeaderTextColor"], Color.FromArgb(255, 255, 255));
            HeaderText = SettingsHelper.ParseBool(element["HeaderText"], true);
            HeaderTimesColor = SettingsHelper.ParseColor(element["HeaderTimesColor"], Color.FromArgb(255, 255, 255));
            HeaderTimes = SettingsHelper.ParseBool(element["HeaderTimes"], true);
            HeaderAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["HeaderAccuracy"], TimeAccuracy.Tenths);
            SectionTimer = SettingsHelper.ParseBool(element["SectionTimer"], true);
            SectionTimerColor = SettingsHelper.ParseColor(element["SectionTimerColor"], Color.FromArgb(0x77, 0x77, 0x77));
            SectionTimerGradient = SettingsHelper.ParseBool(element["SectionTimerGradient"], true);
            SectionTimerAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["SectionTimerAccuracy"], TimeAccuracy.Tenths);
            OverrideTimesColor = SettingsHelper.ParseBool(element["OverrideTimesColor"], false);
            BeforeTimesColor = SettingsHelper.ParseColor(element["BeforeTimesColor"], Color.FromArgb(255, 255, 255));
            CurrentTimesColor = SettingsHelper.ParseColor(element["CurrentTimesColor"], Color.FromArgb(255, 255, 255));
            AfterTimesColor = SettingsHelper.ParseColor(element["AfterTimesColor"], Color.FromArgb(255, 255, 255));
            SplitHeight = SettingsHelper.ParseFloat(element["SplitHeight"], 3.6f);
            SplitGradientString = SettingsHelper.ParseString(element["CurrentSplitGradient"], GradientType.Vertical.ToString());
            BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"], Color.Transparent);
            BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"], Color.FromArgb(1, 255, 255, 255));
            GradientString = SettingsHelper.ParseString(element["BackgroundGradient"], ExtendedGradientType.Alternating.ToString());
            SeparatorLastSplit = SettingsHelper.ParseBool(element["SeparatorLastSplit"], true);
            DropDecimals = SettingsHelper.ParseBool(element["DropDecimals"], true);
            DeltasAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["DeltasAccuracy"], TimeAccuracy.Tenths);
            OverrideDeltasColor = SettingsHelper.ParseBool(element["OverrideDeltasColor"], false);
            DeltasColor = SettingsHelper.ParseColor(element["DeltasColor"], Color.FromArgb(255, 255, 255));
            Comparison = SettingsHelper.ParseString(element["Comparison"], "Current Comparison");
            Display2Rows = SettingsHelper.ParseBool(element["Display2Rows"], false);
            ShowSplitTimes = SettingsHelper.ParseBool(element["ShowSplitTimes"], false);
            SplitTimesAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["SplitTimesAccuracy"], TimeAccuracy.Seconds);
            ShowBlankSplits = SettingsHelper.ParseBool(element["ShowBlankSplits"], true);
            LockLastSplit = SettingsHelper.ParseBool(element["LockLastSplit"], true);
            IconSize = SettingsHelper.ParseFloat(element["IconSize"], 24f);
            IconShadows = SettingsHelper.ParseBool(element["IconShadows"], true);

            if (version >= new Version(1, 3))
            {
                BeforeNamesColor = SettingsHelper.ParseColor(element["BeforeNamesColor"]);
                CurrentNamesColor = SettingsHelper.ParseColor(element["CurrentNamesColor"]);
                AfterNamesColor = SettingsHelper.ParseColor(element["AfterNamesColor"]);
                OverrideTextColor = SettingsHelper.ParseBool(element["OverrideTextColor"]);
            }
            else
            {
                if (version >= new Version(1, 2))
                    BeforeNamesColor = CurrentNamesColor = AfterNamesColor = SettingsHelper.ParseColor(element["SplitNamesColor"]);
                else
                {
                    BeforeNamesColor = Color.FromArgb(255, 255, 255);
                    CurrentNamesColor = Color.FromArgb(255, 255, 255);
                    AfterNamesColor = Color.FromArgb(255, 255, 255);
                }
                OverrideTextColor = !SettingsHelper.ParseBool(element["UseTextColor"], true);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(SettingsHelper.ToElement(document, "Version", "1.6.0"));
            parent.AppendChild(SettingsHelper.ToElement(document, CurrentSplitTopColor, "CurrentSplitTopColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, CurrentSplitBottomColor, "CurrentSplitBottomColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, "VisualSplitCount", VisualSplitCount));
            parent.AppendChild(SettingsHelper.ToElement(document, "SplitPreviewCount", SplitPreviewCount));
            parent.AppendChild(SettingsHelper.ToElement(document, "DisplayIcons", DisplayIcons));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowThinSeparators", ShowThinSeparators));
            parent.AppendChild(SettingsHelper.ToElement(document, "AlwaysShowLastSplit", AlwaysShowLastSplit));
            parent.AppendChild(SettingsHelper.ToElement(document, "SplitWidth", SplitWidth));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowSplitTimes", ShowSplitTimes));
            parent.AppendChild(SettingsHelper.ToElement(document, "SplitTimesAccuracy", SplitTimesAccuracy));
            parent.AppendChild(SettingsHelper.ToElement(document, BeforeNamesColor, "BeforeNamesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, CurrentNamesColor, "CurrentNamesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, AfterNamesColor, "AfterNamesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(SettingsHelper.ToElement(document, BeforeTimesColor, "BeforeTimesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, CurrentTimesColor, "CurrentTimesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, AfterTimesColor, "AfterTimesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideTimesColor", OverrideTimesColor));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowBlankSplits", ShowBlankSplits));
            parent.AppendChild(SettingsHelper.ToElement(document, "LockLastSplit", LockLastSplit));
            parent.AppendChild(SettingsHelper.ToElement(document, "IconSize", IconSize));
            parent.AppendChild(SettingsHelper.ToElement(document, "IconShadows", IconShadows));
            parent.AppendChild(SettingsHelper.ToElement(document, "SplitHeight", SplitHeight));
            parent.AppendChild(SettingsHelper.ToElement(document, "CurrentSplitGradient", CurrentSplitGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(SettingsHelper.ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, "SeparatorLastSplit", SeparatorLastSplit));
            parent.AppendChild(SettingsHelper.ToElement(document, "DeltasAccuracy", DeltasAccuracy));
            parent.AppendChild(SettingsHelper.ToElement(document, "DropDecimals", DropDecimals));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideDeltasColor", OverrideDeltasColor));
            parent.AppendChild(SettingsHelper.ToElement(document, DeltasColor, "DeltasColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, "Comparison", Comparison));
            parent.AppendChild(SettingsHelper.ToElement(document, "Display2Rows", Display2Rows));
            parent.AppendChild(SettingsHelper.ToElement(document, "HideBlankIcons", HideBlankIcons));
            parent.AppendChild(SettingsHelper.ToElement(document, "IndentSubsplits", IndentSubsplits));
            parent.AppendChild(SettingsHelper.ToElement(document, "HideSubsplits", HideSubsplits));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowSubsplits", ShowSubsplits));
            parent.AppendChild(SettingsHelper.ToElement(document, "CurrentSectionOnly", CurrentSectionOnly));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideSubsplitColor", OverrideSubsplitColor));
            parent.AppendChild(SettingsHelper.ToElement(document, "SubsplitGradient", SubsplitGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowHeader", ShowHeader));
            parent.AppendChild(SettingsHelper.ToElement(document, "IndentSectionSplit", IndentSectionSplit));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowIconSectionSplit", ShowIconSectionSplit));
            parent.AppendChild(SettingsHelper.ToElement(document, "ShowSectionIcon", ShowSectionIcon));
            parent.AppendChild(SettingsHelper.ToElement(document, "HeaderGradient", HeaderGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, "OverrideHeaderColor", OverrideHeaderColor));
            parent.AppendChild(SettingsHelper.ToElement(document, "HeaderText", HeaderText));
            parent.AppendChild(SettingsHelper.ToElement(document, "HeaderTimes", HeaderTimes));
            parent.AppendChild(SettingsHelper.ToElement(document, "HeaderAccuracy", HeaderAccuracy));
            parent.AppendChild(SettingsHelper.ToElement(document, "SectionTimer", SectionTimer));
            parent.AppendChild(SettingsHelper.ToElement(document, "SectionTimerGradient", SectionTimerGradient));
            parent.AppendChild(SettingsHelper.ToElement(document, "SectionTimerAccuracy", SectionTimerAccuracy));
            parent.AppendChild(SettingsHelper.ToElement(document, SubsplitTopColor, "SubsplitTopColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, SubsplitBottomColor, "SubsplitBottomColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, HeaderTopColor, "HeaderTopColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, HeaderBottomColor, "HeaderBottomColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, HeaderTextColor, "HeaderTextColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, HeaderTimesColor, "HeaderTimesColor"));
            parent.AppendChild(SettingsHelper.ToElement(document, SectionTimerColor, "SectionTimerColor"));
            return parent;
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            SettingsHelper.ColorButtonClick((Button)sender, this);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubsplitVisibility();
        }

        private void rdoNormalSubsplits_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubsplitVisibility();
        }

        private void rdoShowSubsplits_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSubsplitVisibility();
        }

        private void chkOverrideSubsplitColor_CheckedChanged(object sender, EventArgs e)
        {
            label15.Enabled = btnSubsplitTopColor.Enabled = btnSubsplitBottomColor.Enabled = cmbSubsplitGradient.Enabled = chkOverrideSubsplitColor.Checked;
        }

        private void cmbSubsplitGradient_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSubsplitTopColor.Visible = cmbSubsplitGradient.SelectedItem.ToString() != "Plain";
            btnSubsplitBottomColor.DataBindings.Clear();
            btnSubsplitBottomColor.DataBindings.Add("BackColor", this, btnSubsplitTopColor.Visible ? "SubsplitBottomColor" : "SubsplitTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            SubsplitGradientString = cmbSubsplitGradient.SelectedItem.ToString();
        }

        private void chkShowHeader_CheckedChanged(object sender, EventArgs e)
        {
            chkShowSectionIcon.Enabled = groupBox12.Enabled = groupBox13.Enabled = chkShowHeader.Checked;
        }

        private void cmbHeaderGradient_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnHeaderTopColor.Visible = cmbHeaderGradient.SelectedItem.ToString() != "Plain";
            btnHeaderBottomColor.DataBindings.Clear();
            btnHeaderBottomColor.DataBindings.Add("BackColor", this, btnHeaderTopColor.Visible ? "HeaderBottomColor" : "HeaderTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            HeaderGradientString = cmbHeaderGradient.SelectedItem.ToString();
        }

        private void chkOverrideHeaderColor_CheckedChanged(object sender, EventArgs e)
        {
            label17.Enabled = btnHeaderTextColor.Enabled = label18.Enabled = btnHeaderTimesColor.Enabled = chkOverrideHeaderColor.Checked;
        }

        void UpdateHeaderAccuracy()
        {
            if (rdoHeaderAccuracySeconds.Checked)
                HeaderAccuracy = TimeAccuracy.Seconds;
            else if (rdoHeaderAccuracyTenths.Checked)
                HeaderAccuracy = TimeAccuracy.Tenths;
            else
                HeaderAccuracy = TimeAccuracy.Hundredths;
        }

        private void rdoHeaderAccuracySeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateHeaderAccuracy();
        }

        private void rdoHeaderAccuracyTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateHeaderAccuracy();
        }

        private void rdoHeaderAccuracyHundreths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateHeaderAccuracy();
        }

        private void chkSectionTimer_CheckedChanged(object sender, EventArgs e)
        {
            label19.Enabled = btnSectionTimerColor.Enabled = chkSectionTimerGradient.Enabled = groupBox14.Enabled = chkSectionTimer.Checked;
        }

        void UpdateSectionTimerAccuracy()
        {
            if (rdoSectionTimerAccuracySeconds.Checked)
            {
                SectionTimerAccuracy = TimeAccuracy.Seconds;
            }
            else if (rdoSectionTimerAccuracyTenths.Checked)
                SectionTimerAccuracy = TimeAccuracy.Tenths;
            else
                SectionTimerAccuracy = TimeAccuracy.Hundredths;
        }

        private void rdoSectionTimerAccuracySeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSectionTimerAccuracy();
        }

        private void rdoSectionTimerAccuracyTenths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSectionTimerAccuracy();
        }

        private void rdoSectionTimerAccuracyHundreths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSectionTimerAccuracy();
        }
    }
}
