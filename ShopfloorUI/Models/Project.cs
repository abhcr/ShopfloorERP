using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace ShopfloorUI.Models
{
    public class Project
    {

        public enum ProjectOrderStatus
        {
            WaitingQuote=0,
            WaitingOrder=1,
            OrderConfirmed = 2,
            DeliveryAcknowledged=3
        }
        public enum ProjectProgressStatus
        {
            NotStarted = 0,
            WaitingCollection = 1,
            WorkInProgress = 2,     //internal processes, and outsourced processes
            WaitingInspection = 3,  //qc
            ReadyToDeliver = 4,
            Completed = 5,
            OnHold = 6
        }
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public Decimal Quantity { get; set; }
        public string Description { get; set; }
        public Customer Customer { get; set; }
        public string QuoteNumber { get; set; }
        public string PoNumber { get; set; }
        public DateTimeOffset? PoDate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? Deadline { get; set; }
        public List<Process> Processes { get; set; }
        public int? TotalProcesses => Processes?.Count;
        public int? CompletedProcesses => Processes.FindAll(p => { return p.Status == Process.ProcessStatus.Completed; }).Count;
        public ProjectOrderStatus OrderStatus { get;set; }
        public string ProgressNumber => $"{CompletedProcesses.ToString()}/{TotalProcesses.ToString()}";
        public ProjectProgressStatus ProgressStatus
        {
            get
            {
                if (OrderStatus == ProjectOrderStatus.WaitingQuote
                    || OrderStatus == ProjectOrderStatus.WaitingOrder
                    || Processes.Count == 0)
                {
                    return ProjectProgressStatus.NotStarted;
                }
                foreach (var process in Processes)
                {
                    if (process.Type == Process.ProcessType.ItemCollection)
                    {
                        if (process.Status == Process.ProcessStatus.OnHold)
                        {
                            return ProjectProgressStatus.OnHold;
                        }
                        else if (process.Status != Process.ProcessStatus.Completed)
                        {
                            return ProjectProgressStatus.WaitingCollection;
                        }
                    }
                    else if (process.Type == Process.ProcessType.Job)
                    {
                        if (process.Status == Process.ProcessStatus.OnHold)
                        {
                            return ProjectProgressStatus.OnHold;
                        }
                        else if (process.Status != Process.ProcessStatus.Completed)
                        {
                            return ProjectProgressStatus.WorkInProgress;
                        }
                    }
                    else if(process.Type == Process.ProcessType.QualityCheck)
                    {
                        if (process.Status == Process.ProcessStatus.OnHold)
                        {
                            return ProjectProgressStatus.OnHold;
                        }
                        else if (process.Status != Process.ProcessStatus.Completed)
                        {
                            return ProjectProgressStatus.WaitingInspection;
                        }
                    }
                    else if(process.Type == Process.ProcessType.Delivery)
                    {
                        if (process.Status == Process.ProcessStatus.OnHold)
                        {
                            return ProjectProgressStatus.OnHold;
                        }
                        else if (process.Status != Process.ProcessStatus.Completed)
                        {
                            return ProjectProgressStatus.ReadyToDeliver;
                        }
                        else
                        {
                            return ProjectProgressStatus.Completed;
                        }
                    }
                }
                return ProjectProgressStatus.Completed;
            }
            //get
            //{
            //    if(OrderStatus == ProjectOrderStatus.WaitingQuote
            //        || OrderStatus == ProjectOrderStatus.WaitingOrder)
            //    {
            //        return ProjectProgressStatus.NotStarted;
            //    }
            //    var collectionProcesses = GetCollectionProcesses();
            //    if (collectionProcesses.Count > 0)
            //    {
            //        foreach (var process in collectionProcesses)
            //        {
            //            if(process.Status != Process.ProcessStatus.Completed)
            //            {
            //                //if any of the collection process are in waiting, set project status to waiting materials.
            //                return ProjectProgressStatus.WaitingCollection;
            //            }
            //        }
            //    }

            //    var jobProcesses = GetJobProcesses();
            //    if (jobProcesses.Count > 0)
            //    {
            //        foreach (var process in jobProcesses)
            //        {
            //            if(process.Status != Process.ProcessStatus.Completed)
            //            {
            //                return ProjectProgressStatus.WorkInProgress;
            //            }
            //        }
            //    }
            //    var qcProcesses = GetQCProcesses();
            //    var deliveryProcesses = GetDeliveryProcesses();
            //    if(qcProcesses.Count > 0)
            //    {
            //        foreach (var process in qcProcesses)
            //        {
            //            if(process.Status != Process.ProcessStatus.Completed)
            //            {
            //                return ProjectProgressStatus.WaitingInspection;
            //            }
            //        }
            //        if(deliveryProcesses.Count == 0)
            //        {
            //            return ProjectProgressStatus.Completed;
            //        }
            //    }
            //    if(deliveryProcesses.Count > 0)
            //    {
            //        foreach (var process in deliveryProcesses)
            //        {
            //            if(process.Status != Process.ProcessStatus.Completed)
            //            {
            //                return ProjectProgressStatus.ReadyToDeliver;
            //            }
            //        }
            //        return ProjectProgressStatus.Completed;
            //    }
            //    return ProjectProgressStatus.NotStarted;
            //}
        }
        public string DeadlineDisplay => Deadline?.LocalDateTime.ToString("dd/MMM/yy");

        public List<Process> GetCollectionProcesses()
        {
            return Processes?.FindAll(p => { return p.Type == Process.ProcessType.ItemCollection; });
        }
        public List<Process> GetJobProcesses()
        {
            return Processes?.FindAll(p => { return p.Type == Process.ProcessType.Job; });
        }
        public List<Process> GetQCProcesses()
        {
            return Processes?.FindAll(p => { return p.Type == Process.ProcessType.QualityCheck; });
        }
        public List<Process> GetDeliveryProcesses()
        {
            return Processes?.FindAll(p => { return p.Type == Process.ProcessType.Delivery; });
        }
        public bool IsOverdue =>  (ProgressStatus != ProjectProgressStatus.Completed) && (Deadline < DateTimeOffset.UtcNow);
        public bool IsApproachingDelivery => (ProgressStatus != ProjectProgressStatus.Completed) && (Deadline < DateTimeOffset.UtcNow.AddDays(4));  //4 days for delivery
        public SolidColorBrush StatusColor
        {
            get
            {
                if (IsOverdue)
                    return new SolidColorBrush(Colors.Red);
                else if (ProgressStatus == ProjectProgressStatus.OnHold)
                    return new SolidColorBrush(Colors.Yellow);
                else if (IsApproachingDelivery)
                    return new SolidColorBrush(Colors.Orange);
                else if (ProgressStatus == ProjectProgressStatus.Completed)
                    return new SolidColorBrush(Colors.Green);
                else
                    return new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
