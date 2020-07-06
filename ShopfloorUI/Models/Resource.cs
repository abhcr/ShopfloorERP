using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class Resource
    {
        public enum ResourceType
        {
            Inhouse=0,
            Outsourced=1,
            Supplier=2
        }
        
        public Int64 Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        //public List<EmployeeRole> AccessList { get; set; } - TODO: use this to limit access to resource to certain roles alone. now any employees can be assigned to a process theoretically.
        public string Rate { get; set; }
        public string Location { get; set; }
        public string FullAddress { get; set; }
        public ResourceType Type { get; set; }
        public string DisplayName => $"{Name} ({(Type == ResourceType.Inhouse ? "Inhouse" : Location)})";
        /// <summary>
        /// Call ResourceCache.GetInstance().UpdateAllQSizes() before using this property
        /// </summary>
        public int QSize { get; set; }

        public string NameWithQSize => $"{Name} ({QSize.ToString()})";
    }
}
