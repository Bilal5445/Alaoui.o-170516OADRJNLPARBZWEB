using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextPersist
    {
        public void Serialize<T>(T entry, String path)
        {
            // Create file if not there (and close to remove lock on file)
            // if (!System.IO.File.Exists(path)) System.IO.File.Create(path).Close();

            //
            List<T> entries = new List<T>();
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                // deserialize / serialize entries : read / add / write back
                using (var reader = new StreamReader(path))
                {
                    entries = (List<T>)serializer.Deserialize(reader);
                }
            }

            // add
            entries.Add(entry);

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }
    }
}
