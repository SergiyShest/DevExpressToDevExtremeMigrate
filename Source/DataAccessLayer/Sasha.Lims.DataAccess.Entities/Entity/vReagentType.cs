//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Sasha.Lims.DataAccess.Entities.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class vReagentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public decimal Quantity { get; set; }
        public int Unit_id { get; set; }
        public string UnitName { get; set; }
        public string CatalogNumber { get; set; }
        public Nullable<int> StorageTemp_id { get; set; }
        public string StorageTemp { get; set; }
        public Nullable<int> Vendor_id { get; set; }
        public string VendorName { get; set; }
        public Nullable<int> CommentsCount { get; set; }
        public Nullable<int> FilesCount { get; set; }
        public Nullable<int> ChaildCount { get; set; }
        public string Size { get; set; }
        public Nullable<int> TypeId { get; set; }
        public string TypeName { get; set; }
        public Nullable<int> ParentCount { get; set; }
    }
}
