using ShopfloorUI.Models;
using ShopfloorUI.ObjectCache;
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

namespace ShopfloorUI.Views.Cards
{
    /// <summary>
    /// Interaction logic for EmployeeEditorView.xaml
    /// </summary>
    public partial class EmployeeEditorView : UserControl
    {
        ObservableCollection<Employee> Employees;
        ObservableCollection<EmployeeRole> Roles;
        private bool justSaved;
        public EmployeeEditorView()
        {
            InitializeComponent();
            Refresh();
        }
        public void Refresh()
        {
            EmployeeCache.GetInstance().Clear();
            RoleCache.GetInstance().Clear();
            Roles = new ObservableCollection<EmployeeRole>(RoleCache.GetInstance().Roles);
            Employees = new ObservableCollection<Employee>(EmployeeCache.GetInstance().Employees);
            ComboboxEmpRole.ItemsSource = Roles;
            ListViewEmployees.ItemsSource = Employees;
            ListViewEmployees.SelectedItem = GetFirstEmployee();
            ComboboxEmpRole.SelectedItem = ((Employee)ListViewEmployees.SelectedItem).Role;
        }

        private object GetFirstEmployee()
        {
            return Employees.Count == 0
                ? new Employee { Id = 0, Name = string.Empty, Address = string.Empty, Phone = string.Empty, Role = GetFirstRole() }
                : Employees[0];
        }

        private EmployeeRole GetFirstRole()
        {
            return Roles.Count == 0
                    ? new EmployeeRole { Id = 0, Name = string.Empty, Description = string.Empty }
                    : Roles[0];
        }

        private void ButtonAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Employees.Add(new Employee { Id = 0, Name = string.Empty, Address = string.Empty, Phone = string.Empty, Role = GetFirstRole() });
                ListViewEmployees.ItemsSource = Employees;
                ListViewEmployees.SelectedItem = Employees[Employees.Count - 1];
                TextboxEmpName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSelectedEmployee();
                ListViewEmployees.ItemsSource = Employees;
                ListViewEmployees.SelectedItem = GetFirstEmployee();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedEmployee()
        {
            EmployeeCache.GetInstance().Delete(ListViewEmployees.SelectedItem as Employee);
            Employees = new ObservableCollection<Employee>(EmployeeCache.GetInstance().Employees);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ListViewEmployees.SelectedItem == null)
                    return;
                SaveEmployee(new Employee
                {
                    Id = (ListViewEmployees.SelectedItem as Employee).Id,
                    Name = TextboxEmpName.Text,
                    Address = TextboxEmpAddress.Text,
                    Phone = TextboxEmpPhone.Text,
                    Role = ComboboxEmpRole.SelectedItem as EmployeeRole
                });
                justSaved = true;
                ListViewEmployees.ItemsSource = Employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListViewEmployees_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                {
                    var unselectedItem = e.RemovedItems?[0] as Employee;
                    if(unselectedItem == null)
                    {
                        return;
                    }
                    if(ComboboxEmpRole.SelectedItem == null)
                    {
                        ComboboxEmpRole.SelectedItem = GetFirstRole();
                        return;
                    }
                    //if the unselected item is not given any name and was a new addition , remove it from the list.
                    if (unselectedItem?.Id == 0 && TextboxEmpName.Text?.Length == 0)
                    {
                        Employees.Remove(unselectedItem);
                    }
                    else if (justSaved)
                    {
                        //nothing to do here, except setting the flag off
                        justSaved = false;
                    }
                    else
                    {
                        if (unselectedItem?.Name != TextboxEmpName.Text
                            || unselectedItem?.Address != TextboxEmpAddress.Text
                            || unselectedItem?.Phone != TextboxEmpPhone.Text
                            || unselectedItem?.Role?.Id != (ComboboxEmpRole.SelectedItem as EmployeeRole)?.Id)
                        {
                            var response = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (response == MessageBoxResult.Yes)
                            {
                                SaveEmployee(new Employee
                                {
                                    Id = unselectedItem.Id,
                                    Name = TextboxEmpName.Text,
                                    Address = TextboxEmpAddress.Text,
                                    Phone = TextboxEmpPhone.Text,
                                    Role = ComboboxEmpRole.SelectedItem as EmployeeRole
                                });
                            }
                            else
                            {
                                if (unselectedItem.Id == 0)
                                {
                                    Employees.Remove(unselectedItem);
                                }
                            }
                        }
                    }
                }
                ListViewEmployees.ItemsSource = Employees;
                if (ListViewEmployees.SelectedValue != null)
                {
                    TextboxEmpName.Text = ((Employee)ListViewEmployees.SelectedItem).Name;
                    TextboxEmpAddress.Text = ((Employee)ListViewEmployees.SelectedItem).Address;
                    TextboxEmpPhone.Text = ((Employee)ListViewEmployees.SelectedItem).Phone;
                    ComboboxEmpRole.SelectedItem = (ListViewEmployees.SelectedItem as Employee).Role;
                    TextBlockSelectedName.Text = ((Employee)ListViewEmployees.SelectedItem).Name;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveEmployee(Employee empToSave)
        {
            if (empToSave.Id > 0)
            {
                EmployeeCache.GetInstance().Update(empToSave);
            }
            else
            {
                EmployeeCache.GetInstance().Insert(empToSave);
            }
            Employees = new ObservableCollection<Employee>(EmployeeCache.GetInstance().Employees);
        }
    }
}
