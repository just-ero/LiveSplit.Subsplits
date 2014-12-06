using Fetze.WinFormsColor;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            VisualSplitCount = 16;
            SplitPreviewCount = 1;
            DisplayIcons = false;
            IconShadows = true;
            ShowThinSeparators = true;
            AlwaysShowLastSplit = true;
            ShowSplitTimes = false;
            ShowBlankSplits = true;
            LockLastSplit = true;
            SeparatorLastSplit = true;
            SplitTimesAccuracy = TimeAccuracy.Seconds;
            CurrentSplitTopColor = Color.FromArgb(0x00, 0xA2, 0xFF);
            CurrentSplitBottomColor = Color.FromArgb(0x07, 0x44, 0x67);
            SplitWidth = 20;
            SplitHeight = 6;
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
            BackgroundColor = Color.FromArgb(0x00, 0x03, 0x83);
            BackgroundColor2 = Color.FromArgb(0x1D, 0x20, 0x9C);
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
            OverrideSubsplitColor = true;
            SubsplitTopColor = Color.FromArgb(0x8D, 0x00, 0x00, 0x00);
            SubsplitBottomColor = Color.Transparent;
            SubsplitGradient = GradientType.Plain;
            ShowHeader = true;
            IndentSectionSplit = true; 
            ShowSectionIcon = true;
            HeaderTopColor = Color.FromArgb(0x2B, 0xFF, 0xFF, 0xFF);
            HeaderBottomColor = Color.FromArgb(0xD8, 0x00, 0x00, 0x00);
            HeaderGradient = GradientType.Vertical;
            OverrideHeaderColor = false;
            HeaderTextColor = Color.FromArgb(255, 255, 255);
            HeaderText = false;
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
            cmbSubsplitGradient.SelectedIndexChanged += cmbSubsplitGradient_SelectedIndexChanged;
            cmbSubsplitGradient.DataBindings.Add("SelectedItem", this, "SubsplitGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSubsplitTopColor.DataBindings.Add("BackColor", this, "SubsplitTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSubsplitBottomColor.DataBindings.Add("BackColor", this, "SubsplitBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);

            chkShowHeader.DataBindings.Add("Checked", this, "ShowHeader", false, DataSourceUpdateMode.OnPropertyChanged);
            chkIndentSectionSplit.DataBindings.Add("Checked", this, "IndentSectionSplit", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowSectionIcon.DataBindings.Add("Checked", this, "ShowSectionIcon", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTopColor.DataBindings.Add("BackColor", this, "HeaderTopColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderBottomColor.DataBindings.Add("BackColor", this, "HeaderBottomColor", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbHeaderGradient.SelectedIndexChanged += cmbHeaderGradient_SelectedIndexChanged;
            cmbHeaderGradient.DataBindings.Add("SelectedItem", this, "HeaderGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideHeaderColor.DataBindings.Add("Checked", this, "OverrideHeaderColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTextColor.DataBindings.Add("BackColor", this, "HeaderTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkHeaderText.DataBindings.Add("Checked", this, "HeaderText", false, DataSourceUpdateMode.OnPropertyChanged);
            btnHeaderTimesColor.DataBindings.Add("BackColor", this, "HeaderTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkHeaderTimes.DataBindings.Add("Checked", this, "HeaderTimes", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSectionTimer.DataBindings.Add("Checked", this, "SectionTimer", false, DataSourceUpdateMode.OnPropertyChanged);
            btnSectionTimerColor.DataBindings.Add("BackColor", this, "SectionTimerColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkSectionTimerGradient.DataBindings.Add("Checked", this, "SectionTimerGradient", false, DataSourceUpdateMode.OnPropertyChanged);

            this.Load += SplitsSettings_Load;
            chkThinSeparators.CheckedChanged += chkThinSeparators_CheckedChanged;
            chkLastSplit.CheckedChanged += chkLastSplit_CheckedChanged;
            chkShowBlankSplits.CheckedChanged += chkShowBlankSplits_CheckedChanged;
            chkLockLastSplit.CheckedChanged += chkLockLastSplit_CheckedChanged;
            chkSeparatorLastSplit.CheckedChanged += chkSeparatorLastSplit_CheckedChanged;
            trkIconSize.DataBindings.Add("Value", this, "IconSize", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbSplitGradient.DataBindings.Add("SelectedItem", this, "SplitGradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbSplitGradient.SelectedIndexChanged += cmbSplitGradient_SelectedIndexChanged;
            cmbComparison.SelectedIndexChanged += cmbComparison_SelectedIndexChanged;
            cmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);

            cmbGradientType.SelectedIndexChanged += cmbGradientType_SelectedIndexChanged;
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);

            rdoSeconds.CheckedChanged += rdoSeconds_CheckedChanged;
            rdoTenths.CheckedChanged += rdoTenths_CheckedChanged;

            rdoDeltaSeconds.CheckedChanged += rdoDeltaSeconds_CheckedChanged;
            rdoDeltaTenths.CheckedChanged += rdoDeltaTenths_CheckedChanged;

            chkOverrideTextColor.CheckedChanged += chkOverrideTextColor_CheckedChanged;
            chkOverrideDeltaColor.CheckedChanged += chkOverrideDeltaColor_CheckedChanged;
            chkOverrideTimesColor.CheckedChanged += chkOverrideTimesColor_CheckedChanged;
            chkDisplayIcons.CheckedChanged += chkDisplayIcons_CheckedChanged;
            chkOverrideSubsplitColor.CheckedChanged += chkOverrideSubsplitColor_CheckedChanged;
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
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0, 0, 0);
            CurrentSplitTopColor = ParseColor(element["CurrentSplitTopColor"]);
            CurrentSplitBottomColor = ParseColor(element["CurrentSplitBottomColor"]);
            VisualSplitCount = Int32.Parse(element["VisualSplitCount"].InnerText);
            SplitPreviewCount = Int32.Parse(element["SplitPreviewCount"].InnerText);
            DisplayIcons = Boolean.Parse(element["DisplayIcons"].InnerText);
            ShowThinSeparators = Boolean.Parse(element["ShowThinSeparators"].InnerText);
            AlwaysShowLastSplit = Boolean.Parse(element["AlwaysShowLastSplit"].InnerText);
            SplitWidth = Single.Parse(element["SplitWidth"].InnerText.Replace(',', '.'), CultureInfo.InvariantCulture);
            if (version >= new Version(1, 4, 1))
            {
                HideBlankIcons = Boolean.Parse(element["HideBlankIcons"].InnerText);
                IndentSubsplits = Boolean.Parse(element["IndentSubsplits"].InnerText);
                HideSubsplits = Boolean.Parse(element["HideSubsplits"].InnerText);
                ShowSubsplits = Boolean.Parse(element["ShowSubsplits"].InnerText);
                CurrentSectionOnly = Boolean.Parse(element["CurrentSectionOnly"].InnerText);
                OverrideSubsplitColor = Boolean.Parse(element["OverrideSubsplitColor"].InnerText);
                SubsplitTopColor = ParseColor(element["SubsplitTopColor"]);
                SubsplitBottomColor = ParseColor(element["SubsplitBottomColor"]);
                SubsplitGradientString = element["SubsplitGradient"].InnerText;
                ShowHeader = Boolean.Parse(element["ShowHeader"].InnerText);
                IndentSectionSplit = Boolean.Parse(element["IndentSectionSplit"].InnerText);
                ShowSectionIcon = Boolean.Parse(element["ShowSectionIcon"].InnerText);
                HeaderTopColor = ParseColor(element["HeaderTopColor"]);
                HeaderBottomColor = ParseColor(element["HeaderBottomColor"]);
                HeaderGradientString = element["HeaderGradient"].InnerText;
                OverrideHeaderColor = Boolean.Parse(element["OverrideHeaderColor"].InnerText);
                HeaderTextColor = ParseColor(element["HeaderTextColor"]);
                HeaderText = Boolean.Parse(element["HeaderText"].InnerText);
                HeaderTimesColor = ParseColor(element["HeaderTimesColor"]);
                HeaderTimes = Boolean.Parse(element["HeaderTimes"].InnerText);
                HeaderAccuracy = ParseEnum<TimeAccuracy>(element["HeaderAccuracy"]);
                SectionTimer = Boolean.Parse(element["SectionTimer"].InnerText);
                SectionTimerColor = ParseColor(element["SectionTimerColor"]);
                SectionTimerGradient = Boolean.Parse(element["SectionTimerGradient"].InnerText);
                SectionTimerAccuracy = ParseEnum<TimeAccuracy>(element["SectionTimerAccuracy"]);
            }
            else
            {
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
                ShowSectionIcon = true;
                HeaderTopColor = Color.FromArgb(0x2B, 0xFF, 0xFF, 0xFF);
                HeaderBottomColor = Color.FromArgb(0xD8, 0x00, 0x00, 0x00);
                HeaderGradient = GradientType.Vertical;
                OverrideHeaderColor = false;
                HeaderTextColor = Color.FromArgb(255, 255, 255);
                HeaderText = false;
                HeaderTimesColor = Color.FromArgb(255, 255, 255);
                HeaderTimes = true;
                HeaderAccuracy = TimeAccuracy.Tenths;
                SectionTimer = true;
                SectionTimerColor = Color.FromArgb(0x77, 0x77, 0x77);
                SectionTimerGradient = true;
                SectionTimerAccuracy = TimeAccuracy.Tenths;
            }

            if (version >= new Version(1, 3))
            {
                OverrideTimesColor = Boolean.Parse(element["OverrideTimesColor"].InnerText);
                BeforeTimesColor = ParseColor(element["BeforeTimesColor"]);
                CurrentTimesColor = ParseColor(element["CurrentTimesColor"]);
                AfterTimesColor = ParseColor(element["AfterTimesColor"]);
                BeforeNamesColor = ParseColor(element["BeforeNamesColor"]);
                CurrentNamesColor = ParseColor(element["CurrentNamesColor"]);
                AfterNamesColor = ParseColor(element["AfterNamesColor"]);
                SplitHeight = Single.Parse(element["SplitHeight"].InnerText.Replace(',', '.'), CultureInfo.InvariantCulture);
                SplitGradientString = element["CurrentSplitGradient"].InnerText;
                BackgroundColor = ParseColor(element["BackgroundColor"]);
                BackgroundColor2 = ParseColor(element["BackgroundColor2"]);
                GradientString = element["BackgroundGradient"].InnerText;
                SeparatorLastSplit = Boolean.Parse(element["SeparatorLastSplit"].InnerText);
                DropDecimals = Boolean.Parse(element["DropDecimals"].InnerText);
                DeltasAccuracy = ParseEnum<TimeAccuracy>(element["DeltasAccuracy"]);
                OverrideDeltasColor = Boolean.Parse(element["OverrideDeltasColor"].InnerText);
                DeltasColor = ParseColor(element["DeltasColor"]);
                Comparison = element["Comparison"].InnerText;
                Display2Rows = Boolean.Parse(element["Display2Rows"].InnerText);
            }
            else
            {
                if (version >= new Version(1, 2))
                    BeforeNamesColor = CurrentNamesColor = AfterNamesColor = ParseColor(element["SplitNamesColor"]);
                else
                {
                    BeforeNamesColor = Color.FromArgb(255, 255, 255);
                    CurrentNamesColor = Color.FromArgb(255, 255, 255);
                    AfterNamesColor = Color.FromArgb(255, 255, 255);
                }
                BeforeTimesColor = Color.FromArgb(255, 255, 255);
                CurrentTimesColor = Color.FromArgb(255, 255, 255);
                AfterTimesColor = Color.FromArgb(255, 255, 255);
                OverrideTimesColor = false;
                SplitHeight = 6;
                CurrentSplitGradient = GradientType.Vertical;
                BackgroundColor = Color.FromArgb(0x00, 0x03, 0x83);
                BackgroundColor2 = Color.FromArgb(0x1D, 0x20, 0x9C);
                BackgroundGradient = ExtendedGradientType.Alternating;
                SeparatorLastSplit = true;
                DropDecimals = true;
                DeltasAccuracy = TimeAccuracy.Tenths;
                OverrideDeltasColor = false;
                DeltasColor = Color.FromArgb(255, 255, 255);
                Comparison = "Current Comparison";
                Display2Rows = false;
            }              
            if (version >= new Version(1, 2))
            {
                ShowSplitTimes = Boolean.Parse(element["ShowSplitTimes"].InnerText);
                SplitTimesAccuracy = ParseEnum<TimeAccuracy>(element["SplitTimesAccuracy"]);
                if (version >= new Version(1, 3))
                    OverrideTextColor = Boolean.Parse(element["OverrideTextColor"].InnerText);
                else
                    OverrideTextColor = !Boolean.Parse(element["UseTextColor"].InnerText);
                ShowBlankSplits = Boolean.Parse(element["ShowBlankSplits"].InnerText);
                LockLastSplit = Boolean.Parse(element["LockLastSplit"].InnerText);
                IconSize = Single.Parse(element["IconSize"].InnerText.Replace(',', '.'), CultureInfo.InvariantCulture);
                IconShadows = Boolean.Parse(element["IconShadows"].InnerText);
            }
            else
            {
                ShowSplitTimes = false;
                SplitTimesAccuracy = TimeAccuracy.Seconds;
                OverrideTextColor = false;
                ShowBlankSplits = true;
                LockLastSplit = false;
                IconSize = 24f;
                IconShadows = true;
            }
                
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.4.1"));
            parent.AppendChild(ToElement(document, CurrentSplitTopColor, "CurrentSplitTopColor"));
            parent.AppendChild(ToElement(document, CurrentSplitBottomColor, "CurrentSplitBottomColor"));
            parent.AppendChild(ToElement(document, "VisualSplitCount", VisualSplitCount));
            parent.AppendChild(ToElement(document, "SplitPreviewCount", SplitPreviewCount));
            parent.AppendChild(ToElement(document, "DisplayIcons", DisplayIcons));
            parent.AppendChild(ToElement(document, "ShowThinSeparators", ShowThinSeparators));
            parent.AppendChild(ToElement(document, "AlwaysShowLastSplit", AlwaysShowLastSplit));
            parent.AppendChild(ToElement(document, "SplitWidth", SplitWidth));
            parent.AppendChild(ToElement(document, "ShowSplitTimes", ShowSplitTimes));
            parent.AppendChild(ToElement(document, "SplitTimesAccuracy", SplitTimesAccuracy));
            parent.AppendChild(ToElement(document, BeforeNamesColor, "BeforeNamesColor"));
            parent.AppendChild(ToElement(document, CurrentNamesColor, "CurrentNamesColor"));
            parent.AppendChild(ToElement(document, AfterNamesColor, "AfterNamesColor"));
            parent.AppendChild(ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(ToElement(document, BeforeTimesColor, "BeforeTimesColor"));
            parent.AppendChild(ToElement(document, CurrentTimesColor, "CurrentTimesColor"));
            parent.AppendChild(ToElement(document, AfterTimesColor, "AfterTimesColor"));
            parent.AppendChild(ToElement(document, "OverrideTimesColor", OverrideTimesColor));
            parent.AppendChild(ToElement(document, "ShowBlankSplits", ShowBlankSplits));
            parent.AppendChild(ToElement(document, "LockLastSplit", LockLastSplit));
            parent.AppendChild(ToElement(document, "IconSize", IconSize));
            parent.AppendChild(ToElement(document, "IconShadows", IconShadows));
            parent.AppendChild(ToElement(document, "SplitHeight", SplitHeight));
            parent.AppendChild(ToElement(document, "CurrentSplitGradient", CurrentSplitGradient));
            parent.AppendChild(ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(ToElement(document, "SeparatorLastSplit", SeparatorLastSplit));
            parent.AppendChild(ToElement(document, "DeltasAccuracy", DeltasAccuracy));
            parent.AppendChild(ToElement(document, "DropDecimals", DropDecimals));
            parent.AppendChild(ToElement(document, "OverrideDeltasColor", OverrideDeltasColor));
            parent.AppendChild(ToElement(document, DeltasColor, "DeltasColor"));
            parent.AppendChild(ToElement(document, "Comparison", Comparison));
            parent.AppendChild(ToElement(document, "Display2Rows", Display2Rows));

            parent.AppendChild(ToElement(document, "HideBlankIcons", HideBlankIcons));
            parent.AppendChild(ToElement(document, "IndentSubsplits", IndentSubsplits));
            parent.AppendChild(ToElement(document, "HideSubsplits", HideSubsplits));
            parent.AppendChild(ToElement(document, "ShowSubsplits", ShowSubsplits));
            parent.AppendChild(ToElement(document, "CurrentSectionOnly", CurrentSectionOnly));
            parent.AppendChild(ToElement(document, "OverrideSubsplitColor", OverrideSubsplitColor));
            parent.AppendChild(ToElement(document, "SubsplitGradient", SubsplitGradient));
            parent.AppendChild(ToElement(document, "ShowHeader", ShowHeader));
            parent.AppendChild(ToElement(document, "IndentSectionSplit", IndentSectionSplit));
            parent.AppendChild(ToElement(document, "ShowSectionIcon", ShowSectionIcon));
            parent.AppendChild(ToElement(document, "HeaderGradient", HeaderGradient));
            parent.AppendChild(ToElement(document, "OverrideHeaderColor", OverrideHeaderColor));
            parent.AppendChild(ToElement(document, "HeaderText", HeaderText));
            parent.AppendChild(ToElement(document, "HeaderTimes", HeaderTimes));
            parent.AppendChild(ToElement(document, "HeaderAccuracy", HeaderAccuracy));
            parent.AppendChild(ToElement(document, "SectionTimer", SectionTimer));
            parent.AppendChild(ToElement(document, "SectionTimerGradient", SectionTimerGradient));
            parent.AppendChild(ToElement(document, "SectionTimerAccuracy", SectionTimerAccuracy));
            parent.AppendChild(ToElement(document, SubsplitTopColor, "SubsplitTopColor"));
            parent.AppendChild(ToElement(document, SubsplitBottomColor, "SubsplitBottomColor"));
            parent.AppendChild(ToElement(document, HeaderTopColor, "HeaderTopColor"));
            parent.AppendChild(ToElement(document, HeaderBottomColor, "HeaderBottomColor"));
            parent.AppendChild(ToElement(document, HeaderTextColor, "HeaderTextColor"));
            parent.AppendChild(ToElement(document, HeaderTimesColor, "HeaderTimesColor"));
            parent.AppendChild(ToElement(document, SectionTimerColor, "SectionTimerColor"));
            return parent;
        }

        private Color ParseColor(XmlElement colorElement)
        {
            return Color.FromArgb(Int32.Parse(colorElement.InnerText, NumberStyles.HexNumber));
        }

        private XmlElement ToElement(XmlDocument document, Color color, string name)
        {
            var element = document.CreateElement(name);
            element.InnerText = color.ToArgb().ToString("X8");
            return element;
        }

        private T ParseEnum<T>(XmlElement element)
        {
            return (T)Enum.Parse(typeof(T), element.InnerText);
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var picker = new ColorPickerDialog();
            picker.SelectedColor = picker.OldColor = button.BackColor;
            picker.SelectedColorChanged += (s, x) => button.BackColor = picker.SelectedColor;
            picker.ShowDialog(this);
            button.BackColor = picker.SelectedColor;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }

        private XmlElement ToElement(XmlDocument document, String name, float value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString(CultureInfo.InvariantCulture);
            return element;
        }

        private void chkOverrideDeltaColor_CheckedChanged_1(object sender, EventArgs e)
        {

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

        private void chkIndentSubsplits_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkShowHeader_CheckedChanged(object sender, EventArgs e)
        {
            IndentSectionSplit = chkIndentSectionSplit.Enabled = chkShowSectionIcon.Enabled = groupBox12.Enabled = groupBox13.Enabled = chkShowHeader.Checked;
        }

        private void chkIndentSectionSplit_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void chkShowSectionIcon_CheckedChanged(object sender, EventArgs e)
        {
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

        private void chkHeaderText_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void chkHeaderTimes_CheckedChanged(object sender, EventArgs e)
        {
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

        private void chkSectionTimerGradient_CheckedChanged(object sender, EventArgs e)
        {

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
