

using Core;
using System.Text.Json.Serialization;

namespace BLL { 
    public class ObjectError
    {

        public ObjectError(string objName, string error)
        {
            Name = objName;
            Errors = new List<string> { error };
        }


        private Exception ex;

        public string Name { get; }

        public List<string> Errors { get; set; }


    }



}