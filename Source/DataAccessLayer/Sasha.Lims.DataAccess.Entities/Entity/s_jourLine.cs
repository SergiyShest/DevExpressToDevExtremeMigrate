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
    
    public partial class s_jourLine
    {
        public byte[] v { get; set; }
        public int id { get; set; }
        public Nullable<Sasha.Lims.DataAccess.Entities.Constants.JournalType> jourType_id { get; set; }
        public string documentNo { get; set; }
        public Nullable<int> sample_id { get; set; }
        public string barcode { get; set; }
        public string well { get; set; }
        public string name { get; set; }
        public Nullable<int> workflow_id { get; set; }
        public Nullable<System.DateTime> collectedDate { get; set; }
        public Nullable<Sasha.Lims.DataAccess.Entities.Constants.SampleStatus> status_id { get; set; }
        public Nullable<int> client_id { get; set; }
        public Nullable<int> doctor_id { get; set; }
        public Nullable<int> patient_id { get; set; }
        public Nullable<int> location_id { get; set; }
        public Nullable<int> equipment_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> sampleType_id { get; set; }
        public Nullable<double> volume { get; set; }
        public Nullable<int> volumeUnit_id { get; set; }
        public Nullable<int> tubeType_id { get; set; }
        public string aspNetUser_Id { get; set; }
        public Nullable<System.DateTime> DateCreate { get; set; }
        public Nullable<System.DateTime> ModDate { get; set; }
        public string patientNo { get; set; }
        public string patientFirstName { get; set; }
        public string patientLastName { get; set; }
        public Nullable<System.DateTime> patientDateOfBirth { get; set; }
        public Nullable<int> workflowStatus_id { get; set; }
        public byte[] sample_version { get; set; }
        public Nullable<System.DateTime> acceptedDate { get; set; }
        public Nullable<int> parent_id { get; set; }
        public Nullable<int> batch_id { get; set; }
        public Nullable<Sasha.Lims.DataAccess.Entities.Constants.SampleModel> model_id { get; set; }
        public Nullable<bool> makeAliquot { get; set; }
        public string patientEmail { get; set; }
        public Nullable<bool> patientSendEmail { get; set; }
        public Nullable<bool> emailReport { get; set; }
        public Nullable<int> prop1 { get; set; }
        public Nullable<int> prop2 { get; set; }
        public Nullable<int> prop3 { get; set; }
        public Nullable<int> prop4 { get; set; }
        public Nullable<int> prop5 { get; set; }
        public Nullable<int> prop6 { get; set; }
        public Nullable<int> prop7 { get; set; }
        public Nullable<int> prop8 { get; set; }
        public Nullable<int> prop9 { get; set; }
        public Nullable<int> prop10 { get; set; }
        public Nullable<int> source_id { get; set; }
        public Nullable<int> process_id { get; set; }
        public string Discriminator { get; set; }
        public Nullable<int> draft_bathId { get; set; }
        public Nullable<int> flags { get; set; }
        public string props1 { get; set; }
        public string props2 { get; set; }
        public string props3 { get; set; }
        public string props4 { get; set; }
        public string props5 { get; set; }
        public Nullable<decimal> propn1 { get; set; }
        public Nullable<decimal> propn2 { get; set; }
        public Nullable<decimal> propn3 { get; set; }
        public Nullable<decimal> propn4 { get; set; }
        public Nullable<decimal> propn5 { get; set; }
        public Nullable<System.DateTime> propd1 { get; set; }
        public Nullable<System.DateTime> propd2 { get; set; }
        public Nullable<System.DateTime> propd3 { get; set; }
        public Nullable<System.DateTime> propd4 { get; set; }
        public Nullable<System.DateTime> propd5 { get; set; }
    }
}
