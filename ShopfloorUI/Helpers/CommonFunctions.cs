using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopfloorUI.Helpers
{
    public static class CommonFunctions
    {
        public static bool InsertNewColumns()
        {
            string deadlineDateColumn = "ALTER TABLE Projects ADD COLUMN DeadlineDate TEXT";
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand(deadlineDateColumn);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static void UpdateQs()
        {

            //Set Q No. TO 0 if the process is completed
            foreach (var process in ProcessCache.GetInstance().Processes)
            {
                var q = ProcessQCache.GetInstance().GetByProcess(process);
                if (q == null)
                {
                    //if no q entry, make a new entry.
                    q = ProcessQCache.GetInstance().Insert(new ProcessQ
                    {
                        ProcessId = process.Id,
                        QNumber = 0
                    });
                }
                if (process.Status == Process.ProcessStatus.Completed)
                {
                    q.QNumber = 0;
                    ProcessQCache.GetInstance().Update(q);
                }
            }
            //For each resource,
            foreach (var resource in ResourceCache.GetInstance().Resources)
            {
                //get incomplete processes...
                var incompleteProcs = ProcessCache.GetInstance().GetByResourceId(resource.Id).FindAll(p => p.Status != Process.ProcessStatus.Completed).ToList<Process>();
                //order them ascending order of existing q numbers... if a ProcessQ isn't available, temporarily assign it a large value so that they get to the end of the q in next step
                var procsInQueueOrder = incompleteProcs.OrderBy(p => ProcessQCache.GetInstance().GetByProcess(p)?.QNumber ?? long.MaxValue);
                long index = 0;
                foreach (var process in procsInQueueOrder)
                {
                    index++;
                    var q = ProcessQCache.GetInstance().GetByProcess(process);
                    if (q != null)
                    {
                        q.QNumber = index;
                        ProcessQCache.GetInstance().Update(q);
                    }
                    else if (q == null)
                    {
                        ProcessQCache.GetInstance().Insert(
                            new ProcessQ
                            {
                                ProcessId = process.Id,
                                QNumber = index
                            });
                    }
                }
            }
        }
    }
}
