using ShopfloorUI.Helpers;
using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using ShopfloorUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for OrganizationView.xaml
    /// </summary>
    public partial class OrganizationView : UserControl
    {
        
        public OrganizationView()
        {
            InitializeComponent();
            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                switch (TabControlEditorWindows.SelectedIndex)
                {
                    case 0:
                        rolesView?.Refresh();
                        break;
                    case 1:
                        employeeView?.Refresh();
                        break;
                    case 2:
                        resourceView?.Refresh();
                        break;
                    //case 3:
                    //    rawMaterialView?.Refresh();
                    //    break;
                    case 3://4:
                        customerView?.Refresh();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
