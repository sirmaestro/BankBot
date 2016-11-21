﻿using Newtonsoft.Json;
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

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "contempt")]
        public double Contempt { get; set; }

        [JsonProperty(PropertyName = "disgust")]
        public double Disgust { get; set; }

        [JsonProperty(PropertyName = "fear")]
        public double Fear { get; set; }

        [JsonProperty(PropertyName = "happiness")]
        public double Happiness { get; set; }

        [JsonProperty(PropertyName = "neutral")]
        public double Neutral { get; set; }

        [JsonProperty(PropertyName = "sadness")]
        public double Sadness { get; set; }

        [JsonProperty(PropertyName = "surprise")]
        public double Surprise { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public double Lat { get; set; }

        [JsonProperty(PropertyName = "lon")]
        public double Lon { get; set; }
    }
}