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
    
    public partial class vPatient :IEntity
    {
        public int? Id { get; set; }
        public string no { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public Nullable<System.DateTime> dob { get; set; }
        public string email { get; set; }
        public Nullable<short> sendEmail { get; set; }
        public string hospital { get; set; }
        public string doctor { get; set; }
        public Nullable<int> SampleCount { get; set; }
        public Nullable<int> CommentsCount { get; set; }
        public Nullable<int> FilesCount { get; set; }
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
        public string PropS1 { get; set; }
        public string PropS2 { get; set; }
        public string PropS3 { get; set; }
        public string PropS4 { get; set; }
        public string PropS5 { get; set; }
        public Nullable<decimal> PropN1 { get; set; }
        public Nullable<decimal> PropN2 { get; set; }
        public Nullable<decimal> PropN3 { get; set; }
        public Nullable<decimal> PropN4 { get; set; }
        public Nullable<decimal> PropN5 { get; set; }
        public Nullable<System.DateTime> PropD1 { get; set; }
        public Nullable<System.DateTime> PropD2 { get; set; }
        public Nullable<System.DateTime> PropD3 { get; set; }
        public Nullable<System.DateTime> PropD4 { get; set; }
        public Nullable<System.DateTime> PropD5 { get; set; }
    }
}
