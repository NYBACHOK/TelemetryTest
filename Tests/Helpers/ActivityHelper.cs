using System.Diagnostics;
using System.Reflection;

namespace Tests.Helpers
{
    public static class ActivityHelper
    {
        private static ActivitySource _activitySource;
        private static ActivityKind _activityKind;

        public static string SourceName { get; }

        static ActivityHelper()
        {
            string sourceName = Assembly.GetCallingAssembly().GetName().Name;

            SourceName = sourceName;
            _activitySource = new ActivitySource(sourceName);
            _activityKind = ActivityKind.Server;
        }

        /// <summary>
        /// Create started activity. If no ActivityListeners presented - returns null.
        /// </summary>
        /// <param name="activityName"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static Activity StartActivity(string activityName)
        {
            var activity = _activitySource.StartActivity(activityName, _activityKind);

            if (activity != null)
                return activity;

            //Add some code to alert that no ActivityListeners is presented
            return activity;
        }

    }
}
