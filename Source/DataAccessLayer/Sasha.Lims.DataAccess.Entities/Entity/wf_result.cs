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
    
    public partial class wf_result
    {
        public byte[] v { get; set; }
        public int id { get; set; }
        public Nullable<int> process_id { get; set; }
        public Nullable<int> sample_id { get; set; }
        public Nullable<int> result_id { get; set; }
        public Nullable<double> value { get; set; }
        public string filePath { get; set; }
        public string comment { get; set; }
        public Nullable<short> Sended { get; set; }
        public string jsonData { get; set; }
    }
}
