using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Models.Repository
{
   public  class RegisterUserConcrete:IRegisterUser
    {
        ArabiziDbContext _context;
        public RegisterUserConcrete()
        {
            _context = new ArabiziDbContext();
        }

        public void Add(RegisterUser registeruser)
        {
            _context.RegisterUsers.Add(registeruser);
            _context.SaveChanges();
        }

        public int GetLoggedUserID(RegisterUser registeruser)
        {
            var usercount = (from User in _context.RegisterUsers
                             where User.Username == registeruser.Username && User.Password == registeruser.Password
                             select User.UserID).FirstOrDefault();

            return usercount;
        }

        public bool ValidateRegisteredUser(RegisterUser registeruser)
        {
            var usercount = (from User in _context.RegisterUsers
                             where User.Username == registeruser.Username && User.Password == registeruser.Password
                             select User).Count();
            if (usercount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ValidateUsername(RegisterUser registeruser)
        {
            var usercount = (from User in _context.RegisterUsers
                             where User.Username == registeruser.Username
                             select User).Count();
            if (usercount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
