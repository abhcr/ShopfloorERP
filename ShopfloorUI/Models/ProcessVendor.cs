using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class ProcessVendor : Vendor
    {
        public List<Process> Processes { get; set; }
        public TimeSpan? DefaultLeadTime { get; set; }
    }
}
