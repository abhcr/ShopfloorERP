using ShopfloorUI.ObjectCache;
using System;
using System.Windows.Media;

namespace ShopfloorUI.Models
{
    public class Process
    {
        public enum ProcessStatus
        {
            Waiting = 0,
            InProgress = 1,
            OnHold = 2,
            Completed = 3,
        }
        public enum ProcessType
        {
            ItemCollection = 0,
            Job = 1,
            QualityCheck = 2,
            Delivery = 3
        }
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Resource ExecutingResource { get; set; }
        public Employee ExecutingEmployee { get; set; }
        public string Rate { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public string DurationHours { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public ProcessStatus Status { get; set; }
        public Int64 ProjectId { get; set; }
        public ProcessType Type { get; set; }


        public string TypeDisplay
        {
            get => Type.ToString();
            set => Type = Enum.Parse<ProcessType>(value);
        }
        public string StatusDisplay
        {
            get => Status.ToString();
            set => Status = Enum.Parse<ProcessStatus>(value);
        }
        public string[] ProcessTypeOptions => Enum.GetNames(typeof(ProcessType));
        public string[] ProcessStatusOptions => Enum.GetNames(typeof(ProcessStatus));
        public DateTime? ProcessStartDateDisplay
        {
            get => StartDate?.LocalDateTime;
            set => StartDate = new DateTimeOffset(value.Value); //TODO TODO TODO
        }
        public DateTime? ProcessEndDateDisplay
        {
            get => EndDate?.LocalDateTime;
            set => EndDate = new DateTimeOffset(value.Value); //TODO TODO TODO
        }

        public long QNumber { get; set; }

        public SolidColorBrush ProcessStatusColor
        {
            get
            {
                switch (Status)
                {
                    //case ProcessStatus.Waiting:
                    //    return new SolidColorBrush(Color.FromRgb(0xd6, 0xd5, 0x7a));
                    //case ProcessStatus.InProgress:
                    //    return new SolidColorBrush(Color.FromRgb(0x7a, 0x80,0xc2));
                    //case ProcessStatus.OnHold:
                    //    return new SolidColorBrush(Color.FromRgb(0xc2, 0x7a, 0x7a));
                    //case ProcessStatus.Completed:
                    //    return new SolidColorBrush(Color.FromRgb(0x8b, 0xcc, 0x8b));
                    default:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        public string ProjectName
        {
            get
            {
                if(ProjectCache.GetInstance().GetById(ProjectId) != null)
                {
                    return ProjectCache.GetInstance().GetById(ProjectId).Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
