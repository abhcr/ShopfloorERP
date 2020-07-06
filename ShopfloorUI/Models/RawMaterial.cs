using System;
using System.Collections.Generic;
using System.Text;

namespace ShopfloorUI.Models
{
    public class RawMaterial
    {
        public enum RawMaterialStatus
        {
            Waiting,
            Completed
        }
        public UInt32 Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Rate { get; set; }
        public Int64 ProjectId { get; set; }
        public Vendor Vendor { get; set; }
        public Process CollectingEmployee { get; set; }
    }
}
