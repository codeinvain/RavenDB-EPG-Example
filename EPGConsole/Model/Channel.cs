using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenEGP.Model
{
    class Channel
    {
        //RavenDB document identifier
        public string Id { get; set; }

        public string Name { get; set; }
        public IList<string> Tags { get; set; }
        
    }
}
