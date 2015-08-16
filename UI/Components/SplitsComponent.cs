using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class SplitsComponent : IComponent
    {
        public ComponentRendererComponent InternalComponent { get; protected set; }

        public float PaddingTop { get { return InternalComponent.PaddingTop; } }
        public float PaddingLeft { get { return InternalComponent.PaddingLeft; } }
        public float PaddingBottom { get { return InternalComponent.PaddingBottom; } }
        public float PaddingRight { get { return InternalComponent.PaddingRight; } }

        protected IList<IComponent> Components { get; set; }
        protected IList<SplitComponent> SplitComponents { get; set; }

        protected SplitsSettings Settings { get; set; }

        private Dictionary<Image, Image> ShadowImages { get; set; }

        private int visualSplitCount;
        private int settingsSplitCount;
        private IRun previousRun;

        private SectionList sectionList;

        protected int ScrollOffset { get; set; }
        protected int LastSplitSeparatorIndex { get; set; }
        private int lastSplitOfSection { get; set; }

        protected LiveSplitState State { get; set; }

        protected Color OldShadowsColor { get; set; }

        public string ComponentName
        {
            get { return "Subsplits" + (Settings.Comparison == "Current Comparison" ? "" : " (" + CompositeComparisons.GetShortComparisonName(Settings.Comparison) + ")"); }
        }

        public float VerticalHeight
        {
            get { return InternalComponent.VerticalHeight; }
        }

        public float MinimumWidth
        {
            get { return InternalComponent.MinimumWidth; }
        }

        public float HorizontalWidth
        {
            get { return InternalComponent.HorizontalWidth; }
        }

        public float MinimumHeight
        {
            get { return InternalComponent.MinimumHeight; }
        }

        public IDictionary<string, Action> ContextMenuControls
        {
            get { return null; }
        }

        public SplitsComponent(LiveSplitState state)
        {
            Settings = new SplitsSettings()
            {
                CurrentState = state
            };
            InternalComponent = new ComponentRendererComponent();
            ShadowImages = new Dictionary<Image, Image>();
            visualSplitCount = Settings.VisualSplitCount;
            settingsSplitCount = Settings.VisualSplitCount;
            Settings.SplitLayoutChanged += Settings_SplitLayoutChanged;
            ScrollOffset = 0;
            RebuildVisualSplits();
            sectionList = new SectionList();
            previousRun = state.Run;
            sectionList.UpdateSplits(state.Run);
        }

        void state_RunManuallyModified(object sender, EventArgs e)
        {
            sectionList.UpdateSplits(((LiveSplitState)sender).Run);
        }

        void state_ComparisonRenamed(object sender, EventArgs e)
        {
            var args = (RenameEventArgs)e;
            if (Settings.Comparison == args.OldName)
            {
                Settings.Comparison = args.NewName;
                ((LiveSplitState)sender).Layout.HasChanged = true;
            }
        }

        void Settings_SplitLayoutChanged(object sender, EventArgs e)
        {
            RebuildVisualSplits();
        }

        private void RebuildVisualSplits()
        {
            Components = new List<IComponent>();
            SplitComponents = new List<SplitComponent>();
            InternalComponent.VisibleComponents = Components;

            for (var i = 0; i < visualSplitCount; ++i)
            {
                if (i > 0 && i == visualSplitCount - 1)
                {
                    LastSplitSeparatorIndex = Components.Count;
                    if (Settings.AlwaysShowLastSplit && Settings.SeparatorLastSplit)
                        Components.Add(new SeparatorComponent());
                    else if (Settings.ShowThinSeparators)
                        Components.Add(new ThinSeparatorComponent());
                }

                var splitComponent = new SplitComponent(Settings);
                Components.Add(splitComponent);
                SplitComponents.Add(splitComponent);

                if (Settings.ShowThinSeparators && i < visualSplitCount - 2)
                    Components.Add(new ThinSeparatorComponent());
            }
        }

        private void Prepare(LiveSplitState state)
        {
            if (state != State)
            {
                state.OnScrollDown += state_OnScrollDown;
                state.OnScrollUp += state_OnScrollUp;
                state.OnStart += state_OnStart;
                state.OnReset += state_OnReset;
                state.OnSplit += state_OnSplit;
                state.OnSkipSplit += state_OnSkipSplit;
                state.OnUndoSplit += state_OnUndoSplit;
                state.ComparisonRenamed += state_ComparisonRenamed;
                state.RunManuallyModified += state_RunManuallyModified;
                State = state;
            }

            if (Settings.VisualSplitCount != visualSplitCount)
            {
                visualSplitCount = Settings.VisualSplitCount;
                RebuildVisualSplits();
            }

            if (OldShadowsColor != state.LayoutSettings.ShadowsColor)
                ShadowImages.Clear();

            foreach (var split in state.Run)
            {
                if (split.Icon != null && (!ShadowImages.ContainsKey(split.Icon) || OldShadowsColor != state.LayoutSettings.ShadowsColor))
                {
                    ShadowImages.Add(split.Icon, IconShadow.Generate(split.Icon, state.LayoutSettings.ShadowsColor));
                }
            }

            var iconsNotBlank = state.Run.Where(x => x.Icon != null).Count() > 0;

            foreach (var split in SplitComponents)
            {
                var hideIconSectionSplit = !Settings.ShowIconSectionSplit && split.Split != null && state.Run.IndexOf(split.Split) == lastSplitOfSection;
                
                split.DisplayIcon = Settings.DisplayIcons && !hideIconSectionSplit && iconsNotBlank 
                    && (split.Split == null || split.Split.Icon != null || Settings.IndentBlankIcons);

                if (split.Split != null && split.Split.Icon != null)
                    split.ShadowImage = ShadowImages[split.Split.Icon];
                else
                    split.ShadowImage = null;
            }
            OldShadowsColor = state.LayoutSettings.ShadowsColor;

            foreach (var component in Components)
            {
                if (component is SeparatorComponent)
                {
                    var separator = (SeparatorComponent)component;
                    var index = Components.IndexOf(separator);
                    if (state.CurrentPhase == TimerPhase.Running || state.CurrentPhase == TimerPhase.Paused)
                    {
                        if (((SplitComponent)Components[index + 1]).Split == state.CurrentSplit)
                            separator.LockToBottom = true;
                        else if (((SplitComponent)Components[index - 1]).Split == state.CurrentSplit)
                            separator.LockToBottom = false;
                    }
                }
                else if (component is ThinSeparatorComponent)
                {
                    var separator = (ThinSeparatorComponent)component;
                    var index = Components.IndexOf(separator);
                    if (state.CurrentPhase == TimerPhase.Running || state.CurrentPhase == TimerPhase.Paused)
                    {
                        if (((SplitComponent)Components[index + 1]).Split == state.CurrentSplit)
                            separator.LockToBottom = true;
                        else if (((SplitComponent)Components[index - 1]).Split == state.CurrentSplit)
                            separator.LockToBottom = false;
                    }
                }
            }
        }

        void state_OnUndoSplit(object sender, EventArgs e)
        {
            ScrollOffset = 0;
        }

        void state_OnSkipSplit(object sender, EventArgs e)
        {
            ScrollOffset = 0;
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            ScrollOffset = 0;
        }

        void state_OnReset(object sender, TimerPhase e)
        {
            ScrollOffset = 0;
        }

        private sealed class SectionList
        {
            public sealed class Section
            {
                public readonly int startIndex;
                public readonly int endIndex;

                public Section(int topIndex, int bottomIndex)
                {
                    startIndex = topIndex;
                    endIndex = bottomIndex;
                }

                public bool splitInRange(int splitIndex)
                {
                    return (splitIndex >= startIndex && splitIndex <= endIndex);
                }

                public int getSubsplitCount()
                {
                    return endIndex - startIndex;
                }
            }

            public List<Section> Sections;

            public void UpdateSplits(IRun splits)
            {
                Sections = new List<Section>();
                for (int splitIndex = splits.Count() - 1; splitIndex >= 0; splitIndex--)
                {
                    int sectionIndex = splitIndex;
                    while ((splitIndex > 0) && (splits[splitIndex - 1].Name.StartsWith("-")))
                        splitIndex--;

                    Sections.Insert(0, new Section(splitIndex, sectionIndex));
                }
            }

            public int getSection(int splitIndex)
            {
                foreach (Section section in Sections)
                {
                    if (section.splitInRange(splitIndex))
                    {
                        return Sections.IndexOf(section);
                    }
                }

                return -1;
            }

            public bool isMajorSplit(int splitIndex)
            {
                int sectionIndex = getSection(splitIndex);

                if (sectionIndex == -1)
                    return true;

                return (splitIndex == Sections[sectionIndex].endIndex);
            }

            public int getMajorSplit(int splitIndex)
            {
                int sectionIndex = getSection(splitIndex);

                if (sectionIndex == -1)
                    return splitIndex;

                return Sections[sectionIndex].endIndex;
            }
        }

        void state_OnStart(object sender, EventArgs e)
        {
            ScrollOffset = 0;
        }

        void state_OnScrollUp(object sender, EventArgs e)
        {
            ScrollOffset--;
        }

        void state_OnScrollDown(object sender, EventArgs e)
        {
            ScrollOffset++;
        }

        void DrawBackground(Graphics g, float width, float height)
        {
            if (Settings.BackgroundGradient != ExtendedGradientType.Alternating
                && (Settings.BackgroundColor.ToArgb() != Color.Transparent.ToArgb()
                || Settings.BackgroundGradient != ExtendedGradientType.Plain
                && Settings.BackgroundColor2.ToArgb() != Color.Transparent.ToArgb()))
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == ExtendedGradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == ExtendedGradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            Prepare(state);
            DrawBackground(g, width, VerticalHeight);
            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            Prepare(state);
            DrawBackground(g, HorizontalWidth, height);
            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
            RebuildVisualSplits();
        }


        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (state.Run != previousRun)
            {
                sectionList.UpdateSplits(state.Run);
                previousRun = state.Run;
            }

            var runningSectionIndex = Math.Min(Math.Max(state.CurrentSplitIndex, 0), state.Run.Count - 1);
            ScrollOffset = Math.Min(Math.Max(ScrollOffset, -runningSectionIndex), state.Run.Count - runningSectionIndex - 1);
            var currentSplit = ScrollOffset + runningSectionIndex;
            var currentSection = sectionList.getSection(currentSplit);
            runningSectionIndex = sectionList.getSection(runningSectionIndex);
            if (sectionList.Sections[currentSection].getSubsplitCount() > 0)
                lastSplitOfSection = sectionList.Sections[currentSection].endIndex;
            else
                lastSplitOfSection = -1;

            if (Settings.HideSubsplits)
            {
                if (ScrollOffset != 0)
                {
                    currentSplit = sectionList.getMajorSplit(currentSplit);
                    SplitsSettings.HilightSplit = state.Run[currentSplit];
                }
                else
                    SplitsSettings.HilightSplit = null;


                SplitsSettings.SectionSplit = state.Run[sectionList.Sections[runningSectionIndex].endIndex];
            }
            else
            {
                if (ScrollOffset != 0)
                    SplitsSettings.HilightSplit = state.Run[currentSplit];
                else
                    SplitsSettings.HilightSplit = null;

                if (currentSection == runningSectionIndex)
                    SplitsSettings.SectionSplit = null;
                else
                    SplitsSettings.SectionSplit = state.Run[sectionList.Sections[runningSectionIndex].endIndex];
            }

            bool addLast = (Settings.AlwaysShowLastSplit || currentSplit == state.Run.Count() - 1);
            bool addHeader = (Settings.ShowHeader && (sectionList.Sections[currentSection].getSubsplitCount() > 0));

            int freeSplits = visualSplitCount - (addLast ? 1 : 0) - (addHeader ? 1 : 0);
            int topSplit = currentSplit - 1;
            int bottomSplit = currentSplit + 1;

            List<int> visibleSplits = new List<int>();
            if ((currentSplit < state.Run.Count() - 1) && (freeSplits > 0) && !(Settings.HideSubsplits && !sectionList.isMajorSplit(currentSplit)))
            {
                visibleSplits.Add(currentSplit);
                freeSplits--;
            }

            int previewCount = 0;
            while ((previewCount < Settings.SplitPreviewCount) && (bottomSplit < state.Run.Count() - (addLast ? 1 : 0)) && (freeSplits > 0))
            {
                if ((sectionList.isMajorSplit(bottomSplit)
                        && (!Settings.CurrentSectionOnly || sectionList.getSection(bottomSplit) == currentSection)) ||
                    (!sectionList.isMajorSplit(bottomSplit)
                        && (Settings.ShowSubsplits || (!Settings.HideSubsplits && sectionList.getSection(bottomSplit) == currentSection))))
                {
                    visibleSplits.Add(bottomSplit);
                    freeSplits--;
                    previewCount++;
                }
                bottomSplit++;
            }

            while ((topSplit >= 0) && (freeSplits > 0))
            {
                if ((sectionList.isMajorSplit(topSplit) && (!Settings.CurrentSectionOnly)) ||
                    (!sectionList.isMajorSplit(topSplit)
                        && (Settings.ShowSubsplits || (!Settings.HideSubsplits && sectionList.getSection(topSplit) == currentSection))))
                {
                    visibleSplits.Insert(0, topSplit);
                    freeSplits--;
                }
                topSplit--;
            }

            while ((bottomSplit < state.Run.Count() - (addLast ? 1 : 0)) && (freeSplits > 0))
            {
                if ((sectionList.isMajorSplit(bottomSplit)
                        && (!Settings.CurrentSectionOnly || sectionList.getSection(bottomSplit) == currentSection)) ||
                    (!sectionList.isMajorSplit(bottomSplit)
                        && (Settings.ShowSubsplits || (!Settings.HideSubsplits && sectionList.getSection(bottomSplit) == currentSection))))
                {
                    visibleSplits.Add(bottomSplit);
                    freeSplits--;
                }
                bottomSplit++;
            }


            foreach (var component in Components)
            {
                if (component is SeparatorComponent)
                {
                    var separator = (SeparatorComponent)component;
                    var index = Components.IndexOf(separator);

                    if (Settings.AlwaysShowLastSplit && Settings.SeparatorLastSplit && index == LastSplitSeparatorIndex)
                    {
                        int lastIndex = state.Run.Count() - 1;

                        if (freeSplits > 0 || visibleSplits.Any() && (visibleSplits.Last() == lastIndex - 1))
                        {
                            if (Settings.ShowThinSeparators)
                                separator.DisplayedSize = 1f;
                            else
                                separator.DisplayedSize = 0f;

                            separator.UseSeparatorColor = false;
                        }
                        else
                        {
                            int prevSection = sectionList.getSection(lastIndex) - 1;
                            if (visibleSplits.Any() && (prevSection <= 0 || visibleSplits.Last() == sectionList.Sections[prevSection].endIndex))
                            {
                                if (Settings.ShowThinSeparators)
                                    separator.DisplayedSize = 1f;
                                else
                                    separator.DisplayedSize = 0f;

                                separator.UseSeparatorColor = false;
                            }
                            else
                            {
                                separator.DisplayedSize = 2f;
                                separator.UseSeparatorColor = true;
                            }
                        }
                    }
                }
            }


            if (!Settings.LockLastSplit && addLast)
                visibleSplits.Add(state.Run.Count() - 1);

            if (Settings.ShowBlankSplits)
            {
                for (; freeSplits > 0; freeSplits--)
                    visibleSplits.Add(int.MinValue);
            }

            if (Settings.LockLastSplit && addLast)
                visibleSplits.Add(state.Run.Count() - 1);

            if (addHeader)
            {
                int insertIndex = visibleSplits.IndexOf(sectionList.Sections[currentSection].startIndex);
                visibleSplits.Insert(insertIndex >= 0 ? insertIndex : 0, -(currentSection + 1));
            }

            int i = 0;
            foreach (int split in visibleSplits)
            {
                if (i < SplitComponents.Count)
                {
                    SplitComponents[i].ForceIndent = Settings.IndentSectionSplit && split == lastSplitOfSection;

                    if (split == int.MinValue)
                    {
                        SplitComponents[i].Header = false;
                        SplitComponents[i].CollapsedSplit = false;
                        SplitComponents[i].Split = null;
                        SplitComponents[i].oddSplit = true;
                    }
                    else if (split < 0)
                    {
                        SplitComponents[i].Header = true;
                        SplitComponents[i].CollapsedSplit = false;
                        SplitComponents[i].TopSplit = sectionList.Sections[-split - 1].startIndex;
                        SplitComponents[i].Split = state.Run[sectionList.Sections[-split - 1].endIndex];
                        SplitComponents[i].oddSplit = (((-split - 1) % 2) == 0);
                    }
                    else
                    {
                        SplitComponents[i].Header = false;
                        SplitComponents[i].Split = state.Run[split];
                        SplitComponents[i].oddSplit = ((sectionList.getSection(split) % 2) == 0);

                        if ((Settings.HideSubsplits || sectionList.getSection(split) != currentSection)
                            && sectionList.Sections[sectionList.getSection(split)].getSubsplitCount() > 0
                            && !Settings.ShowSubsplits)
                        {
                            SplitComponents[i].CollapsedSplit = true;
                            SplitComponents[i].TopSplit = sectionList.Sections[sectionList.getSection(split)].startIndex;
                        }
                        else
                        {
                            SplitComponents[i].CollapsedSplit = false;
                        }
                    }
                }

                i++;
            }

            if (invalidator != null)
                InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose()
        {
            State.OnScrollDown -= state_OnScrollDown;
            State.OnScrollUp -= state_OnScrollUp;
            State.OnStart -= state_OnStart;
            State.OnReset -= state_OnReset;
            State.OnSplit -= state_OnSplit;
            State.OnSkipSplit -= state_OnSkipSplit;
            State.OnUndoSplit -= state_OnUndoSplit;
            State.ComparisonRenamed -= state_ComparisonRenamed;
            State.RunManuallyModified -= state_RunManuallyModified;
        }

        public int GetSettingsHashCode()
        {
            return Settings.GetSettingsHashCode();
        }
    }
}
