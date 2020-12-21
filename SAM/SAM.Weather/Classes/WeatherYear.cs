﻿using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public class WeatherYear : SAMObject
    {
        private string description;
        private double latitude;
        private double longitude;
        private double elevation;
        private WeatherDay[] weatherDays;

        public WeatherYear()
        {

        }

        public WeatherYear(WeatherYear weatherYear)
            : base(weatherYear)
        {
            description = weatherYear.description;
            latitude = weatherYear.latitude;
            longitude = weatherYear.longitude;
            elevation = weatherYear.elevation;

            if(weatherYear.weatherDays != null)
            {
                weatherDays = new WeatherDay[weatherYear.weatherDays.Length];
                for (int i = 0; i < weatherYear.weatherDays.Length; i++)
                    weatherDays[i] = weatherYear.weatherDays[i] == null ? null : new WeatherDay(weatherYear.weatherDays[i]);
            }
        }

        public WeatherYear(double latitude, double longitude, double elevation)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.elevation = elevation;
        }
        
        public WeatherYear(string description, double latitude, double longitude, double elevation, WeatherYear weatherYear)
            : base(weatherYear)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;

            if (weatherYear.weatherDays != null)
            {
                weatherDays = new WeatherDay[weatherYear.weatherDays.Length];
                for (int i = 0; i < weatherYear.weatherDays.Length; i++)
                    weatherDays[i] = weatherYear.weatherDays[i] == null ? null : new WeatherDay(weatherYear.weatherDays[i]);
            }
        }

        public WeatherYear(string name, string description, double latitude, double longitude, double elevation)
            : base(name)
        {
            this.description = description;
            this.latitude = latitude;
            this.longitude = longitude;
            this.elevation = elevation;
        }

        public WeatherYear(JObject jObject)
            : base(jObject)
        {
        }

        public WeatherDay this[int i]
        {
            get
            {
                if (i < 0 || i >= 365)
                    return null;

                if (weatherDays == null)
                    return null;

                return weatherDays[i];
            }
            set
            {
                if (i < 0 || i >= 365)
                    return;

                if (weatherDays == null)
                    weatherDays = new WeatherDay[365];

                weatherDays[i] = value;
            }
        }

        public bool Add(int day, int hour, Dictionary<string, double> values)
        {
            if (day < 0 || day >= 365)
                return false;

            if (hour < 0 || hour >= 24)
                return false;

            if (weatherDays == null)
                weatherDays = new WeatherDay[365];

            WeatherDay weatherDay = weatherDays[day];
            if(weatherDay == null)
            {
                weatherDay = new WeatherDay();
                weatherDays[day] = weatherDay;
            }
            
            foreach(KeyValuePair<string, double> keyValuePair in values)
                weatherDay[keyValuePair.Key, hour] = keyValuePair.Value;

            return true;
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }

        public double Elevtion
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        public List<double> GetValues(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            List<double> result = new List<double>();
            for(int i=0; i < weatherDays.Length; i++)
            {
                WeatherDay weatherDay = weatherDays[i];
                if(weatherDay == null)
                {
                    result.AddRange(Enumerable.Repeat(double.NaN, 24));
                    continue;
                }

                double[] values = weatherDay[name];
                if (values == null || values.Length != 24)
                    result.AddRange(Enumerable.Repeat(double.NaN, 24));
            }

            return result;
        }

        public List<double> GetValues(WeatherDataType weatherDataType)
        {
            return GetValues(weatherDataType.ToString());
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("Description"))
                description = jObject.Value<string>("Description");

            if (jObject.ContainsKey("Latitude"))
                latitude = jObject.Value<double>("Latitude");

            if (jObject.ContainsKey("Longitude"))
                longitude = jObject.Value<double>("Longitude");

            if(jObject.ContainsKey("WeatherDays"))
            {
                JArray jArray = jObject.Value<JArray>("WeatherDays");
                if(jArray != null && jArray.Count == 365)
                {
                    weatherDays = new WeatherDay[365];
                    for (int i = 0; i < 365; i++)
                    {
                        JObject jObject_Temp = jArray[i] as JObject;
                        if (jObject_Temp == null)
                            continue;

                        weatherDays[i] =new WeatherDay(jObject_Temp);
                    }
                }
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (description != null)
                jObject.Add("Description", description);

            if (double.IsNaN(latitude))
                jObject.Add("Latitude", latitude);

            if (double.IsNaN(longitude))
                jObject.Add("Longitude", longitude);

            if(weatherDays != null)
            {
                JArray jArray = new JArray();
                foreach (WeatherDay weatherDay in weatherDays)
                    jArray.Add(weatherDay?.ToJObject());

                jObject.Add("WeatherDays", jArray);
            }

            return jObject;
        }
    }
}