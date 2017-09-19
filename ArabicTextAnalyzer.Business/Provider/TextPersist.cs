using ArabicTextAnalyzer.Domain.Models;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public void Serialize_Touch<T>(String path)
        {
            //
            List<T> entries = new List<T>();
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                ;
            }
            else
            {
                // write schema only
                using (var writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, entries);
                    writer.Flush();
                }
            }
        }

        /*public void SerializeBack_path<T>(List<T> entries, String path)
        {
            //
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }*/

        public void SerializeBack_dataPath<T>(List<T> entries, String dataPath)
        {
            //
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            var path = dataPath + "/data_" + typeof(T).Name + ".txt";

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }

        public List<T> Deserialize<T>(String dataPath)
        {
            var path = dataPath + "/data_" + typeof(T).Name + ".txt";
            
            // if not previous existing, create
            if (File.Exists(path) && new FileInfo(path).Length > 0)
                ;
            else
                Serialize_Touch<T>(path);

            List<T> entries = new List<T>();
            var serializer = new XmlSerializer(entries.GetType());
            using (var reader = new StreamReader(path))
            {
                entries = (List<T>)serializer.Deserialize(reader);
            }

            return entries;
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

        public void Serialize_Update_M_TWINGLYACCOUNT_calls_free(Guid id_twinglyaccount_api_key, int calls_free, String path)
        {
            //
            List<M_TWINGLYACCOUNT> entries = new List<M_TWINGLYACCOUNT>();
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                // deserialize / serialize entries : read / add / write back
                using (var reader = new StreamReader(path))
                {
                    entries = (List<M_TWINGLYACCOUNT>)serializer.Deserialize(reader);
                }
            }

            // add
            entries.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == id_twinglyaccount_api_key).calls_free = calls_free;

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }

        /*public void Serialize_Update_M_TWINGLYACCOUNT_CurrentActive(Guid id_twinglyaccount_api_key, String currentActive, String path)
        {
            //
            List<M_TWINGLYACCOUNT> entries = new List<M_TWINGLYACCOUNT>();
            XmlSerializer serializer = new XmlSerializer(entries.GetType());

            //
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                // deserialize / serialize entries : read / add / write back
                using (var reader = new StreamReader(path))
                {
                    entries = (List<M_TWINGLYACCOUNT>)serializer.Deserialize(reader);
                }
            }

            // add
            entries.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == id_twinglyaccount_api_key).CurrentActive = currentActive;

            // write back
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }*/

        /*public void Serialize_Delete_Entry<T>(Guid id_entry, String path)
        {
            // deserialize / serialize entries : read / del / write back

            //
            List<T> entries = Deserialize2<T>(path);

            //
            RemoveItemFromList(entries, id_entry);

            // write back
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, entries);
                writer.Flush();
            }
        }*/

        public void Serialize_Delete_M_ARABIZIENTRY_Cascading(Guid id_arabizientry, String dataPath)
        {
            // load/deserialize M_ARABICDARIJAENTRY
            List<M_ARABICDARIJAENTRY> arabicdarijaentries = Deserialize<M_ARABICDARIJAENTRY>(dataPath);

            // filter on the one linked to current arabizi entry
            var arabicdarijaentry = arabicdarijaentries.Single(m => m.ID_ARABIZIENTRY == id_arabizientry);

            // load/deserialize data_M_ARABICDARIJAENTRY_TEXTENTITY
            // filter on the ones linked to current arabic darija entry
            // remove latin words
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = Deserialize<M_ARABICDARIJAENTRY_TEXTENTITY>(dataPath);
            var linkedtextEntities = textEntities.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
            foreach (var textEntity in linkedtextEntities)
            {
                RemoveItemFromList(textEntities, textEntity.ID_ARABICDARIJAENTRY_TEXTENTITY);
            }

            // load/deserialize M_ARABICDARIJAENTRY_LATINWORD
            // filter on the ones linked to current arabic darija entry
            // remove latin words
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordEntries = Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(dataPath);
            var linkedlatinWordEntries = latinWordEntries.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
            foreach (var latinWordEntry in linkedlatinWordEntries)
            {
                RemoveItemFromList(latinWordEntries, latinWordEntry.ID_ARABICDARIJAENTRY_LATINWORD);
            }

            // remove arabic darija
            RemoveItemFromList(arabicdarijaentries, arabicdarijaentry.ID_ARABICDARIJAENTRY);

            // remove arabizi
            List<M_ARABIZIENTRY> arabizientries = Deserialize<M_ARABIZIENTRY>(dataPath);
            RemoveItemFromList(arabizientries, id_arabizientry);

            // serialize back
            SerializeBack_dataPath<M_ARABICDARIJAENTRY_TEXTENTITY>(textEntities, dataPath);
            SerializeBack_dataPath<M_ARABICDARIJAENTRY_LATINWORD>(latinWordEntries, dataPath);
            SerializeBack_dataPath<M_ARABICDARIJAENTRY>(arabicdarijaentries, dataPath);
            SerializeBack_dataPath<M_ARABIZIENTRY>(arabizientries, dataPath);
        }

        #region BACK YARD BO
        public void RemoveItemFromList<T>(List<T> entries, Guid id_entry)
        {
            // find PK field name
            var member = "ID_" + typeof(T).Name.Substring(2);   // skip "M_"

            // remove
            entries.RemoveAll(m => id_entry == (Guid)GetProperty(m, member));
        }

        /*public List<T> Deserialize2<T>(String path)
        {
            List<T> entries = new List<T>();

            var serializer = new XmlSerializer(entries.GetType());
            using (var reader = new StreamReader(path))
            {
                entries = (List<T>)serializer.Deserialize(reader);
            }

            return entries;
        }*/

        public static object GetProperty(object o, string member)
        {
            if (o == null) throw new ArgumentNullException("o");
            if (member == null) throw new ArgumentNullException("member");
            Type scope = o.GetType();
            IDynamicMetaObjectProvider provider = o as IDynamicMetaObjectProvider;
            if (provider != null)
            {
                ParameterExpression param = Expression.Parameter(typeof(object));
                DynamicMetaObject mobj = provider.GetMetaObject(param);
                GetMemberBinder binder = (GetMemberBinder)Microsoft.CSharp.RuntimeBinder.Binder.GetMember(0, member, scope, new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(0, null) });
                DynamicMetaObject ret = mobj.BindGetMember(binder);
                BlockExpression final = Expression.Block(
                    Expression.Label(CallSiteBinder.UpdateLabel),
                    ret.Expression
                );
                LambdaExpression lambda = Expression.Lambda(final, param);
                Delegate del = lambda.Compile();
                return del.DynamicInvoke(o);
            }
            else
            {
                return o.GetType().GetProperty(member, BindingFlags.Public | BindingFlags.Instance).GetValue(o, null);
            }
        }
        #endregion
    }
}
