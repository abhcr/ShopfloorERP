using Microsoft.Win32;
using ShopfloorUI.Helpers;
using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using ShopfloorUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopfloorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            //activeDate.SelectedDate = DateTimeOffset.UtcNow.LocalDateTime;
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select Shopfloor Data";
            fileDialog.Multiselect = false;
            fileDialog.Filter = "ShopfloorData(*.db)|*.db";
            fileDialog.FileOk += FileDialog_FileOk;
            fileDialog.ShowDialog();
            if (!fileSelected)
            {
                Application.Current.Shutdown();
            }
            else
            {
                //update db to include new columns or tables..
                _ = CommonFunctions.InsertNewColumns();
                ButtonDashboard_Click(this, new RoutedEventArgs());
            }
        }
        private bool fileSelected;
        private void FileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var name = ((OpenFileDialog)sender).FileName;
                if (string.IsNullOrEmpty(name))
                    return;
                DBFunctions.GetInstance().SetConnectionString(name);
                fileSelected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void activeDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GridTitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonManageProjects_Click(object sender, RoutedEventArgs e)
        {
            TextblockPanelName.Text = "Manage Projects";
            DataContext = new ProjectsViewModel
            {
                ProjectsToDisplay = new ObservableCollection<Project>(ProjectCache.GetInstance().Projects),
                NewProjectClickHandler = ProjectViewNewProjectClickHandler,
                ProjectOpenClickHandler =  ProjectViewOpenProectClickHandler
            };
        }

        private void ProjectViewOpenProectClickHandler(ProjectsViewModel viewModel)
        {
            TextblockPanelName.Text = "Project Details";
            RoleCache.GetInstance().Clear();
            EmployeeCache.GetInstance().Clear();
            ResourceCache.GetInstance().Clear();
            CustomerCache.GetInstance().Clear();
            ProcessCache.GetInstance().Clear();
            ProjectCache.GetInstance().Clear();
            var resources = ResourceCache.GetInstance().Resources;
            resources = ResourceCache.GetInstance().UpdateAllQSizes();

            DataContext = new AddEditProjectViewModel
            {
                Project = viewModel.SelectedProject,
                BackClickedHandler = AddEditProjectViewBackClickHandler,
                Customers = CustomerCache.GetInstance().Customers,
                Processes = new ObservableCollection<Process>(viewModel.SelectedProject.Processes),
                Resources = resources.OrderByDescending(r => r.QSize).ToList<Resource>(),
                Employees = EmployeeCache.GetInstance().Employees
            };
        }

        private void ProjectViewNewProjectClickHandler(ProjectsViewModel viewModel)
        {
            TextblockPanelName.Text = "Create New Project";

            RoleCache.GetInstance().Clear();
            EmployeeCache.GetInstance().Clear();
            ResourceCache.GetInstance().Clear();
            CustomerCache.GetInstance().Clear();
            ProcessCache.GetInstance().Clear();
            ProjectCache.GetInstance().Clear();
            var resources = ResourceCache.GetInstance().Resources;
            resources = ResourceCache.GetInstance().UpdateAllQSizes();

            DataContext = new AddEditProjectViewModel
            {
                Project = new Project
                {
                    Id = 0,
                    Processes = new List<Process>(),
                    StartDate = DateTimeOffset.UtcNow,
                    PoDate = DateTimeOffset.UtcNow,
                    Quantity = 1,
                    OrderStatus = Project.ProjectOrderStatus.WaitingQuote,
                },
                BackClickedHandler = AddEditProjectViewBackClickHandler,
                Customers = CustomerCache.GetInstance().Customers,
                Processes = new ObservableCollection<Process>(new List<Process>()),
                Resources = resources.OrderByDescending(r => r.QSize).ToList<Resource>(),
                Employees = EmployeeCache.GetInstance().Employees
            };
        }

        private void AddEditProjectViewBackClickHandler(AddEditProjectViewModel vm)
        {
            DataContext = null;
            ButtonManageProjects_Click(null, null);
        }

        private void ButtonDashboard_Click(object sender, RoutedEventArgs e)
        {
            TextblockPanelName.Text = "Dashboard";

            RoleCache.GetInstance().Clear();
            EmployeeCache.GetInstance().Clear();
            ResourceCache.GetInstance().Clear();
            CustomerCache.GetInstance().Clear();
            ProcessCache.GetInstance().Clear();
            ProjectCache.GetInstance().Clear();

            List<Resource> recs = ResourceCache.GetInstance().Resources;
            recs = ResourceCache.GetInstance().UpdateAllQSizes();
            recs = recs.OrderByDescending(r => r.QSize).ToList<Resource>();
            var emps = EmployeeCache.GetInstance().Employees;
            emps = EmployeeCache.GetInstance().UpdateAllQSizes();
            emps = emps.OrderByDescending(e => e.QSize).ToList<Employee>();
            DataContext = new DashboardViewModel
            {
                Resources = new ObservableCollection<Resource>(recs),
                SelectedResource = recs[0],
                Employees = new ObservableCollection<Employee>(emps),
                SelectedEmployee = emps[0]
            };
        }

        private void ButtonOrganization_Click(object sender, RoutedEventArgs e)
        {
            TextblockPanelName.Text = "Organization";
            DataContext = new OrganizationViewModel();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
