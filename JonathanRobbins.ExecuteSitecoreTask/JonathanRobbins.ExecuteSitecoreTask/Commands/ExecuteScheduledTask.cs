using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Data.Items;
using Sitecore.Jobs;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Tasks;

namespace JonathanRobbins.ExecuteSitecoreTask.Commands
{
    class ExecuteScheduledTask : Command
    {
        public override void Execute(CommandContext context)
        {
            Item selectedItem = context.Items.FirstOrDefault();
            if (selectedItem == null)
                return;

            if (!selectedItem.TemplateID.Equals(Sitecore.TemplateIDs.Schedule))
                return;

            var scheduleItem = new ScheduleItem(selectedItem);
            if (scheduleItem == null)
                return;

            string type = scheduleItem.CommandItem[Sitecore.Configuration.Settings.GetSetting("TypeFieldName")];
            if (string.IsNullOrEmpty(type))
                throw new Exception("Unable to find the type defined in the Type field of the command item");
            string method = scheduleItem.CommandItem[Sitecore.Configuration.Settings.GetSetting("MethodFieldName")];
            if (string.IsNullOrEmpty(method))
                throw new Exception("Unable to find the method defined in the Method field of the command item");

            Type definedType = Type.GetType(type);
            if (definedType == null)
                throw new Exception("Unable to find the type defined by the command item");

            object obj = Activator.CreateInstance(definedType);

            var options = new JobOptions("Job Runner", "schedule", "scheduler", obj, method, new object[] { scheduleItem.Items, scheduleItem.CommandItem, scheduleItem });
            options.SiteName = "scheduler";
            JobManager.Start(options).WaitHandle.WaitOne();

            //TODO report execution
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (context == null)
                return CommandState.Hidden;

            if (!context.Items.Any())
                return CommandState.Hidden;

            Item item = context.Items[0];

            if (!item.TemplateID.Equals(Sitecore.TemplateIDs.Schedule))
                return CommandState.Hidden;

            var scheduleItem = new ScheduleItem(item);
            if (scheduleItem != null)
                return CommandState.Enabled;

            return base.QueryState(context);
        }
    }
}
