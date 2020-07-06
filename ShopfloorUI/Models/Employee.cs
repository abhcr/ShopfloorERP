using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class Employee
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public EmployeeRole Role { get; set; }
        public int QSize { get; set; }
        public string NameWithQSize => $"{Name} ({QSize.ToString()})";
    }
}
