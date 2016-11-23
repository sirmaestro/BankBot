using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botapplication.DataModel
{
    public class Test
    {
        [JsonProperty(PropertyName = "Id")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string firstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string lastName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime Date { get; set; }
    }
}