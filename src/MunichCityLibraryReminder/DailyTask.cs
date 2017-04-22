namespace MunichCityLibraryReminder
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using TaskSchedulerEngine;

    public class DailyTask : ITask
    {
        public void HandleConditionsMetEvent(object sender, ConditionsMetEventArgs e)
        {
            // Daily Task
            Logger.Write("Daily Task:" + DateTime.Now.ToLongTimeString());
        }

        public void Initialize(ScheduleDefinition schedule, object parameters)
        {
            // Daily Task Initialization
        }
    }
}
