using ShopfloorUI.Helpers;
using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace ShopfloorUI.ViewModels
{
    public class ProjectsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string[] FilterOptions => new string[] { "All", "Pending", "Completed" }; //, "Waiting Quote", "Waiting LPO", "Waiting Materials", "In Progress", "Waiting QC", "Ready to Deliver"};
        private string _selectedFilterOption;
        public string SelectedFilterOption
        {
            get => _selectedFilterOption;
            set
            {
                _selectedFilterOption = value;
                Refresh();
            }
        }
        public Action<ProjectsViewModel> NewProjectClickHandler;
        public Action<ProjectsViewModel> ProjectOpenClickHandler;

        public ObservableCollection<Project> ProjectsToDisplay { get; set; }
        public Project SelectedProject { get; set; }
        public bool IsProjectSelected => SelectedProject != null;
        public Command RefreshClickedCommand => new Command(() => Refresh());
        public Command NewProjectClickedCommand => new Command(() => NewProjectClickHandler?.Invoke(this)); // { get; set; } //=> new Command(() => AddNewProjectClickHandler());
        public Command ProjectOpenClickedCommand => new Command(() => ProjectOpenClickHandler?.Invoke(this));
        public Command DeleteProjectClickedCommand => new Command(() => DeleteProjectClickHandler());

        private void DeleteProjectClickHandler()
        {
            if (this.SelectedProject != null)
            {
                if (MessageBox.Show("Delete the project " + this.SelectedProject?.Name + " and it's processes?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if(this.SelectedProject.ProgressStatus != Project.ProjectProgressStatus.NotStarted)
                    {
                        MessageBox.Show("This project has already started and cannot be deleted.", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }
                    foreach (var process in ProcessCache.GetInstance().GetByProjectId(this.SelectedProject.Id))
                    {
                        ProcessCache.GetInstance().Delete(process);
                    }
                    ProjectCache.GetInstance().Delete(this.SelectedProject);
                    CommonFunctions.UpdateQs();
                    Refresh();
                }
            }
        }


        public ProjectsViewModel()
        {
            SelectedFilterOption = FilterOptions[1];    //default filter selection
        }

        public void Refresh()
        {
            ProjectCache.GetInstance().Clear();
            switch (_selectedFilterOption)
            {
                case "Pending":
                    ProjectsToDisplay = new ObservableCollection<Project>(ProjectCache.GetInstance().GetProjectsPending());
                    break;
                case "Completed":
                    ProjectsToDisplay = new ObservableCollection<Project>(ProjectCache.GetInstance().GetProjectsCompleted());
                    break;
                default:
                    ProjectsToDisplay = new ObservableCollection<Project>(ProjectCache.GetInstance().Projects);
                    break;
            }
        }
    }
}
