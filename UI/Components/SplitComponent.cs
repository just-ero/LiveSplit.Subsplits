using LiveSplit.Model;
using LiveSplit.Properties;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace LiveSplit.UI.Components
{
    public class SplitComponent : IComponent
    {
        public bool Header { get; set; }
        public int TopSplit { get; set; }
        public bool ForceIndent { get; set; }
        public bool CollapsedSplit { get; set; }
        public bool oddSplit { get; set; }

        public ISegment Split { get; set; }
        protected bool blankOut = false;

        protected SimpleLabel NameLabel { get; set; }
        protected SimpleLabel TimeLabel { get; set; }
        protected SimpleLabel MeasureTimeLabel { get; set; }
        protected SimpleLabel MeasureDeltaLabel { get; set; }
        protected SimpleLabel DeltaLabel { get; set; }
        public SplitsSettings Settings { get; set; }

        protected int FrameCount { get; set; }

        public GraphicsCache Cache { get; set; }
        protected bool NeedUpdateAll { get; set; }
        protected bool IsActive { get; set; }
        protected bool IsHighlight { get; set; }
        protected bool IsSubsplit { get; set; }

        protected TimeAccuracy CurrentAccuracy { get; set; }
        protected TimeAccuracy CurrentDeltaAccuracy { get; set; }
        protected TimeAccuracy CurrentHeaderTimesAccuracy { get; set; }
        protected TimeAccuracy CurrentSectionTimerAccuracy { get; set; }
        protected bool CurrentDropDecimals { get; set; }

        protected ITimeFormatter TimeFormatter { get; set; }
        protected ITimeFormatter DeltaTimeFormatter { get; set; }
        protected ITimeFormatter HeaderTimesFormatter { get; set; }
        protected ITimeFormatter SectionTimerFormatter { get; set; }

        protected int IconWidth { get { return ((!Header && DisplayIcon && Settings.ShowIconSectionSplit) || (Header && Settings.ShowSectionIcon)) ? (int)(Settings.IconSize + 7.5f) : 0; } }

        public bool DisplayIcon { get; set; }

        public Image ShadowImage { get; set; }

        protected Image OldImage { get; set; }

        public Image NoIconImage = Resources.DefaultSplitIcon.ToBitmap();
        public Image NoIconShadow = IconShadow.Generate(Resources.DefaultSplitIcon.ToBitmap(), Color.Black);

        public float PaddingTop { get { return 0f; } }
        public float PaddingLeft { get { return 0f; } }
        public float PaddingBottom { get { return 0f; } }
        public float PaddingRight { get { return 0f; } }

        private Regex SubsplitRegex = new Regex(@"^{(.+)}\s*(.+)$", RegexOptions.Compiled);

        public float VerticalHeight
        {
            get { return 25f + Settings.SplitHeight; }
        }

        public float MinimumWidth { get; set; }

        public float HorizontalWidth
        {
            get { return Settings.SplitWidth + (((!Header && Settings.ShowSplitTimes) || (Header && Settings.SectionTimer)) ? MeasureDeltaLabel.ActualWidth : 0) + MeasureTimeLabel.ActualWidth + IconWidth; }
        }

        public float MinimumHeight { get; set; }

        public IDictionary<string, Action> ContextMenuControls
        {
            get { return null; }
        }

        public SplitComponent(SplitsSettings settings)
        {
            NoIconShadow = IconShadow.Generate(NoIconImage, Color.Black);
            NameLabel = new SimpleLabel()
            {
                HorizontalAlignment = StringAlignment.Near,
                X = 8,
                Y = 3,
                Text = ""
            };
            TimeLabel = new SimpleLabel()
            {
                HorizontalAlignment = StringAlignment.Far,
                Y = 3,
                Text = ""
            };
            MeasureTimeLabel = new SimpleLabel();
            DeltaLabel = new SimpleLabel()
            {
                HorizontalAlignment = StringAlignment.Far,
                Y = 3,
                Text = ""
            };
            MeasureDeltaLabel = new SimpleLabel();
            Settings = settings;
            TimeFormatter = new RegularSplitTimeFormatter(Settings.SplitTimesAccuracy);
            DeltaTimeFormatter = new DeltaSplitTimeFormatter(Settings.DeltasAccuracy, Settings.DropDecimals);
            HeaderTimesFormatter = new RegularSplitTimeFormatter(Settings.HeaderAccuracy);
            SectionTimerFormatter = new RegularSplitTimeFormatter(Settings.SectionTimerAccuracy);
            MinimumHeight = 31;

            MeasureTimeLabel.Text = TimeFormatter.Format(new TimeSpan(9, 0, 0));
            NeedUpdateAll = true;
            IsActive = false;

            Cache = new GraphicsCache();
        }

        private void DrawGeneral(Graphics g, LiveSplitState state, float width, float height, LayoutMode mode, Region clipRegion)
        {
            if (NeedUpdateAll)
                UpdateAll(state);

            if (Settings.BackgroundGradient == ExtendedGradientType.Alternating)
                g.FillRectangle(new SolidBrush(
                    oddSplit
                    ? Settings.BackgroundColor
                    : Settings.BackgroundColor2
                    ), 0, 0, width, height);

            if ((IsSubsplit || ForceIndent) && (Settings.OverrideSubsplitColor))
            {
                var gradientBrush = new LinearGradientBrush(
                    new PointF(0, 0),
                    Settings.SubsplitGradient == GradientType.Horizontal
                    ? new PointF(width, 0)
                    : new PointF(0, height),
                    Settings.SubsplitTopColor,
                    Settings.SubsplitGradient == GradientType.Plain
                    ? Settings.SubsplitTopColor
                    : Settings.SubsplitBottomColor);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }

            MeasureTimeLabel.Text = TimeFormatter.Format(new TimeSpan(24, 0, 0));
            MeasureDeltaLabel.Text = DeltaTimeFormatter.Format(new TimeSpan(0, 24, 0, 0));

            MeasureTimeLabel.Font = state.LayoutSettings.TimesFont;
            MeasureTimeLabel.IsMonospaced = true;
            MeasureDeltaLabel.Font = state.LayoutSettings.TimesFont;
            MeasureDeltaLabel.IsMonospaced = true;

            MeasureTimeLabel.SetActualWidth(g);
            MeasureDeltaLabel.SetActualWidth(g);
            TimeLabel.SetActualWidth(g);
            DeltaLabel.SetActualWidth(g);

            NameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            TimeLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            DeltaLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            MinimumWidth = (Settings.ShowSplitTimes ? MeasureDeltaLabel.ActualWidth : 10) + MeasureTimeLabel.ActualWidth + IconWidth + 10;
            MinimumHeight = 0.85f * (g.MeasureString("A", state.LayoutSettings.TimesFont).Height + g.MeasureString("A", state.LayoutSettings.TextFont).Height);

            if (Settings.SplitTimesAccuracy != CurrentAccuracy)
            {
                TimeFormatter = new RegularSplitTimeFormatter(Settings.SplitTimesAccuracy);
                CurrentAccuracy = Settings.SplitTimesAccuracy;
            }
            if (Settings.DeltasAccuracy != CurrentDeltaAccuracy || Settings.DropDecimals != CurrentDropDecimals)
            {
                DeltaTimeFormatter = new DeltaSplitTimeFormatter(Settings.DeltasAccuracy, Settings.DropDecimals);
                CurrentDeltaAccuracy = Settings.DeltasAccuracy;
                CurrentDropDecimals = Settings.DropDecimals;
            }
            if (Settings.HeaderAccuracy != CurrentHeaderTimesAccuracy)
            {
                HeaderTimesFormatter = new RegularSplitTimeFormatter(Settings.HeaderAccuracy);
                CurrentHeaderTimesAccuracy = Settings.HeaderAccuracy;
            }
            if (Settings.SectionTimerAccuracy != CurrentSectionTimerAccuracy)
            {
                SectionTimerFormatter = new RegularSplitTimeFormatter(Settings.SectionTimerAccuracy);
                CurrentSectionTimerAccuracy = Settings.SectionTimerAccuracy;
            }

            if (Split != null)
            {

                if (mode == LayoutMode.Vertical)
                {
                    NameLabel.VerticalAlignment = StringAlignment.Center;
                    DeltaLabel.VerticalAlignment = StringAlignment.Center;
                    TimeLabel.VerticalAlignment = StringAlignment.Center;
                    NameLabel.Y = 0;
                    DeltaLabel.Y = 0;
                    TimeLabel.Y = 0;
                    NameLabel.Height = height;
                    DeltaLabel.Height = height;
                    TimeLabel.Height = height;
                }
                else
                {
                    NameLabel.VerticalAlignment = StringAlignment.Near;
                    DeltaLabel.VerticalAlignment = StringAlignment.Far;
                    TimeLabel.VerticalAlignment = StringAlignment.Far;
                    NameLabel.Y = 0;
                    DeltaLabel.Y = height - 50;
                    TimeLabel.Y = height - 50;
                    NameLabel.Height = 50;
                    DeltaLabel.Height = 50;
                    TimeLabel.Height = 50;
                }

                if (IsActive)
                {
                    var currentSplitBrush = new LinearGradientBrush(
                        new PointF(0, 0),
                        Settings.CurrentSplitGradient == GradientType.Horizontal
                        ? new PointF(width, 0)
                        : new PointF(0, height),
                        Settings.CurrentSplitTopColor,
                        Settings.CurrentSplitGradient == GradientType.Plain
                        ? Settings.CurrentSplitTopColor
                        : Settings.CurrentSplitBottomColor);
                    g.FillRectangle(currentSplitBrush, 0, 0, width, height);
                }

                if (IsHighlight)
                {
                    Pen highlightPen = new Pen(Color.White);
                    g.DrawRectangle(highlightPen, 0, 0, width - 1, height - 1);
                }

                if (DisplayIcon && Settings.ShowIconSectionSplit)
                {
                    var icon = Split.Icon ?? NoIconImage;
                    var shadow = (Split.Icon != null) ? ShadowImage : NoIconShadow;

                    if (OldImage != icon)
                    {
                        ImageAnimator.Animate(icon, (s, o) => { });
                        ImageAnimator.Animate(shadow, (s, o) => { });
                        OldImage = icon;
                    }

                    var drawWidth = Settings.IconSize;
                    var drawHeight = Settings.IconSize;
                    var shadowWidth = Settings.IconSize * (5 / 4f);
                    var shadowHeight = Settings.IconSize * (5 / 4f);
                    if (icon.Width > icon.Height)
                    {
                        var ratio = icon.Height / (float)icon.Width;
                        drawHeight *= ratio;
                        shadowHeight *= ratio;
                    }
                    else
                    {
                        var ratio = icon.Width / (float)icon.Height;
                        drawWidth *= ratio;
                        shadowWidth *= ratio;
                    }

                    ImageAnimator.UpdateFrames(shadow);
                    if (Settings.IconShadows && shadow != null)
                    {
                        g.DrawImage(
                            shadow,
                            7 + (Settings.IconSize * (5 / 4f) - shadowWidth) / 2 - 0.7f + ((Settings.IndentSubsplits && IsSubsplit) || ForceIndent ? 20 : 0),
                            (height - Settings.IconSize) / 2.0f + (Settings.IconSize * (5 / 4f) - shadowHeight) / 2 - 0.7f,
                            shadowWidth,
                            shadowHeight);
                    }

                    ImageAnimator.UpdateFrames(icon);

                    g.DrawImage(
                        icon,
                        7 + (Settings.IconSize - drawWidth) / 2 + ((Settings.IndentSubsplits && IsSubsplit) || ForceIndent ? 20 : 0),
                        (height - Settings.IconSize) / 2.0f + (Settings.IconSize - drawHeight) / 2,
                        drawWidth,
                        drawHeight);
                }

                NameLabel.Font = state.LayoutSettings.TextFont;
                //NameLabel.Text = Split.Name;

                if ((Settings.IndentSubsplits && IsSubsplit) || ForceIndent)
                    NameLabel.X = 25 + IconWidth;
                else
                    NameLabel.X = 5 + IconWidth;

                NameLabel.HasShadow = state.LayoutSettings.DropShadows;

                TimeLabel.Font = state.LayoutSettings.TimesFont;

                if (Settings.ShowSplitTimes)
                {
                    TimeLabel.Width = MeasureTimeLabel.ActualWidth + 20;
                    TimeLabel.X = width - MeasureTimeLabel.ActualWidth - 27;
                }
                else
                {
                    TimeLabel.Width = Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth) + 20;
                    TimeLabel.X = width - Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth) - 27;
                }
                TimeLabel.HasShadow = state.LayoutSettings.DropShadows;
                TimeLabel.IsMonospaced = true;

                DeltaLabel.Font = state.LayoutSettings.TimesFont;
                DeltaLabel.X = width - MeasureTimeLabel.ActualWidth - MeasureDeltaLabel.ActualWidth - 32;
                DeltaLabel.Width = MeasureDeltaLabel.ActualWidth + 20;
                DeltaLabel.HasShadow = state.LayoutSettings.DropShadows;
                DeltaLabel.IsMonospaced = true;

                if ((Settings.IndentSubsplits && IsSubsplit) || ForceIndent)
                    NameLabel.Width = -20 + width - IconWidth - (mode == LayoutMode.Vertical ? DeltaLabel.ActualWidth + (String.IsNullOrEmpty(DeltaLabel.Text) ? TimeLabel.ActualWidth : MeasureTimeLabel.ActualWidth + 5) + 10 : 10);
                else
                    NameLabel.Width = width - IconWidth - (mode == LayoutMode.Vertical ? DeltaLabel.ActualWidth + (String.IsNullOrEmpty(DeltaLabel.Text) ? TimeLabel.ActualWidth : MeasureTimeLabel.ActualWidth + 5) + 10 : 10);

                NameLabel.Draw(g);
                TimeLabel.Draw(g);
                DeltaLabel.Draw(g);
            }
        }

        private void DrawHeader(Graphics g, LiveSplitState state, float width, float height, LayoutMode mode, Region clipRegion)
        {
            if (Settings.BackgroundGradient == ExtendedGradientType.Alternating)
                g.FillRectangle(new SolidBrush(
                    oddSplit
                    ? Settings.BackgroundColor
                    : Settings.BackgroundColor2
                    ), 0, 0, width, height);

            var currentSplitBrush = new LinearGradientBrush(
                new PointF(0, 0),
                Settings.HeaderGradient == GradientType.Horizontal
                ? new PointF(width, 0)
                : new PointF(0, height),
                Settings.HeaderTopColor,
                Settings.HeaderGradient == GradientType.Plain
                ? Settings.HeaderTopColor
                : Settings.HeaderBottomColor);
            g.FillRectangle(currentSplitBrush, 0, 0, width, height);            

            MeasureTimeLabel.Text = HeaderTimesFormatter.Format(new TimeSpan(24, 0, 0));
            MeasureDeltaLabel.Text = SectionTimerFormatter.Format(new TimeSpan(0, 24, 0, 0));

            MeasureTimeLabel.Font = state.LayoutSettings.TimesFont;
            MeasureTimeLabel.IsMonospaced = true;
            MeasureDeltaLabel.Font = state.LayoutSettings.TimesFont;
            MeasureDeltaLabel.IsMonospaced = true;

            MeasureTimeLabel.SetActualWidth(g);
            MeasureDeltaLabel.SetActualWidth(g);
            TimeLabel.SetActualWidth(g);
            DeltaLabel.SetActualWidth(g);

            NameLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            TimeLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            DeltaLabel.ShadowColor = state.LayoutSettings.ShadowsColor;
            MinimumWidth = (Settings.SectionTimer ? MeasureDeltaLabel.ActualWidth : 10) + MeasureTimeLabel.ActualWidth + IconWidth + 10;
            MinimumHeight = 0.85f * (g.MeasureString("A", state.LayoutSettings.TimesFont).Height + g.MeasureString("A", state.LayoutSettings.TextFont).Height);

            if (Settings.SplitTimesAccuracy != CurrentAccuracy)
            {
                TimeFormatter = new RegularSplitTimeFormatter(Settings.SplitTimesAccuracy);
                CurrentAccuracy = Settings.SplitTimesAccuracy;
            }
            if (Settings.DeltasAccuracy != CurrentDeltaAccuracy || Settings.DropDecimals != CurrentDropDecimals)
            {
                DeltaTimeFormatter = new DeltaSplitTimeFormatter(Settings.DeltasAccuracy, Settings.DropDecimals);
                CurrentDeltaAccuracy = Settings.DeltasAccuracy;
                CurrentDropDecimals = Settings.DropDecimals;
            }
            if (Settings.HeaderAccuracy != CurrentHeaderTimesAccuracy)
            {
                HeaderTimesFormatter = new RegularSplitTimeFormatter(Settings.HeaderAccuracy);
                CurrentHeaderTimesAccuracy = Settings.HeaderAccuracy;
            }
            if (Settings.SectionTimerAccuracy != CurrentSectionTimerAccuracy)
            {
                SectionTimerFormatter = new RegularSplitTimeFormatter(Settings.SectionTimerAccuracy);
                CurrentSectionTimerAccuracy = Settings.SectionTimerAccuracy;
            }
            
            if (mode == LayoutMode.Vertical)
            {
                NameLabel.VerticalAlignment = StringAlignment.Center;
                DeltaLabel.VerticalAlignment = StringAlignment.Center;
                TimeLabel.VerticalAlignment = StringAlignment.Center;
                NameLabel.Y = 0;
                DeltaLabel.Y = 0;
                TimeLabel.Y = 0;
                NameLabel.Height = height;
                DeltaLabel.Height = height;
                TimeLabel.Height = height;
            }
            else
            {
                NameLabel.VerticalAlignment = StringAlignment.Near;
                DeltaLabel.VerticalAlignment = StringAlignment.Far;
                TimeLabel.VerticalAlignment = StringAlignment.Far;
                NameLabel.Y = 0;
                DeltaLabel.Y = height - 50;
                TimeLabel.Y = height - 50;
                NameLabel.Height = 50;
                DeltaLabel.Height = 50;
                TimeLabel.Height = 50;
            }

            if (Settings.ShowSectionIcon)
            {
                var icon = Split.Icon ?? NoIconImage;
                var shadow = (Split.Icon != null) ? ShadowImage : NoIconShadow;

                if (OldImage != icon)
                {
                    ImageAnimator.Animate(icon, (s, o) => { });
                    ImageAnimator.Animate(shadow, (s, o) => { });
                    OldImage = icon;
                }

                var drawWidth = Settings.IconSize;
                var drawHeight = Settings.IconSize;
                var shadowWidth = Settings.IconSize * (5 / 4f);
                var shadowHeight = Settings.IconSize * (5 / 4f);
                if (icon.Width > icon.Height)
                {
                    var ratio = icon.Height / (float)icon.Width;
                    drawHeight *= ratio;
                    shadowHeight *= ratio;
                }
                else
                {
                    var ratio = icon.Width / (float)icon.Height;
                    drawWidth *= ratio;
                    shadowWidth *= ratio;
                }

                ImageAnimator.UpdateFrames(shadow);
                if (Settings.IconShadows && shadow != null)
                {
                    g.DrawImage(
                        shadow,
                        7 + (Settings.IconSize * (5 / 4f) - shadowWidth) / 2 - 0.7f + ((Settings.IndentSubsplits && IsSubsplit) ? 20 : 0),
                        (height - Settings.IconSize) / 2.0f + (Settings.IconSize * (5 / 4f) - shadowHeight) / 2 - 0.7f,
                        shadowWidth,
                        shadowHeight);
                }

                ImageAnimator.UpdateFrames(icon);

                g.DrawImage(
                    icon,
                    7 + (Settings.IconSize - drawWidth) / 2 + ((Settings.IndentSubsplits && IsSubsplit) ? 20 : 0),
                    (height - Settings.IconSize) / 2.0f + (Settings.IconSize - drawHeight) / 2,
                    drawWidth,
                    drawHeight);
            }

            NameLabel.Font = state.LayoutSettings.TextFont;
            //NameLabel.Text = Split.Name;

            NameLabel.X = 5 + IconWidth;
            NameLabel.HasShadow = state.LayoutSettings.DropShadows;

            TimeLabel.Font = state.LayoutSettings.TimesFont;

            TimeLabel.Width = Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth) + 20;
            TimeLabel.X = width - Math.Max(MeasureDeltaLabel.ActualWidth, MeasureTimeLabel.ActualWidth) - 27;

            TimeLabel.HasShadow = state.LayoutSettings.DropShadows;
            TimeLabel.IsMonospaced = true;

            DeltaLabel.Font = state.LayoutSettings.TimesFont;
            DeltaLabel.X = width - MeasureTimeLabel.ActualWidth - MeasureDeltaLabel.ActualWidth - 32;
            DeltaLabel.Width = MeasureDeltaLabel.ActualWidth + 20;
            DeltaLabel.HasShadow = state.LayoutSettings.DropShadows;
            DeltaLabel.IsMonospaced = true;

            NameLabel.Width = width - IconWidth - (mode == LayoutMode.Vertical ? DeltaLabel.ActualWidth + (String.IsNullOrEmpty(DeltaLabel.Text) ? TimeLabel.ActualWidth : MeasureTimeLabel.ActualWidth + 5) + 10 : 10);

            Color originalColor = DeltaLabel.ForeColor;
            if (Settings.SectionTimer && Settings.SectionTimerGradient)
            {
                var bigFont = state.LayoutSettings.TimerFont;
                var sizeMultiplier = bigFont.Size / ((16f / 2048) * bigFont.FontFamily.GetEmHeight(bigFont.Style));
                var ascent = sizeMultiplier * (16f / 2048) * bigFont.FontFamily.GetCellAscent(bigFont.Style);
                var descent = sizeMultiplier * (16f / 2048) * bigFont.FontFamily.GetCellDescent(bigFont.Style);
                
                if (state.Run.IndexOf(Split) >= state.CurrentSplitIndex)
                {
                    double h, s, v;
                    originalColor.ToHSV(out h, out s, out v);

                    Color bottomColor = ColorExtensions.FromHSV(h, s, 0.8 * v);
                    Color topColor = ColorExtensions.FromHSV(h, 0.5 * s, Math.Min(1, 1.5 * v + 0.1));

                    var bigTimerGradiantBrush = new LinearGradientBrush(
                        new PointF(DeltaLabel.X, DeltaLabel.Y),
                        new PointF(DeltaLabel.X, DeltaLabel.Y + ascent + descent),
                        topColor,
                        bottomColor);

                    DeltaLabel.Brush = bigTimerGradiantBrush;
                }
            }

            NameLabel.Draw(g);
            TimeLabel.Draw(g);
            DeltaLabel.Draw(g);

            DeltaLabel.Brush = new SolidBrush(originalColor);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            if (Header)
                if (Settings.Display2Rows)
                    DrawHeader(g, state, width, VerticalHeight, LayoutMode.Horizontal, clipRegion);
                else
                    DrawHeader(g, state, width, VerticalHeight, LayoutMode.Vertical, clipRegion);
            else
                if (Settings.Display2Rows)
                    DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Horizontal, clipRegion);
                else
                    DrawGeneral(g, state, width, VerticalHeight, LayoutMode.Vertical, clipRegion);
            
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawGeneral(g, state, HorizontalWidth, height, LayoutMode.Horizontal, clipRegion);
        }

        public string ComponentName
        {
            get { return "Split"; }
        }


        public Control GetSettingsControl(LayoutMode mode)
        {
            throw new NotImplementedException();
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            throw new NotImplementedException();
        }


        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            throw new NotImplementedException();
        }

        public string UpdateName
        {
            get { throw new NotSupportedException(); }
        }

        public string XMLURL
        {
            get { throw new NotSupportedException(); }
        }

        public string UpdateURL
        {
            get { throw new NotSupportedException(); }
        }

        public Version Version
        {
            get { throw new NotSupportedException(); }
        }

        private TimeSpan? getSectionTime(LiveSplitState state, int splitNumber, int topNumber, String comparison, TimingMethod method, int currentIndex)
        {
            if (topNumber > currentIndex)
                return null;

            if (splitNumber < currentIndex)
                return (state.Run[splitNumber].SplitTime[method] - (topNumber > 0 ? state.Run[topNumber - 1].SplitTime[method] : TimeSpan.Zero));

            //equal
            return state.CurrentTime[method] - (topNumber > 0 ? state.Run[topNumber - 1].SplitTime[method] : TimeSpan.Zero);
        }

        private TimeSpan? getSectionComparasion(LiveSplitState state, int splitNumber, int topNumber, String comparison, TimingMethod method)
        {
            return (state.Run[splitNumber].Comparisons[comparison][method] - (topNumber > 0 ? state.Run[topNumber - 1].Comparisons[comparison][method] : TimeSpan.Zero));
        }
        
        private TimeSpan? getSectionDelta(LiveSplitState state, int splitNumber, int topNumber, String comparison, TimingMethod method, int currentIndex)
        {
            return getSectionTime(state, splitNumber, topNumber, comparison, method, currentIndex) - getSectionComparasion(state, splitNumber, topNumber, comparison, method);
        }

        private Color? GetSectionColor(LiveSplitState state, TimeSpan? timeDifference, TimeSpan? delta)
        {
            Color? splitColor = null;

            if (timeDifference != null)
            {
                if (timeDifference < TimeSpan.Zero)
                {
                    splitColor = state.LayoutSettings.AheadGainingTimeColor;
                    if (delta != null && delta > TimeSpan.Zero)
                        splitColor = state.LayoutSettings.AheadLosingTimeColor;
                }
                else
                {
                    splitColor = state.LayoutSettings.BehindLosingTimeColor;
                    if (delta != null && delta < TimeSpan.Zero)
                        splitColor = state.LayoutSettings.BehindGainingTimeColor;
                }
            }
            else
            {
                if (delta != null)
                    if (delta < TimeSpan.Zero)
                        splitColor = state.LayoutSettings.AheadGainingTimeColor;
                    else
                        splitColor = state.LayoutSettings.BehindLosingTimeColor;
            }

            return splitColor;
        }

        protected void UpdateAll(LiveSplitState state)
        {
            if (Split != null)
            {
                IsActive = (state.CurrentPhase == TimerPhase.Running
                            || state.CurrentPhase == TimerPhase.Paused) &&
                            ((!Settings.HideSubsplits && state.CurrentSplit == Split) ||
                            (SplitsSettings.SectionSplit != null && SplitsSettings.SectionSplit == Split));
                IsHighlight = (SplitsSettings.HilightSplit == Split);
                IsSubsplit = Split.Name.StartsWith("-") && Split != state.Run.Last();

                if (IsSubsplit)
                    NameLabel.Text = Split.Name.Substring(1);
                else {
                    Match match = SubsplitRegex.Match(Split.Name);
                    if (match.Success) {
                        if (CollapsedSplit || Header)
                            NameLabel.Text = match.Groups[1].Value;
                        else
                            NameLabel.Text = match.Groups[2].Value;
                    } else
                        NameLabel.Text = Split.Name;
                }
          
                var comparison = Settings.Comparison == "Current Comparison" ? state.CurrentComparison : Settings.Comparison;
                if (!state.Run.Comparisons.Contains(comparison))
                    comparison = state.CurrentComparison;

                var splitIndex = state.Run.IndexOf(Split);

                if (Header)
                {
                    TimeSpan? deltaTime = getSectionDelta(state, splitIndex, TopSplit, comparison, state.CurrentTimingMethod, state.CurrentSplitIndex);
                    if ((splitIndex >= state.CurrentSplitIndex) && (deltaTime < TimeSpan.Zero))
                    {
                        deltaTime = null;
                    }

                    var color = GetSectionColor(state, null, deltaTime);
                    if (color == null)
                        color = Settings.OverrideHeaderColor ? Settings.HeaderTimesColor : state.LayoutSettings.TextColor;
                    TimeLabel.ForeColor = color.Value;
                    NameLabel.ForeColor = Settings.OverrideHeaderColor ? Settings.HeaderTextColor : state.LayoutSettings.TextColor;

                    if (deltaTime != null)
                        TimeLabel.Text = DeltaTimeFormatter.Format(deltaTime);
                    else
                        if (splitIndex < state.CurrentSplitIndex)
                            TimeLabel.Text = "-";
                        else
                            TimeLabel.Text = HeaderTimesFormatter.Format(getSectionComparasion(state, splitIndex, TopSplit, comparison, state.CurrentTimingMethod));

                    TimeSpan? sectionTime = getSectionTime(state, splitIndex, TopSplit, comparison, state.CurrentTimingMethod, state.CurrentSplitIndex);
                    DeltaLabel.Text = SectionTimerFormatter.Format(sectionTime);
                    if (splitIndex < state.CurrentSplitIndex)
                        DeltaLabel.ForeColor = (Settings.OverrideHeaderColor ? Settings.HeaderTimesColor : state.LayoutSettings.TextColor);
                    else
                        DeltaLabel.ForeColor = Settings.SectionTimerColor;

                    if (!Settings.HeaderText)
                        NameLabel.Text = "";
                    if (!Settings.HeaderTimes)
                        TimeLabel.Text = "";
                    if (!Settings.SectionTimer)
                        DeltaLabel.Text = "";
                }
                else if (CollapsedSplit)
                {
                    int currentSplit = state.CurrentSplitIndex;
                    if (SplitsSettings.SectionSplit != null)
                        currentSplit = state.Run.IndexOf(SplitsSettings.SectionSplit);

                    TimeSpan? deltaTime = getSectionDelta(state, splitIndex, TopSplit, comparison, state.CurrentTimingMethod, currentSplit);
                    TimeSpan? comparasionTime = Split.SplitTime[state.CurrentTimingMethod] - Split.Comparisons[comparison][state.CurrentTimingMethod];

                    Color? timeColor;
                    if (splitIndex < currentSplit)
                    {
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.BeforeNamesColor : state.LayoutSettings.TextColor;
                        timeColor = Settings.OverrideTextColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                    }
                    else if (splitIndex > currentSplit)
                    {
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.AfterNamesColor : state.LayoutSettings.TextColor;
                        timeColor = Settings.OverrideTextColor ? Settings.AfterTimesColor : state.LayoutSettings.TextColor;
                    }
                    else //equal
                    {
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.CurrentNamesColor : state.LayoutSettings.TextColor;
                        timeColor = Settings.OverrideTextColor ? Settings.CurrentTimesColor : state.LayoutSettings.TextColor;
                        TimeLabel.ForeColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : state.LayoutSettings.TextColor;

                        if (Split.SplitTime[state.CurrentTimingMethod] == null
                                && (state.CurrentTime[state.CurrentTimingMethod] > Split.Comparisons[comparison][state.CurrentTimingMethod]))
                            comparasionTime = state.CurrentTime[state.CurrentTimingMethod] - Split.Comparisons[comparison][state.CurrentTimingMethod];
                    }

                    var deltaColor = GetSectionColor(state, comparasionTime, deltaTime);
                    if (splitIndex == currentSplit)
                        deltaColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : state.LayoutSettings.TextColor;
                    else
                        deltaColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : deltaColor ?? timeColor;

                    TimeLabel.ForeColor = timeColor.Value;
                    DeltaLabel.ForeColor = deltaColor.Value;

                    if (splitIndex < currentSplit)
                    {
                        if (!Settings.ShowSplitTimes)
                        {
                            TimeLabel.ForeColor = deltaColor.Value;
                            if (comparasionTime != null)
                                TimeLabel.Text = DeltaTimeFormatter.Format(comparasionTime);
                            else
                                TimeLabel.Text = TimeFormatter.Format(Split.SplitTime[state.CurrentTimingMethod]);
                            DeltaLabel.Text = "";
                        }
                        else
                        {
                            TimeLabel.Text = TimeFormatter.Format(Split.SplitTime[state.CurrentTimingMethod]);
                            DeltaLabel.Text = DeltaTimeFormatter.Format(comparasionTime);
                        }
                    }
                    else
                    {
                        if (!Settings.ShowSplitTimes)
                        {
                            TimeLabel.ForeColor = deltaColor.Value;
                            if (comparasionTime != null)
                                TimeLabel.Text = DeltaTimeFormatter.Format(comparasionTime);
                            else
                                TimeLabel.Text = TimeFormatter.Format(Split.Comparisons[comparison][state.CurrentTimingMethod]);
                            DeltaLabel.Text = "";
                        }
                        else
                        {
                            TimeLabel.Text = TimeFormatter.Format(Split.Comparisons[comparison][state.CurrentTimingMethod]);
                            if (comparasionTime != null)
                                DeltaLabel.Text = DeltaTimeFormatter.Format(comparasionTime);
                            else
                                DeltaLabel.Text = "";
                        }
                    }
                }
                else if (splitIndex < state.CurrentSplitIndex)
                {
                    TimeLabel.ForeColor = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                    NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.BeforeNamesColor : state.LayoutSettings.TextColor;
                    var deltaTime = Split.SplitTime[state.CurrentTimingMethod] - Split.Comparisons[comparison][state.CurrentTimingMethod];
                    
                    if (!Settings.ShowSplitTimes)
                    {
                        var color = LiveSplitStateHelper.GetSplitColor(state, deltaTime, splitIndex, true, true, comparison, state.CurrentTimingMethod);
                        if (color == null)
                            color = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                        TimeLabel.ForeColor = color.Value;
                        if (deltaTime != null)
                            TimeLabel.Text = DeltaTimeFormatter.Format(deltaTime);
                        else
                            TimeLabel.Text = TimeFormatter.Format(Split.SplitTime[state.CurrentTimingMethod]);
                        DeltaLabel.Text = "";
                    }
                    else
                    {
                        var color = LiveSplitStateHelper.GetSplitColor(state, deltaTime, splitIndex, true, true, comparison, state.CurrentTimingMethod);
                        if (color == null)
                            color = Settings.OverrideTimesColor ? Settings.BeforeTimesColor : state.LayoutSettings.TextColor;
                        DeltaLabel.ForeColor = color.Value;
                        TimeLabel.Text = TimeFormatter.Format(Split.SplitTime[state.CurrentTimingMethod]);
                        DeltaLabel.Text = DeltaTimeFormatter.Format(deltaTime);
                    }
                }
                else
                {
                    if (Split == state.CurrentSplit)
                    {
                        TimeLabel.ForeColor = Settings.OverrideTimesColor ? Settings.CurrentTimesColor : state.LayoutSettings.TextColor;
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.CurrentNamesColor : state.LayoutSettings.TextColor;
                    }
                    else
                    {
                        TimeLabel.ForeColor = Settings.OverrideTimesColor ? Settings.AfterTimesColor : state.LayoutSettings.TextColor;
                        NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.AfterNamesColor : state.LayoutSettings.TextColor;
                    }
                    TimeLabel.Text = TimeFormatter.Format(Split.Comparisons[comparison][state.CurrentTimingMethod]);
                    //Live Delta
                    var bestDelta = LiveSplitStateHelper.CheckLiveDelta(state, true, comparison, state.CurrentTimingMethod);
                    if (bestDelta != null && Split == state.CurrentSplit)
                    {
                        if (!Settings.ShowSplitTimes)
                        {
                            TimeLabel.Text = DeltaTimeFormatter.Format(bestDelta);
                            TimeLabel.ForeColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : state.LayoutSettings.TextColor;
                            DeltaLabel.Text = "";
                        }
                        else
                        {
                            DeltaLabel.Text = DeltaTimeFormatter.Format(bestDelta);
                            DeltaLabel.ForeColor = Settings.OverrideDeltasColor ? Settings.DeltasColor : state.LayoutSettings.TextColor;
                        }
                    }
                    else
                    {
                        DeltaLabel.Text = "";
                    }
                }
            }
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (Split != null)
            {
                UpdateAll(state);
                NeedUpdateAll = false;

                Cache.Restart();
                Cache["Icon"] = Split.Icon;
                if (Cache.HasChanged)
                {
                    if (Split.Icon == null)
                        FrameCount = 0;
                    else
                        FrameCount = Split.Icon.GetFrameCount(new FrameDimension(Split.Icon.FrameDimensionsList[0]));
                }
                Cache["SplitName"] = NameLabel.Text;
                Cache["DeltaLabel"] = DeltaLabel.Text;
                Cache["TimeLabel"] = TimeLabel.Text;
                Cache["IsActive"] = IsActive;
                Cache["IsHighlight"] = IsHighlight;
                Cache["NameColor"] = NameLabel.ForeColor.ToArgb();
                Cache["TimeColor"] = TimeLabel.ForeColor.ToArgb();
                Cache["DeltaColor"] = DeltaLabel.ForeColor.ToArgb();
                Cache["Indent"] = ((IsSubsplit && Settings.IndentSubsplits) || (ForceIndent));

                if (invalidator != null && Cache.HasChanged || FrameCount > 1 || blankOut)
                {
                    invalidator.Invalidate(0, 0, width, height);
                }

                blankOut = false;
            }
            else if (!blankOut)
            {
                blankOut = true;
                IsSubsplit = false;
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public void Dispose()
        {
        }
    }
}
