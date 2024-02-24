using DAL.Core;

namespace Entity.Controllers
{
    public class DateFromTo
    {
        public string from { get; set; } = string.Empty;
        public DateTime? From { get { return CoreHelper.ParseRequestDate(from); } }

        public string to {get;set;} = string.Empty;
        public DateTime? To { get { return CoreHelper.ParseRequestDate(to); } }
    }
}