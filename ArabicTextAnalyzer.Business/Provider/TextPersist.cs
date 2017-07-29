using ArabicTextAnalyzer.Domain.Models;
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

        public void Serialize_Update_M_ARABICDARIJAENTRY_LATINWORD_MostPopularVariant(Guid id_arabicdarijaentry_latinword, String mostPopularVariant, String path)
        {
            //
            List<M_ARABICDARIJAENTRY_LATINWORD> entries = new List<M_ARABICDARIJAENTRY_LATINWORD>();
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                // deserialize / serialize entries : read / add / write back
                using (var reader = new StreamReader(path))
                {
                    entries = (List<M_ARABICDARIJAENTRY_LATINWORD>)serializer.Deserialize(reader);
                }
            }

            // add
            entries.Find(m => m.ID_ARABICDARIJAENTRY_LATINWORD == id_arabicdarijaentry_latinword).MostPopularVariant = mostPopularVariant;

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }
    }
}
