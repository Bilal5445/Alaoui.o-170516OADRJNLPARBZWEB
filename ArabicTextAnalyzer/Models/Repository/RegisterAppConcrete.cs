using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Models.Repository
{
    public class RegisterAppConcrete : IRegisterApp
    {
        ArabiziDbContext _context;

        public RegisterAppConcrete()
        {
            _context = new ArabiziDbContext();
        }

        public IEnumerable<RegisterApp> ListofApps(string UserID)
        {
            try
            {
                var CompanyList =
                    (from companies in _context.RegisterApps
                     where companies.UserID == UserID
                     select companies).ToList();
                return CompanyList;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Add(RegisterApp entity)
        {
            // verify if the them exist already to db
            _context.RegisterApps.Add(entity);

            // save to db
            _context.SaveChanges();
        }

        public void Delete(RegisterApp entity)
        {
            try
            {
                var itemToRemove = _context.RegisterApps.SingleOrDefault(x => x.RegisterAppId == entity.RegisterAppId);
                _context.RegisterApps.Remove(itemToRemove);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public RegisterApp FindAppByUserId(string UserID)
        {
            try
            {
                var Company = _context.RegisterApps.SingleOrDefault(x => x.UserID == UserID);
                return Company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool ValidateAppName(RegisterApp registercompany)
        {
            try
            {
                var result = (from company in _context.RegisterApps
                              where company.Name == registercompany.Name
                              select company).Count();
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CheckIsAppRegistered(string UserID)
        {
            try
            {
                var companyExists = _context.RegisterApps.Any(x => x.UserID == UserID);

                if (companyExists)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
