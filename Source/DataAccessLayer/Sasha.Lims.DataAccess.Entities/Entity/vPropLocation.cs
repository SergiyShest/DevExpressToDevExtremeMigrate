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
    
    public partial class vPropLocation
    {
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> prop_id { get; set; }
        public Nullable<int> LEVEL { get; set; }
        public string treepath { get; set; }
        public Nullable<int> limited_level { get; set; }
        public Nullable<int> limited_id { get; set; }
        public string limited_path { get; set; }
    }
}
