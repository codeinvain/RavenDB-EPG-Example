using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace RavenEGP.Model
{
    class Program
    {
        
        //RavenDB document identifier
        public string Id { get; set; }

        public Channel Channel { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<string> Tags { get; set; }

        [JsonIgnore] 
        public TimeSpan Duration
        {
            get
            {
                if (StartDate !=null && EndDate!=null)
                    return EndDate.Subtract(StartDate);
                return TimeSpan.FromMilliseconds(0.0);
            }
        }
    }
}
