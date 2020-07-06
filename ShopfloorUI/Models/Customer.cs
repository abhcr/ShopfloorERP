using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class Customer
    {
        public Int64? Id { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string Location { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public Employee AccountManager { get; set; }
        public string TaxCode { get; set; }
        public string DisplayName => $"{Name} (ID: {Id.ToString()})";
    }
}
