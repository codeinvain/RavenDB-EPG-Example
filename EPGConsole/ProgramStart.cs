using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;
using Raven.Client;
using RavenEGP.Model;
using RavenEGP.Utilities;
using System.Globalization;

namespace RavenEGP
{
    class ProgramStart
    {
        static void Main(string[] args)
        {
            DocumentStore dc = new DocumentStore() { Url = "http://localhost:8080" };
            dc.Initialize();
            ETL.Ingest("Resources/xmltv.xml", dc);
            Console.Write("process ended, press any key to exit");
            Console.ReadLine();
        }
    }
}
