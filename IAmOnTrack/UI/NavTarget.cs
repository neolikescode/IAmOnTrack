using System;

namespace IAmOnTrack.UI
{
    //Sidebar destinations
    public enum NavTarget
    {
        Dashboard,
        Modules,
        Assessments,
        Marks
    }

    public class NavigationEventArgs : EventArgs
    {
        public NavTarget Target { get; private set; }

        public NavigationEventArgs(NavTarget target)
        {
            Target = target;
        }
    }
}
