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
    /// Interaction logic for ResourceEditorView.xaml
    /// </summary>
    public partial class ResourceEditorView : UserControl
    {
        ObservableCollection<Resource> ResourceList;
        private bool justSaved;
        public ResourceEditorView()
        {
            InitializeComponent();
            Refresh();
        }
        public void Refresh()
        {
            ResourceCache.GetInstance().Clear();
            ResourceList = new ObservableCollection<Resource>(ResourceCache.GetInstance().Resources);
            ListViewResources.ItemsSource = ResourceList;
            ComboboxResType.ItemsSource = Enum.GetNames(typeof(Resource.ResourceType));
            ComboboxResType.SelectedItem = ((string[])ComboboxResType.ItemsSource)[0];
            ListViewResources.SelectedItem = GetFirstResource();
        }

        private object GetFirstResource()
        {
            return ResourceList.Count == 0
                ? new Resource
                {
                    Id = 0,
                    Name = string.Empty,
                    FullAddress = string.Empty,
                    Code = string.Empty,
                    Location = string.Empty,
                    Rate = string.Empty,
                    Type = Resource.ResourceType.Inhouse
                }
                : ResourceList[0];
        }

        private void ButtonAddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResourceList.Add(new Resource { Id = 0, Name = string.Empty, FullAddress = string.Empty, Code = string.Empty, Location = string.Empty, Rate = string.Empty, Type = Resource.ResourceType.Inhouse });
                ListViewResources.ItemsSource = ResourceList;
                ListViewResources.SelectedItem = ResourceList[ResourceList.Count - 1];
                TextboxResName.Focus();
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
                DeleteSelectedResource();
                ListViewResources.ItemsSource = ResourceList;
                ListViewResources.SelectedItem = GetFirstResource();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedResource()
        {
            ResourceCache.GetInstance().Delete(ListViewResources.SelectedItem as Resource);
            ResourceList = new ObservableCollection<Resource>(ResourceCache.GetInstance().Resources);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (ListViewResources.SelectedItem == null)
                    return;
                var itemToSave = new Resource
                {
                    Id = (ListViewResources.SelectedItem as Resource).Id,
                    Name = TextboxResName.Text,
                    Code = TextboxResCode.Text,
                    Type = Enum.Parse<Resource.ResourceType>(ComboboxResType.SelectedItem.ToString()),
                    Rate = TextboxResRate.Text,
                    Location = TextboxResLocation.Text,
                    FullAddress = TextboxResAddress.Text
                };
                SaveResource(itemToSave);
                justSaved = true;
                ListViewResources.ItemsSource = ResourceList;
                ListViewResources.SelectedItem = itemToSave;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListViewResources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.RemovedItems.Count > 0)
                {
                    var unselectedItem = e.RemovedItems?[0] as Resource;
                    //if the unselected item is not given any name and was a new addition , remove it from the list.
                    if (unselectedItem?.Id == 0 && TextboxResName.Text?.Length == 0)
                    {
                        ResourceList.Remove(unselectedItem);
                    }
                    else if (justSaved)
                    {
                        //nothing to do here, except setting the flag off
                        justSaved = false;
                    }
                    else
                    {
                        if (unselectedItem?.Name != TextboxResName.Text
                            || unselectedItem?.FullAddress != TextboxResAddress.Text
                            || unselectedItem?.Location != TextboxResLocation.Text
                            || unselectedItem?.Rate != TextboxResRate.Text
                            || unselectedItem?.Code != TextboxResCode.Text
                            || unselectedItem?.Type != Enum.Parse<Resource.ResourceType>(ComboboxResType.SelectedItem.ToString()))
                        {
                            var response = MessageBox.Show("Save changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (response == MessageBoxResult.Yes)
                            {
                                SaveResource(new Resource
                                {
                                    Id = unselectedItem.Id,
                                    Name = TextboxResName.Text,
                                    FullAddress = TextboxResAddress.Text,
                                    Location = TextboxResLocation.Text,
                                    Rate = TextboxResRate.Text,
                                    Code = TextboxResCode.Text,
                                    Type = (Resource.ResourceType)ComboboxResType.SelectedItem,
                                });
                            }
                            else
                            {
                                if (unselectedItem.Id == 0)
                                {
                                    ResourceList.Remove(unselectedItem);
                                }
                            }
                        }
                    }
                }
                ListViewResources.ItemsSource = ResourceList;
                if (ListViewResources.SelectedValue != null)
                {
                    TextboxResName.Text = ((Resource)ListViewResources.SelectedItem).Name;
                    TextboxResAddress.Text = ((Resource)ListViewResources.SelectedItem).FullAddress;
                    TextboxResLocation.Text = ((Resource)ListViewResources.SelectedItem).Location;
                    TextboxResCode.Text = ((Resource)ListViewResources.SelectedItem).Code;
                    TextboxResRate.Text = ((Resource)ListViewResources.SelectedItem).Rate;
                    ComboboxResType.SelectedItem = ((Resource)ListViewResources.SelectedItem).Type.ToString();
                    TextBlockSelectedName.Text = ((Resource)ListViewResources.SelectedItem).DisplayName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveResource(Resource item)
        {
            if (item.Id > 0)
            {
                ResourceCache.GetInstance().Update(item);
            }
            else
            {
                ResourceCache.GetInstance().Insert(item);
            }
            ResourceList = new ObservableCollection<Resource>(ResourceCache.GetInstance().Resources);
        }

        private void ComboboxResType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0].ToString() == Resource.ResourceType.Inhouse.ToString())
            {
                TexblockLocation.Visibility = Visibility.Hidden;
                TextboxResAddress.Visibility = Visibility.Hidden;
                TextboxResLocation.Visibility = Visibility.Hidden;
                TextblockAddress.Visibility = Visibility.Hidden;
            }
            else
            {
                TexblockLocation.Visibility = Visibility.Visible;
                TextboxResAddress.Visibility = Visibility.Visible;
                TextboxResLocation.Visibility = Visibility.Visible;
                TextblockAddress.Visibility = Visibility.Visible;
            }
            if (ListViewResources.SelectedItem != null)
            {
                ((Resource)ListViewResources.SelectedItem).Type = Enum.Parse<Resource.ResourceType>(ComboboxResType.SelectedItem.ToString());
            }
        }
    }
}
