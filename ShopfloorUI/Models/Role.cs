using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class EmployeeRole : ICloneable, IComparable
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public object Clone()
        {
            return new EmployeeRole
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description
            };
        }

        public int CompareTo(object obj)
        {
            var comparison = obj as EmployeeRole;
            if(this.Id == comparison.Id
                && this.Name == comparison.Name
                && this.Description == comparison.Description)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
