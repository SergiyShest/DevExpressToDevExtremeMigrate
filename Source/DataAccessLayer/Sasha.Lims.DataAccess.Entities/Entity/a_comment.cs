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
    
    public partial class a_comment
    {
        public byte[] v { get; set; }
        public int id { get; set; }
        public Nullable<int> table_id { get; set; }
        public Nullable<int> row_id { get; set; }
        public Nullable<System.DateTime> createdDateTime { get; set; }
        public Nullable<int> createdByUser_id { get; set; }
        public string comment { get; set; }
        public Nullable<int> commentType_id { get; set; }
    }
}
