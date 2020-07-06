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
    /// Interaction logic for RawMaterialVendorEditorView.xaml
    /// </summary>
    public partial class RawMaterialVendorEditorView : UserControl
    {
        ObservableCollection<Vendor> Vendors;
        bool justSaved;
        public RawMaterialVendorEditorView()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            VendorCache.GetInstance().Clear();
            Vendors = new ObservableCollection<Vendor>(VendorCache.GetInstance().Vendors);
            ListViewVendors.ItemsSource = Vendors;
            ListViewVendors.SelectedItem = GetFirstVendor();
        }
        private object GetFirstVendor()
        {
            return Vendors.Count == 0
                ? new Vendor { Id = 0, Name = string.Empty, FullAddress = string.Empty, ContactPerson = string.Empty, Phone = string.Empty, Location = string.Empty }
                : Vendors[0];
        }

        private void ButtonAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Vendors.Add(new Vendor { Id = 0, Name = string.Empty, FullAddress = string.Empty, ContactPerson = string.Empty, Phone = string.Empty, Location = string.Empty });
                ListViewVendors.ItemsSource = Vendors;
                ListViewVendors.SelectedItem = Vendors[Vendors.Count - 1];
                TextboxVendorName.Focus();
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
                DeleteSelectedVendor();
                ListViewVendors.ItemsSource = Vendors;
                ListViewVendors.SelectedItem = GetFirstVendor();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedVendor()
        {
            VendorCache.GetInstance().Delete(ListViewVendors.SelectedItem as Vendor);
            Vendors = new ObservableCollection<Vendor>(VendorCache.GetInstance().Vendors);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                if (ListViewVendors.SelectedItem == null)
                    return;
                SaveVendor(new Vendor
                {
                    Id = (ListViewVendors.SelectedItem as Vendor).Id,
                    Name = TextboxVendorName.Text,
                    FullAddress = TextboxVendorAddress.Text,
                    Location = TextboxVendorLocation.Text,
                    Phone = TextboxVendorPhone.Text,
                    ContactPerson = TextboxVendorContact.Text
                });
                justSaved = true;
                ListViewVendors.ItemsSource = Vendors;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveVendor(Vendor vendorToSave)
        {
            if (vendorToSave.Id > 0)
            {
                VendorCache.GetInstance().Update(vendorToSave);
            }
            else
            {
                VendorCache.GetInstance().Insert(vendorToSave);
            }
            Vendors = new ObservableCollection<Vendor>(VendorCache.GetInstance().Vendors);
        }
        private void ListViewVendors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                {
                    var unselectedItem = e.RemovedItems?[0] as Vendor;
                    //if the unselected item is not given any name and was a new addition , remove it from the list.
                    if (unselectedItem?.Id == 0 && TextboxVendorName.Text?.Length == 0)
                    {
                        Vendors.Remove(unselectedItem);
                    }
                    else if (justSaved)
                    {
                        //nothing to do here, except setting the flag off
                        justSaved = false;
                    }
                    else
                    {
                        if (unselectedItem?.Name != TextboxVendorName.Text
                            || unselectedItem?.FullAddress != TextboxVendorAddress.Text
                            || unselectedItem?.Phone != TextboxVendorPhone.Text
                            || unselectedItem?.Location != TextboxVendorLocation.Text
                            || unselectedItem?.ContactPerson != TextboxVendorContact.Text)
                        {
                            var response = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (response == MessageBoxResult.Yes)
                            {
                                SaveVendor(new Vendor
                                {
                                    Id = (ListViewVendors.SelectedItem as Vendor).Id,
                                    Name = TextboxVendorName.Text,
                                    FullAddress = TextboxVendorAddress.Text,
                                    Location = TextboxVendorLocation.Text,
                                    Phone = TextboxVendorPhone.Text,
                                    ContactPerson = TextboxVendorContact.Text
                                });
                            }
                            else
                            {
                                if (unselectedItem.Id == 0)
                                {
                                    Vendors.Remove(unselectedItem);
                                }
                            }
                        }
                    }
                }
                ListViewVendors.ItemsSource = Vendors;
                if (ListViewVendors.SelectedValue != null)
                {
                    TextboxVendorName.Text = ((Vendor)ListViewVendors.SelectedItem).Name;
                    TextboxVendorAddress.Text = ((Vendor)ListViewVendors.SelectedItem).FullAddress;
                    TextboxVendorLocation.Text = ((Vendor)ListViewVendors.SelectedItem).Location;
                    TextboxVendorContact.Text = ((Vendor)ListViewVendors.SelectedItem).ContactPerson;
                    TextboxVendorPhone.Text = ((Vendor)ListViewVendors.SelectedItem).Phone;
                    TextBlockSelectedName.Text = ((Vendor)ListViewVendors.SelectedItem).Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
