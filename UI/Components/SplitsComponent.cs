using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private SectionList sectionList;

        protected int ScrollOffset { get; set; }
        protected int LastSplitSeparatorIndex { get; set; }

        protected LiveSplitState OldState { get; set; }

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

            var totalSplits = Settings.ShowBlankSplits ? Math.Max(Settings.VisualSplitCount, visualSplitCount) : visualSplitCount;

            for (var i = 0; i < totalSplits; ++i)
            {
                if ((i == totalSplits - 1 && totalSplits > 1 && Settings.LockLastSplit && i > 0)
                    || (i == visualSplitCount - 1 && totalSplits > 1 && !Settings.LockLastSplit && i > 0))
                {
                    LastSplitSeparatorIndex = Components.Count;
                    if (Settings.AlwaysShowLastSplit && Settings.SeparatorLastSplit)
                        Components.Add(new SeparatorComponent());
                    else if (Settings.ShowThinSeparators)
                        Components.Add(new ThinSeparatorComponent());
                }

                var splitComponent = new SplitComponent(Settings);
                Components.Add(splitComponent);
                if (i < visualSplitCount - 1 + (Settings.LockLastSplit ? 0 : 1) || i == totalSplits - 1 + (Settings.LockLastSplit ? 0 : 1))
                    SplitComponents.Add(splitComponent);                   

                if (Settings.ShowThinSeparators && ((i < totalSplits - 2 && Settings.LockLastSplit) || (!Settings.LockLastSplit && i != visualSplitCount-2 && i < totalSplits-1)))
                    Components.Add(new ThinSeparatorComponent());
            }
        }

        private void Prepare(LiveSplitState state)
        {
            if (state != OldState)
            {
                state.OnScrollDown += state_OnScrollDown;
                state.OnScrollUp += state_OnScrollUp;
                state.OnStart += state_OnStart;
                state.OnReset += state_OnReset;
                state.OnSplit += state_OnSplit;
                state.OnSkipSplit += state_OnSkipSplit;
                state.OnUndoSplit += state_OnUndoSplit;
                OldState = state;
            }

            var previousSplitCount = visualSplitCount;
            visualSplitCount = Math.Min(state.Run.Count, Settings.VisualSplitCount);
            if (previousSplitCount != visualSplitCount 
                || (Settings.ShowBlankSplits && settingsSplitCount != Settings.VisualSplitCount) )
            {
                RebuildVisualSplits();
            }
            settingsSplitCount = Settings.VisualSplitCount;

            var skipCount = Math.Min(
                Math.Max(
                    0,
                    state.CurrentSplitIndex - (visualSplitCount - 2 - Settings.SplitPreviewCount + (Settings.AlwaysShowLastSplit ? 0 : 1))),
                state.Run.Count - visualSplitCount);
            //ScrollOffset = Math.Min(Math.Max(ScrollOffset, -skipCount), state.Run.Count - skipCount - visualSplitCount);
            int currentIndex = Math.Min(Math.Max(0, state.CurrentSplitIndex), state.Run.Count - 1);
            ScrollOffset = Math.Min(Math.Max(ScrollOffset, -currentIndex), state.Run.Count - currentIndex - 1);

            skipCount += ScrollOffset;

            if (OldShadowsColor != state.LayoutSettings.ShadowsColor)
                ShadowImages.Clear();

            foreach (var split in state.Run)
            {
                if (split.Icon != null && (!ShadowImages.ContainsKey(split.Icon) || OldShadowsColor != state.LayoutSettings.ShadowsColor))
                {
                    ShadowImages.Add(split.Icon, IconShadow.Generate(split.Icon, state.LayoutSettings.ShadowsColor));
                }
            }
            foreach (var split in SplitComponents)
            {
                split.DisplayIcon = Settings.DisplayIcons && !((split.Split != null) && (split.Split.Icon == null) && Settings.HideBlankIcons);

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
                    //if (Settings.AlwaysShowLastSplit && Settings.SeparatorLastSplit && index == LastSplitSeparatorIndex)
                    //{
                    //    int secondLast = state.Run.Count() - 2;
                    //    int visualSecondLast = SplitComponents.Count() - 2;
                    //    if ((secondLast < 0) || (visualSecondLast < 0) || (state.Run.IndexOf(SplitComponents[visualSecondLast]) == secondLast))//(skipCount >= state.Run.Count - visualSplitCount)
                    //    {
                    //        if (Settings.ShowThinSeparators)
                    //            separator.DisplayedSize = 1f;
                    //        else
                    //            separator.DisplayedSize = 0f;

                    //        separator.UseSeparatorColor = false;
                    //    }
                    //    else
                    //    {
                    //        separator.DisplayedSize = 2f;
                    //        separator.UseSeparatorColor = true;
                    //    }
                    //}
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

        class SectionList {
            public class Section {
                public int startIndex;
                public int endIndex;

                public Section (int topIndex, int bottomIndex) {
                    startIndex = topIndex;
                    endIndex = bottomIndex;
                }

                public bool splitInRange(int splitIndex) {
                    return (splitIndex >= startIndex && splitIndex <= endIndex);
                }

                public int getSubsplitCount()
                {
                    return endIndex - startIndex;
                }
            }

            public List<Section> Sections;
            IRun oldSplits;

            private bool compareSplits(IRun first, IRun second)
            {
                if (first == null || second == null)
                    return false;

                if (first.Count() != second.Count())
                    return false;

                for (int i = 0; i < first.Count(); i++)
                {
                    if (first[i].Name.StartsWith("-") ^ second[i].Name.StartsWith("-"))
                        return false;
                }

                return true;
            }

            public void UpdateSplits(IRun splits) {
                if (compareSplits(oldSplits, splits))
                    return;
                oldSplits = (IRun)splits.Clone();

                Sections = new List<Section>();
                for (int splitIndex = splits.Count() - 1; splitIndex >= 0; splitIndex--)
                {
                    int sectionIndex = splitIndex;
                    while ((splitIndex > 0) && (splits[splitIndex - 1].Name.StartsWith("-")))
                        splitIndex--;

                    Sections.Insert(0, new Section(splitIndex, sectionIndex));
                }
            }

            public int getSection(int splitIndex) {
                foreach(Section section in Sections) {
                    if (section.splitInRange(splitIndex)) {
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


        public void RenameComparison(string oldName, string newName)
        {
            if (Settings.Comparison == oldName)
                Settings.Comparison = newName;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            sectionList.UpdateSplits(state.Run);
            
            int currentSplit = ScrollOffset;
            if (state.CurrentPhase != TimerPhase.NotRunning)
                currentSplit = state.CurrentSplitIndex + ScrollOffset;
            if (state.CurrentPhase == TimerPhase.Ended)
                currentSplit--;
            var currentSection = sectionList.getSection(currentSplit);

            var runningSectionIndex = state.CurrentSplitIndex;
            if (state.CurrentPhase == TimerPhase.NotRunning)
                runningSectionIndex++;
            if (state.CurrentPhase == TimerPhase.Ended)
                runningSectionIndex--;
            runningSectionIndex = sectionList.getSection(runningSectionIndex);

            if (Settings.HideSubsplits)
            {
                if (ScrollOffset != 0)
                {
                    currentSplit = sectionList.getMajorSplit(currentSplit);
                    SplitsSettings.HilightSplit = state.Run[currentSplit];
                } else
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
                        int secondLast = state.Run.Count() - 2;

                        if ((freeSplits > 0) || ((visibleSplits.Count() > 0) && (visibleSplits.Last() == secondLast)))
                        {
                            if (Settings.ShowThinSeparators)
                                separator.DisplayedSize = 1f;
                            else
                                separator.DisplayedSize = 0f;

                            separator.UseSeparatorColor = false;
                        }
                        else
                        {
                            int prevSection = sectionList.getSection(secondLast) - 1;
                            if ((visibleSplits.Count() > 0) &&
                                ((prevSection > 0) ? (visibleSplits.Last() == sectionList.Sections[prevSection].endIndex) : true))
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
                /*
                if (Settings.MultipleHeaders)
                {
                    int firstSection = sectionList.getSection(visibleSplits[0]);
                    int lastSection = sectionList.getSection(visibleSplits.Last());
                    for (int sectionIndex = firstSection; sectionIndex <= lastSection; sectionIndex++)
                    {
                        int sectionTop = sectionList.Sections[sectionIndex].startIndex;
                        int sectionBottom = sectionList.Sections[sectionIndex].endIndex;
                        if (sectionTop == sectionBottom)
                            continue;

                        int insertIndex;
                        do
                        {
                            insertIndex = visibleSplits.IndexOf(sectionTop++);
                        } while ((insertIndex == -1) && (sectionTop > sectionBottom));

                        if (sectionTop <= sectionBottom)
                            visibleSplits.Insert(insertIndex, -(sectionIndex + 1));
                    }
                }
                else
                {
                */
                    int sectionTop = sectionList.Sections[currentSection].startIndex;
                    int insertIndex;
                    do
                    {
                        insertIndex = visibleSplits.IndexOf(sectionTop++);
                    } while (insertIndex == -1);

                    visibleSplits.Insert(insertIndex, -(currentSection + 1));
                //}
            }

            int i = 0;
            foreach (int split in visibleSplits)
            {
                SplitComponents[i].ForceIndent = Settings.IndentSectionSplit && (split == sectionList.Sections[currentSection].endIndex) 
                    && (sectionList.Sections[currentSection].getSubsplitCount() > 0);

                if (split == int.MinValue) {
                    SplitComponents[i].Header = false;
                    SplitComponents[i].CollapsedSplit = false;
                    SplitComponents[i].Split = null;
                    SplitComponents[i].oddSplit = true ;
                }
                else if (split < 0) {
                    SplitComponents[i].Header = true;
                    SplitComponents[i].CollapsedSplit = false;
                    SplitComponents[i].TopSplit = sectionList.Sections[-split - 1].startIndex;
                    SplitComponents[i].Split = state.Run[sectionList.Sections[-split - 1].endIndex];
                    SplitComponents[i].oddSplit = (((-split - 1) % 2) == 0);
                }
                else {
                    SplitComponents[i].Header = false;
                    SplitComponents[i].Split = state.Run[split];
                    SplitComponents[i].oddSplit = ((sectionList.getSection(split) % 2) == 0);

                    if (split > 0 && sectionList.isMajorSplit(split) && (i == 0 || sectionList.isMajorSplit(visibleSplits[i - 1]))
                        && (sectionList.Sections[sectionList.getSection(split)].getSubsplitCount() > 0))
                    {
                        SplitComponents[i].CollapsedSplit = true;
                        SplitComponents[i].TopSplit = sectionList.Sections[sectionList.getSection(split)].startIndex;                      
                    }
                    else
                    {
                        SplitComponents[i].CollapsedSplit = false;
                    }
                }

                i++;
            }

            if (invalidator != null)
                InternalComponent.Update(invalidator, state, width, height, mode);
        }

        public void Dispose()
        {
        }
    }
}
