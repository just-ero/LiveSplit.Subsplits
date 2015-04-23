using LiveSplit.Model;
using System;

namespace LiveSplit.UI.Components
{
    public class SplitsComponentFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Subsplits"; }
        }

        public string Description
        {
            get { return "Displays a list of split times and deltas in relation to a comparison. Only shows subsplits when relevant"; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.List; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new SplitsComponent(state);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
            get { return "http://livesplit.org/update_rc_sdhjdop/Components/update.LiveSplit.Subsplits.xml"; }
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.Subsplits.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
            get { return "http://livesplit.org/update_rc_sdhjdop/"; }
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("1.5.2"); }
        }
    }
}
