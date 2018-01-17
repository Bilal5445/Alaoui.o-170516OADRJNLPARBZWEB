using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain.AES256Encryption;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ArabicTextAnalyzer.Models.Repository
{
    public class ClientKeysConcrete : IClientKeys
    {
        ArabiziDbContext _context;

        public ClientKeysConcrete()
        {
            _context = new ArabiziDbContext();
        }

        public void GenerateUniqueKey(out string ClientID, out string ClientSecret)
        {
            ClientID = EncryptionLibrary.KeyGenerator.GetUniqueKey();
            ClientSecret = EncryptionLibrary.KeyGenerator.GetUniqueKey();
        }

        public bool IsUniqueKeyAlreadyGenerate(string UserID)
        {
            bool keyExists = _context.ClientKeys.Any(clientkeys => clientkeys.UserID.Equals(UserID));

            if (keyExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int SaveClientIDandClientSecret(ClientKeys ClientKeys)
        {
            _context.ClientKeys.Add(ClientKeys);

            //
            return _context.SaveChanges();
        }

        public ClientKeys GetGenerateUniqueKeyByUserID(string UserID)
        {
            var clientkey = (from ckey in _context.ClientKeys
                             where ckey.UserID == UserID
                             select ckey).FirstOrDefault();

            //
            return clientkey;
        }

        public bool IsAppValid(ClientKeys clientkeys)
        {
            bool flags = true;
            var app = _context.RegisterApps.Where(c => c.RegisterAppId == clientkeys.RegisterAppId).FirstOrDefault();
            if (app != null)
            {
                if (app.TotalAppCallLimit == 0)
                {
                    flags = false;
                }
            }
            else
            {
            }

            return flags;
        }

        public int UpdateClientIDandClientSecret(ClientKeys ClientKeys)
        {
            _context.Entry(ClientKeys).State = EntityState.Modified;
            _context.SaveChanges();
            return _context.SaveChanges();
        }
    }
}
