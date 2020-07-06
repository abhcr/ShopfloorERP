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
    /// Interaction logic for CustomerEditorView.xaml
    /// </summary>
    public partial class CustomerEditorView : UserControl
    {
        ObservableCollection<Customer> Customers;
        ObservableCollection<Employee> Employees;
        bool justSaved;
        public CustomerEditorView()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            CustomerCache.GetInstance().Clear();
            EmployeeCache.GetInstance().Clear();
            Customers = new ObservableCollection<Customer>(CustomerCache.GetInstance().Customers);
            Employees = new ObservableCollection<Employee>(EmployeeCache.GetInstance().Employees);
            ListViewCustomers.ItemsSource = Customers;
            ComboboxCustomerManager.ItemsSource = Employees;
            ListViewCustomers.SelectedItem = GetFirstCustomer();
        }
        private object GetFirstCustomer()
        {
            return Customers.Count == 0
                ? new Customer { Id = 0, Name = string.Empty, FullAddress = string.Empty, ContactPerson = string.Empty, Phone = string.Empty, Location = string.Empty, AccountManager = GetFirstEmployee() }
                : Customers[0];
        }

        private Employee GetFirstEmployee()
        {
            return Employees.Count == 0
                ? new Employee { Id = 0, Name = string.Empty, Address = string.Empty, Phone = string.Empty }
                : Employees[0];
        }


        private void ButtonAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Customers.Add(new Customer { Id = 0, Name = string.Empty, FullAddress = string.Empty, ContactPerson = string.Empty, Phone = string.Empty, Location = string.Empty, AccountManager = GetFirstEmployee() });
                ListViewCustomers.ItemsSource = Customers;
                ListViewCustomers.SelectedItem = Customers[Customers.Count - 1];
                TextboxCustomerName.Focus();
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
                DeleteSelectedCustomer();
                ListViewCustomers.ItemsSource = Customers;
                ListViewCustomers.SelectedItem = GetFirstCustomer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedCustomer()
        {
            CustomerCache.GetInstance().Delete(ListViewCustomers.SelectedItem as Customer);
            Customers = new ObservableCollection<Customer>(CustomerCache.GetInstance().Customers);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (ListViewCustomers.SelectedItem == null)
                    return;
                SaveCustomer(new Customer
                {
                    Id = (ListViewCustomers.SelectedItem as Customer).Id,
                    Name = TextboxCustomerName.Text,
                    FullAddress = TextboxCustomerAddress.Text,
                    Location = TextboxCustomerLocation.Text,
                    Phone = TextboxCustomerPhone.Text,
                    ContactPerson = TextboxCustomerContact.Text,
                    AccountManager = ComboboxCustomerManager.SelectedItem as Employee
                });
                justSaved = true;
                ListViewCustomers.ItemsSource = Customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveCustomer(Customer itemToSave)
        {
            if (itemToSave.Id > 0)
            {
                CustomerCache.GetInstance().Update(itemToSave);
            }
            else
            {
                CustomerCache.GetInstance().Insert(itemToSave);
            }
            Customers = new ObservableCollection<Customer>(CustomerCache.GetInstance().Customers);
        }
        private void ListViewCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                {
                    var unselectedItem = e.RemovedItems?[0] as Customer;
                    //if the unselected item is not given any name and was a new addition , remove it from the list.
                    if (unselectedItem?.Id == 0 && TextboxCustomerName.Text?.Length == 0)
                    {
                        Customers.Remove(unselectedItem);
                    }
                    else if (justSaved)
                    {
                        //nothing to do here, except setting the flag off
                        justSaved = false;
                    }
                    else
                    {
                        if (unselectedItem?.Name != TextboxCustomerName.Text
                            || unselectedItem?.FullAddress != TextboxCustomerAddress.Text
                            || unselectedItem?.Phone != TextboxCustomerPhone.Text
                            || unselectedItem?.Location != TextboxCustomerLocation.Text
                            || unselectedItem?.ContactPerson != TextboxCustomerContact.Text
                            || unselectedItem?.AccountManager.Id != (ComboboxCustomerManager.SelectedItem as Process).Id)
                        {
                            var response = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (response == MessageBoxResult.Yes)
                            {
                                SaveCustomer(new Customer
                                {
                                    Id = (ListViewCustomers.SelectedItem as Customer).Id,
                                    Name = TextboxCustomerName.Text,
                                    FullAddress = TextboxCustomerAddress.Text,
                                    Location = TextboxCustomerLocation.Text,
                                    Phone = TextboxCustomerPhone.Text,
                                    ContactPerson = TextboxCustomerContact.Text,
                                    AccountManager = ComboboxCustomerManager.SelectedItem as Employee
                                });
                            }
                            else
                            {
                                if (unselectedItem.Id == 0)
                                {
                                    Customers.Remove(unselectedItem);
                                }
                            }
                        }
                    }
                }
                ListViewCustomers.ItemsSource = Customers;
                if (ListViewCustomers.SelectedValue != null)
                {
                    TextboxCustomerName.Text = ((Customer)ListViewCustomers.SelectedItem).Name;
                    TextboxCustomerAddress.Text = ((Customer)ListViewCustomers.SelectedItem).FullAddress;
                    TextboxCustomerLocation.Text = ((Customer)ListViewCustomers.SelectedItem).Location;
                    TextboxCustomerContact.Text = ((Customer)ListViewCustomers.SelectedItem).ContactPerson;
                    TextboxCustomerPhone.Text = ((Customer)ListViewCustomers.SelectedItem).Phone;
                    ComboboxCustomerManager.SelectedItem = ((Customer)ListViewCustomers.SelectedItem).AccountManager;
                    TextBlockSelectedName.Text = ((Customer)ListViewCustomers.SelectedItem).DisplayName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
