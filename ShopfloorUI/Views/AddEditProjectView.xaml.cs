using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using ShopfloorUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopfloorUI.Views
{
    /// <summary>
    /// Interaction logic for AddEditProjectView.xaml
    /// </summary>
    public partial class AddEditProjectView : UserControl
    {
        public AddEditProjectView()
        {
            InitializeComponent();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var vm = DataContext as AddEditProjectViewModel;
            vm.Refresh();
        }

        private void ResourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as AddEditProjectViewModel;
            var combobox = sender as ComboBox;
            Grid containingRow = combobox.Parent as Grid;
            var process = containingRow.DataContext as Process;
            //var index = vm.Processes.IndexOf(process);
            //reset the q number of the process when resource is changed. A new Q Number will be given while saving (as the last in the q of the new resource selected)
            if (process.Id != 0)
            {
                //for new processes, (with id = 0), it won;t hve a processQ, so do this only for existing processes.
                //new processes will get a new processQ when the project is saved.
                ProcessQCache.GetInstance().GetByProcess(process).QNumber = 0;
                //update processQ of this process object
                ProcessQCache.GetInstance().Update(process);
            }
            //vm.Processes[index] = process;
        }
    }
}
