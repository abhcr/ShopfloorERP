using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class Vendor //vendor and resource are alike.. Raw material and process are alike
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string Location { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        //public List<EmployeeRole> AccessList { get; set; }  -- commented - now any employee can be assigned to bring a raw material theoretically. 
    }
}
