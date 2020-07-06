using ShopfloorUI.Helpers;
using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace ShopfloorUI.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
     
        public ObservableCollection<Resource> Resources { get; set; }
        public Resource SelectedResource { get; set; }
        public ObservableCollection<Process> Processes => new ObservableCollection<Process>(ProcessCache.GetInstance()
            .GetByResourceId(SelectedResource?.Id ?? ResourceCache.GetInstance().Resources[0].Id)   //get all processes for the selected resource
            .FindAll(p => p.Status != Process.ProcessStatus.Completed)                              //select all incomplete processes
            .OrderBy(p => ProcessQCache.GetInstance().GetByProcess(p)?.QNumber ?? 0).ToList<Process>());    //order by the Qnumber in the ProcessQ cache
        public Process SelectedProcess { get; set; }

        public ObservableCollection<Employee> Employees { get; set; }
        public Employee SelectedEmployee { get; set; }
        public ObservableCollection<Process> EmployeeProcesses => new ObservableCollection<Process>(ProcessCache.GetInstance()
            .GetByEmployeeId(SelectedEmployee?.Id ?? EmployeeCache.GetInstance().Employees[0].Id)
            .FindAll(p => p.Status != Process.ProcessStatus.Completed)
            .OrderBy(p => ProcessQCache.GetInstance().GetByProcess(p)?.QNumber ?? 0).ToList<Process>());
        public Process SelectedEmployeeProcess { get; set; }
        
        public bool IsUpButtonEnabled
            => SelectedProcess == null
            ? false
            : Processes.IndexOf(SelectedProcess) == 0
                ? false
                : true;
        public bool IsDownButtonEnabled
            => SelectedProcess == null
            ? false
            : Processes.IndexOf(SelectedProcess) == Processes.Count - 1
                ? false
                : true;
        public Command SaveClickedCommand => new Command(() => SaveClickHandler());
        public Command EmployeeSaveClickedCommand => new Command(() => EmployeeSaveClickeHandler());
        public Command UpClickedCommand => new Command(() => UpClickHandler());
        public Command DownClickedCommand => new Command(() => DownClickHandler());
        public Command RefreshClickedCommand => new Command(() => Refresh());

        
        private void Refresh()
        {
            int selectedResourceIndex = Resources.IndexOf(SelectedResource);
            int selectedEmployeeIndex = Employees.IndexOf(SelectedEmployee);
            SelectedResource = null;
            SelectedEmployee = null;
            Thread.Sleep(20);
            _ = ResourceCache.GetInstance().Resources;
            Resources = new ObservableCollection<Resource>(ResourceCache.GetInstance().UpdateAllQSizes().OrderByDescending(r => r.QSize));
            SelectedResource = Resources[selectedResourceIndex];
            _ = EmployeeCache.GetInstance().Employees;
            Employees = new ObservableCollection<Employee>(EmployeeCache.GetInstance().UpdateAllQSizes().OrderByDescending(e => e.QSize));
            SelectedEmployee = Employees[selectedEmployeeIndex];
        }
        private void DownClickHandler()
        {
            var selectedIndex = Processes.IndexOf(SelectedProcess);
            //step 1: Identify the process just below the selected process.
            var processBelow = Processes[Processes.IndexOf(SelectedProcess) + 1];
            //step 2: increase q number of selected item.
            var selectedQ = ProcessQCache.GetInstance().GetByProcess(SelectedProcess);
            selectedQ.QNumber = selectedQ.QNumber + 1;
            ProcessQCache.GetInstance().Update(selectedQ);
            //step 3: Reduce q number of item below.
            var aboveQ = ProcessQCache.GetInstance().GetByProcess(processBelow);
            aboveQ.QNumber = aboveQ.QNumber - 1;
            ProcessQCache.GetInstance().Update(aboveQ);
            //step 4. Refresh.
            Refresh();
            SelectedProcess = Processes[selectedIndex + 1]; //selected the item moved down.
        }

        private void UpClickHandler()
        {
            var selectedIndex = Processes.IndexOf(SelectedProcess);
            //step 1: Identify the process just above the selected process.
            var processAbove = Processes[Processes.IndexOf(SelectedProcess) - 1];
            //step 2: reduce q number of selected item.
            var selectedQ = ProcessQCache.GetInstance().GetByProcess(SelectedProcess);
            selectedQ.QNumber = selectedQ.QNumber - 1;
            ProcessQCache.GetInstance().Update(selectedQ);
            //step 3: increase q number of item above.
            var aboveQ = ProcessQCache.GetInstance().GetByProcess(processAbove);
            aboveQ.QNumber = aboveQ.QNumber + 1;
            ProcessQCache.GetInstance().Update(aboveQ);
            //step 4. Refresh.
            Refresh();
            SelectedProcess = Processes[selectedIndex - 1]; //select the item moved up
        }

        //private void UpdateQs()
        //{

        //    //Set Q No. TO 0 if the process is completed
        //    foreach (var process in ProcessCache.GetInstance().Processes)
        //    {
        //        var q = ProcessQCache.GetInstance().GetByProcess(process);
        //        if (q == null)
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


        private void SaveClickHandler()
        {
            foreach (var process in ProcessCache.GetInstance().GetByResourceId(SelectedResource?.Id ?? 0))
            {
                ProcessCache.GetInstance().Update(process);
            }
            CommonFunctions.UpdateQs();
            ResourceCache.GetInstance().UpdateAllQSizes();
            EmployeeCache.GetInstance().UpdateAllQSizes();
            Refresh();
        }

        private void EmployeeSaveClickeHandler()
        {
            foreach (var process in ProcessCache.GetInstance().GetByEmployeeId(SelectedEmployee?.Id ?? 0))
            {
                ProcessCache.GetInstance().Update(process);
            }
            CommonFunctions.UpdateQs();
            ResourceCache.GetInstance().UpdateAllQSizes();
            EmployeeCache.GetInstance().UpdateAllQSizes();
            Refresh();
        }

    }
}
