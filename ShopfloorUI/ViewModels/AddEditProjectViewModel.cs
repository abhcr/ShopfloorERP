using ShopfloorUI.Helpers;
using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace ShopfloorUI.ViewModels
{
    public class AddEditProjectViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Project Project { get; set; }
        public ObservableCollection<Process> Processes { get; set; }
        public Process SelectedProcess { get; set; }
        public bool IsProcessSelected => SelectedProcess != null;
        public List<Resource> Resources { get; set; }
        public List<Employee> Employees { get; set; }
        public string Title => Project?.Id == 0 ? "<New Project>" : $"{Project?.Id.ToString()}({Project?.Name})";

        internal void Refresh()
        {
            var projectId = Project?.Id;
            EmployeeCache.GetInstance().Clear();
            ResourceCache.GetInstance().Clear();
            ProcessCache.GetInstance().Clear();
            ProjectCache.GetInstance().Clear();
            Resources = ResourceCache.GetInstance().Resources;
            Resources = ResourceCache.GetInstance().UpdateAllQSizes();
            Employees = EmployeeCache.GetInstance().Employees;
            if (projectId.Value> 0)
            {
                Project = ProjectCache.GetInstance().GetById(projectId.Value);
                Processes = new ObservableCollection<Process>(Project.Processes);
            }
        }

        public Action<AddEditProjectViewModel> BackClickedHandler;
        public Command BackClickedCommand => new Command(() => BackClickedHandler?.Invoke(this));
        public Command AddProcessClickedCommand => new Command(() => AddProcessClickedHandler());
        public Command DeleteProcessClickedCommand => new Command(() => DeleteProcessClickHandler());
        public Command SaveClickedCommand => new Command(() => SaveClickHandler());
        public Command RefreshClickedCommand => new Command(() => Refresh());

        

        private void SaveClickHandler()
        {
            try
            {
                if(Project?.Customer == null)
                {
                    MessageBox.Show("Please select a customer!", "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                if (!DeadlineDisplay.HasValue)
                {
                    MessageBox.Show("Please enter a deadline date for this project!", "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                foreach (var process in Processes)
                {
                    if (!double.TryParse(process.DurationHours, out double result))
                    {
                        MessageBox.Show("Enter a valid duration for the process " + process.Name, "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    if (process.ExecutingEmployee == null)
                    {
                        MessageBox.Show("Select an employee for the process " + process.Name, "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    if (process.ExecutingResource == null)
                    {
                        MessageBox.Show("Select a resource for the process " + process.Name, "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                }
                if(Project.Id == 0)
                {
                    Project = ProjectCache.GetInstance().Insert(Project);
                }
                else
                {
                    ProjectCache.GetInstance().Update(Project);
                }
                var projectId = Project.Id;
                foreach (var process in Processes)
                {
                    process.ProjectId = projectId;
                    
                    if(process.Id == 0)
                    {
                        _ = ProcessCache.GetInstance().Insert(process);
                    }
                    else
                    {
                        ProcessCache.GetInstance().Update(process);
                    }
                }
                CommonFunctions.UpdateQs();
                BackClickedHandler?.Invoke(this);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void UpdateQs()
        //{
            
        //    //Set Q No. TO 0 if the process is completed
        //    foreach (var process in ProcessCache.GetInstance().Processes)
        //    {
        //        var q = ProcessQCache.GetInstance().GetByProcess(process);
        //        if(q == null)
        //        {
        //            //if no q entry, make a new entry.
        //            q = ProcessQCache.GetInstance().Insert(new ProcessQ
        //            {
        //                ProcessId = process.Id,
        //                QNumber = 0
        //            });
        //        }
        //        if (process.Status == Process.ProcessStatus.Completed)
        //        {
        //            q.QNumber = 0;
        //            ProcessQCache.GetInstance().Update(q);
        //        }
        //    }
        //    //For each resource,
        //    foreach (var resource in ResourceCache.GetInstance().Resources)
        //    {
        //        //get incomplete processes...
        //        var incompleteProcs = ProcessCache.GetInstance().GetByResourceId(resource.Id).FindAll(p => p.Status != Process.ProcessStatus.Completed).ToList<Process>();
        //        //order them ascending order of existing q numbers... if a ProcessQ isn't available, temporarily assign it a large value so that they get to the end of the q in next step
        //        var procsInQueueOrder = incompleteProcs.OrderBy(p => ProcessQCache.GetInstance().GetByProcess(p)?.QNumber ?? long.MaxValue);
        //        long index = 0;
        //        foreach (var process in procsInQueueOrder)
        //        {
        //            index++;
        //            var q = ProcessQCache.GetInstance().GetByProcess(process);
        //            if (q != null)
        //            {
        //                q.QNumber = index;
        //                ProcessQCache.GetInstance().Update(q);
        //            }
        //            else if (q == null)
        //            {
        //                ProcessQCache.GetInstance().Insert(
        //                    new ProcessQ
        //                    {
        //                        ProcessId = process.Id,
        //                        QNumber = index
        //                    });
        //            }
        //        }
        //    }
        //}

        private void DeleteProcessClickHandler()
        {
            //delete q number of this process first..
            ProcessQCache.GetInstance().Delete(ProcessQCache.GetInstance().GetByProcess(SelectedProcess));
            //now delete the process.
            ProcessCache.GetInstance().Delete(SelectedProcess);
            Processes.Remove(SelectedProcess);

            //Run the Q update logic now..
            CommonFunctions.UpdateQs();
            Resources = ResourceCache.GetInstance().UpdateAllQSizes();
        }

        private void AddProcessClickedHandler()
        {
            Processes.Add(new Process
            {
                Id = 0,
                Type = Process.ProcessType.Job,
                Status = Process.ProcessStatus.Waiting
            });
        }

        public List<Customer> Customers { get; set; }
        public string[] OrderStatusOptions => Enum.GetNames(typeof(Project.ProjectOrderStatus));
        public string ProjectOrderStatus
        {
            get => Project.OrderStatus.ToString();
            set  
            {
                Project.OrderStatus = Enum.Parse<Project.ProjectOrderStatus>(value);
            }
        }
        public DateTime? ProjectStartDate
        {
            get => Project.StartDate?.LocalDateTime;
            set
            {
                Project.StartDate = new DateTimeOffset(value.Value);    //TODO TODO TODO
            }
        }
        public DateTime? ProjectPoDate
        {
            get => Project.PoDate?.LocalDateTime;
            set => Project.PoDate = new DateTimeOffset(value.Value); //TODO TODO TODO
        }

        public DateTime? DeadlineDisplay
        {
            get => Project.Deadline?.LocalDateTime;
            set => Project.Deadline = new DateTimeOffset(value.Value);
        }
    }
}
