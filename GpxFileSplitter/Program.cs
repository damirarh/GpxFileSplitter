using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Xml.Linq;

namespace GpxFileSplitter
{
    class Program
    {
        private readonly XNamespace topografix = "http://www.topografix.com/GPX/1/1";

        static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
        }

        [Required]
        [Option(Description = "Input GPX file to split into tracks")]
        public string Input { get; set; } = null!;

        [Required]
        [Option(Description = "Output folder to create and save GPX files with individual tracks")]
        public string OutputFolder { get; set; } = null!;

        private void OnExecute()
        {
            var fullDocument = XDocument.Load(Input);

            foreach (var track in fullDocument.Root.Elements(topografix + "trk"))
            {
                var trackDocument = new XDocument(
                    new XElement(topografix + "gpx",
                        new XElement(topografix + "metadata"),
                        track
                    )
                );

                Directory.CreateDirectory(OutputFolder);
                var filePath = GenerateUniqueFilename(track);
                    
                trackDocument.Save(filePath);
            }
        }

        private string GenerateUniqueFilename(XElement track)
        {
            var trackName = track.Element(topografix + "name").Value;
            var trackStartTime = (DateTime)track
                .Element(topografix + "trkseg")
                .Element(topografix + "trkpt")
                .Element(topografix + "time");
            var coreFilename = $"{trackStartTime.ToString("yyyy-MM-dd")} {trackName}";
            var index = 0;
            string filePath;

            do
            {
                var suffix = index == 0 ? "" : $" ({index})";
                index++;
                var filename = $"{coreFilename}{suffix}.gpx";
                var sanitizedFilename = string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

                filePath = Path.Combine(OutputFolder, sanitizedFilename);
            } while (File.Exists(filePath));

            return filePath;
        }
    }
}
