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
    using Core;
    using System;
    using System.Collections.Generic;
    
    public partial class vContaiter: IEntity
    {
        public int? Id { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public Nullable<int> WorkflowId { get; set; }
        public Nullable<System.DateTime> CollectedDate { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public Nullable<int> LocationId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> BatchId { get; set; }
        public string BatchName { get; set; }
        public string BatchBarcode { get; set; }
        public Nullable<int> ModelId { get; set; }
        public string Model { get; set; }
        public Nullable<int> ContentListCount { get; set; }
        public Nullable<int> CommentsCount { get; set; }
        public Nullable<int> DraftCount { get; set; }
        public Nullable<int> Prop1 { get; set; }
        public Nullable<int> Prop2 { get; set; }
        public Nullable<int> Prop3 { get; set; }
        public Nullable<int> Prop4 { get; set; }
        public Nullable<int> Prop5 { get; set; }
        public Nullable<int> Prop6 { get; set; }
        public Nullable<int> Prop7 { get; set; }
        public Nullable<int> Prop8 { get; set; }
        public Nullable<int> Prop9 { get; set; }
        public Nullable<int> Prop10 { get; set; }
        public byte[] RowVersion { get; set; }
        public int DoubleBarcodeValidation { get; set; }
        public string ContainerType { get; set; }
        public string ContainerTypeDescription { get; set; }
        public Nullable<int> FilesCount { get; set; }
    }
}
