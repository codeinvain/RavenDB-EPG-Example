using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using Raven.Client.Document;
using Raven.Client;
using RavenEGP.Model;
using System.Globalization;
using Raven.Database.Data;

namespace RavenEGP.Utilities
{
    class ETL
    {
        public DocumentStore Raven { get; set; }
        public IDocumentSession Session { get; private set; }
        public string Source { get; set; }
        private Dictionary<string, Channel> channels;
        private int BulkOperationSize = 512;
        private ETL()
        {
        }

        public static void Ingest(String source, DocumentStore raven)
        {
            var etl = new ETL();
            etl.Source = source;
            etl.Raven = raven;
            etl.Start();
        }

        private void Start()
        {
            channels = new Dictionary<string, Channel>();
            this.Session = Raven.OpenSession();

            deleteCustomIndexes();
            deleteAllChannels();
            deleteAllPrograms();

            importChannels();
            importPrograms();
            createCustomIndex();
            
            if (Session != null && Session is IDisposable)
            {
                Session.Dispose();
            }
        }

        private void createCustomIndex()
        {
            // creatre all custom indexes
        }

        private void deleteCustomIndexes()
        {
            // delete all custom indexes
        }

        private void deleteAllPrograms()
        {
            Console.WriteLine("Deleting all programs");
            Raven.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName", new IndexQuery()
            {
                Query = "Tag:[[Programs]]"
            }, allowStale:true);
        }

        private void deleteAllChannels()
        {
            Console.WriteLine("Deleting all channels");
            Raven.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName", new IndexQuery()
                                                  {
                                                      Query = "Tag:[[Channels]]"
                                                  }, allowStale: true);
        }

        private void importChannels()
        {
            Console.WriteLine("Importing channels");
            var settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            var xml = XmlReader.Create(Source, settings);
            var dump = 0;
            var session = Raven.OpenSession();

            while (xml.Read())
            {
                if (xml.Name == "channel")
                {
                    var key = xml.GetAttribute("id");
                    xml.Read();
                    if (xml.Name == "display-name")
                    {
                        xml.Read();
                        dump++;
                        var channel = new Channel() { Name = xml.Value };
                        channels[key] = channel;
                        session.Store(channel);
                    }
                }

                if (dump == BulkOperationSize)
                {
                    session.SaveChanges(); 
                    session.Dispose();
                    session = Raven.OpenSession();
                    dump = 0;
                }

            }
            session.SaveChanges();
            session.Dispose();
        }

        private void importPrograms()
        {
            Console.WriteLine("Importing programs");
            var settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            var xml = XmlReader.Create(Source, settings);
            var dump = 0;
            var session = Raven.OpenSession();
            while (xml.Read())
            {
                if (xml.NodeType == XmlNodeType.Element && xml.Name == "programme")
                {
                    var program = new Model.Program();
                    if (xml.GetAttribute("start") != null)
                        program.StartDate = DateTime.ParseExact(xml.GetAttribute("start"), "yyyyMMddHHmmss zz00", CultureInfo.CurrentCulture.DateTimeFormat);
                    if (xml.GetAttribute("stop") != null)
                        program.EndDate = DateTime.ParseExact(xml.GetAttribute("stop"), "yyyyMMddHHmmss zz00", CultureInfo.CurrentCulture.DateTimeFormat);
                    if (xml.GetAttribute("channel") != null)
                    {
                        program.Channel = channels[xml.GetAttribute("channel").ToString()];
                        if (program.Channel == null)
                            Console.WriteLine("can't find " + xml.GetAttribute("channel").ToString());
                    }
                    else
                    {
                        Console.WriteLine("no channel attribute on node " + xml.Name);
                    }
                    xml.Read();
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "title")
                    {
                        xml.Read();
                        program.Name = xml.Value;
                        xml.Read();
                    }
                    if (xml.NodeType == XmlNodeType.Element && xml.Name == "desc")
                    {
                        xml.Read();
                        program.Description = xml.Value;

                    }
                    session.Store(program);
                    dump++;
                    if (dump == BulkOperationSize)
                    {
                        dump = 0;
                        session.SaveChanges();
                        session.Dispose();
                        session = Raven.OpenSession();
                    }
                }
            }
            session.SaveChanges();
            session.Dispose();
        }


    }
}
