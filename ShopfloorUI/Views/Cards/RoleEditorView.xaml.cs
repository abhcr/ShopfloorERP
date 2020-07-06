using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ShopfloorUI.Views.Cards
{
    /// <summary>
    /// Interaction logic for RoleEditorView.xaml
    /// </summary>
    public partial class RoleEditorView : UserControl
    {
        public ObservableCollection<EmployeeRole> Roles;
        private bool justSaved = false;
        public RoleEditorView()
        {
            InitializeComponent();
            Refresh();
        }
        public void Refresh()
        {
            RoleCache.GetInstance().Clear();
            Roles = new ObservableCollection<EmployeeRole>(RoleCache.GetInstance().Roles);
            ListViewRoles.ItemsSource = Roles;
            ListViewRoles.SelectedItem = GetFirstRole();
        }
        private EmployeeRole GetFirstRole()
        {
            return Roles.Count == 0
                ? new EmployeeRole { Id = 0, Name = string.Empty, Description = string.Empty }
                : Roles[0];
        }
        private void ListViewRoles_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //var vm = (OrganizationViewModel)DataContext;
                if (e.RemovedItems.Count > 0)
                {
                    var unselectedItem = e.RemovedItems?[0] as EmployeeRole;
                    //if the unselected item is not given any name and was a new addition , remove it from the list.
                    if (unselectedItem?.Id == 0 && TextboxRoleName.Text?.Length == 0)
                    {
                        Roles.Remove(unselectedItem);
                    }
                    else if (justSaved)
                    {
                        //nothing to do here, except setting the flag off
                        justSaved = false;
                    }
                    else
                    {
                        if (unselectedItem?.Name != TextboxRoleName.Text
                            || unselectedItem?.Description != TextboxRoleDescription.Text)
                        {
                            var response = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (response == MessageBoxResult.Yes)
                            {
                                SaveRole(new EmployeeRole { Id = unselectedItem.Id, Name = TextboxRoleName.Text, Description = TextboxRoleDescription.Text });
                            }
                            else
                            {
                                if (unselectedItem.Id == 0)
                                {
                                    Roles.Remove(unselectedItem);
                                }
                            }
                        }
                    }
                }
                ListViewRoles.ItemsSource = Roles;
                if (ListViewRoles.SelectedItem != null)
                {
                    TextboxRoleName.Text = ((EmployeeRole)ListViewRoles.SelectedItem).Name;
                    TextboxRoleDescription.Text = ((EmployeeRole)ListViewRoles.SelectedItem).Description;
                    TextblockSelectedName.Text = ((EmployeeRole)ListViewRoles.SelectedItem).Name;
                }
                else
                {
                    ListViewRoles.SelectedItem = GetFirstRole();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonAddNewRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Roles.Add(new EmployeeRole { Id = 0, Name = string.Empty, Description = string.Empty });
                ListViewRoles.ItemsSource = Roles;
                ListViewRoles.SelectedItem = Roles[Roles.Count - 1];
                TextboxRoleName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonDeleteRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var vm = (OrganizationViewModel)DataContext;
                DeleteSelectedRole();
                ListViewRoles.ItemsSource = Roles;
                ListViewRoles.SelectedItem = Roles.Count > 0 ? Roles[0] : new EmployeeRole { Id = 0, Name = string.Empty, Description = string.Empty };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteSelectedRole()
        {
            RoleCache.GetInstance().DeleteRole(ListViewRoles.SelectedItem as EmployeeRole);
            Roles = new ObservableCollection<EmployeeRole>(RoleCache.GetInstance().Roles);
        }
        private void ButtonSaveRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (ListViewRoles.SelectedItem == null)
                    return;
                //var vm = (OrganizationViewModel)DataContext;
                SaveRole(new EmployeeRole { Id = ((EmployeeRole)ListViewRoles.SelectedItem).Id, Name = TextboxRoleName.Text, Description = TextboxRoleDescription.Text });
                justSaved = true;
                ListViewRoles.ItemsSource = Roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveRole(EmployeeRole roleToSave)
        {
            if (roleToSave.Id > 0)
            {
                RoleCache.GetInstance().UpdateRole(roleToSave);
            }
            else
            {
                RoleCache.GetInstance().InsertRole(roleToSave);
            }
            Roles = new ObservableCollection<EmployeeRole>(RoleCache.GetInstance().Roles);
        }
    }
}
