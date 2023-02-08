﻿using ForteDigitalTask.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ForteDigitalTask.Parser
{
    public class InternParser : InternParserInterface
    {
        private WebClient client;

        public InternParser(WebClient client)
        {
            this.client = client;
        }
        public List<Intern> ParseInternsFromFile(string url)
        {
            string fileContent = client.DownloadString(url);
            string contentType = client.ResponseHeaders["Content-Type"];


            if (contentType == "application/zip")
            {
                return ParseInternsFromZip(url);
            }

            if (contentType == "application/json; charset=utf-8")
            {
                return ParseInternsFromJson(fileContent);
            }
            if (contentType == "text/csv; charset=utf-8")
            {
                return ParseInternsFromCsv(fileContent);
            }
            Console.WriteLine("Error: Unsupported file format.");
            return new List<Intern>();
        }

        public List<Intern> ParseInternsFromJson(string jsonContent)
        {
            JObject data = JObject.Parse(jsonContent);
            JArray interns = (JArray)data["interns"];

            List<Intern> listOfInterns = new List<Intern>();

            foreach (var intern in interns)
            {
                Intern newIntern = new Intern
                {
                    id = (int)intern["id"],
                    age = (int)intern["age"],
                    name = (string)intern["name"],
                    email = (string)intern["email"],
                    internshipStart = DateTime.ParseExact(intern["internshipStart"].ToString(), "yyyy-MM-ddTHH:mm+00Z", CultureInfo.InvariantCulture),
                    internshipEnd = DateTime.ParseExact(intern["internshipEnd"].ToString(), "yyyy-MM-ddTHH:mm+00Z", CultureInfo.InvariantCulture)
                };
                listOfInterns.Add(newIntern);
            }


            return listOfInterns;
        }

        public List<Intern> ParseInternsFromZip(string url)
        {
            List<Intern> interns = new List<Intern>();
            byte[] archiveBytes = client.DownloadData(url);
            using (MemoryStream stream = new MemoryStream(archiveBytes, false))
            {
                using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(stream))
                {
                    var entry = archive.Entries.FirstOrDefault();
                    if (entry != null)
                    {
                        using (StreamReader reader = new StreamReader(entry.OpenEntryStream()))
                        {
                            string line = reader.ReadLine();
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] values = line.Split(',');
                                Intern intern = Intern.CreateFromArray(values);
                                interns.Add(intern);
                            }
                        }
                    }
                }
            }
            return interns;
        }

        public List<Intern> ParseInternsFromCsv(string fileContent)
        {
            List<Intern> interns = new List<Intern>();
            using (StringReader reader = new StringReader(fileContent))
            {
                string line = reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(',');
                    Intern intern = Intern.CreateFromArray(values);
                    interns.Add(intern);
                }
            }
            return interns;
        }
    }
}