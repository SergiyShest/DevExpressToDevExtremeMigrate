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
    
    public partial class h_item
    {
        public int id { get; set; }
        public string path { get; set; }
        public Nullable<int> parent_id { get; set; }
        public string name { get; set; }
        public string content { get; set; }
        public Nullable<int> num { get; set; }
    }
}
