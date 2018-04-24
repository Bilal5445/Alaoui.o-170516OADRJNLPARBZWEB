using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using ArabicTextAnalyzer.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Configuration;
using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Models.Repository;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.BO;
using OADRJNLPCommon.Business;
using Exceptions;
using ArabicTextAnalyzer.Content.Resources;

namespace ArabicTextAnalyzer.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Create the Admn account using setiing in web.config (if needed)
            CreateAdminIfNeeded();

            // to avoid case when already logged in, but try to connect login page
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("IndexSplash", "Train");

            //
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                //
                // string callbackUrl = await SendEmailConfirmationTokenAsync(user);
                if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                {
                    ViewBag.errorMessage = "You must have a confirmed email to log on.";
                    return View("Error");
                }
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    goto createDefaultTheme;
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", R.InvalidLoginAttempt);
                    return View(model);
            }

        createDefaultTheme:
            // TMP DEV TIME : Create default theme if there is not any one before
            var userIdentity = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity;
            String userId = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
            var arabizer = new Arabizer();
            if (arabizer.loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId).Count == 0)
            {
                arabizer.saveserializeM_XTRCTTHEME_EFSQL(new M_XTRCTTHEME
                {
                    ID_XTRCTTHEME = Guid.NewGuid(),
                    CurrentActive = "active",
                    ThemeName = "Default",
                    UserID = userId
                });
            }

            // create Default App For Admin (and actually any new user) first time
            if (arabizer.loaddeserializeRegisterApp_DAPPERSQL(userId).Count == 0)
            {
                // create app to use the arabizi
                var appLimit = Convert.ToInt32(ConfigurationManager.AppSettings["TotalAppCallLimit"]);
                var app = new RegisterApp { Name = userId + ".app" };
                new AppManager().CreateApp(app, userId, false, new RegisterAppConcrete(), new ClientKeysConcrete(), appLimit);
            }

            // create registered user
            using (var db = new ArabiziDbContext())
            {
                var userguid = Guid.Parse(userId);
                var registeredUser = db.RegisterUsers.SingleOrDefault(m => m.UserGuid == userguid);
                if (registeredUser == null)
                {
                    db.RegisterUsers.Add(new RegisterUser
                    {
                        UserGuid = userguid,
                        LastLoginTime = DateTime.Now,
                        Username = model.Email,
                        Password = model.Password,
                        CreateOn = DateTime.Now,
                        EmailID = model.Email,
                    });
                }
                else
                    registeredUser.LastLoginTime = DateTime.Now;

                // commit
                db.SaveChanges();
            }

            // log login time
            using (var db = new ArabiziDbContext())
            {
                var userguid = Guid.Parse(userId);
                var registeredUser = db.RegisterUsers.SingleOrDefault(m => m.UserGuid == userguid);
                if (registeredUser == null)
                {
                    db.RegisterUsers.Add(new RegisterUser
                    {
                        UserGuid = userguid,
                        LastLoginTime = DateTime.Now,
                        Username = model.Email,
                        Password = model.Password,
                        CreateOn = DateTime.Now,
                        EmailID = model.Email,
                    });
                }
                else
                    registeredUser.LastLoginTime = DateTime.Now;

                // commit
                db.SaveChanges();
            }

            //
            // return RedirectToAction("Index", "Train");
            // return RedirectToAction("IndexTranslateArabizi", "Train");
            return RedirectToAction("IndexSplash", "Train");
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<ActionResult> Register(RegisterViewModel model)
          {
              if (ModelState.IsValid)
              {
                  var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName, fk_activity_id = model.fk_activity_id };

                  try
                  {
                      var result = await UserManager.CreateAsync(user, model.Password);
                      if (result.Succeeded)
                      {
                          // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                          // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                          // Send an email with this link
                          string callbackUrl = await SendEmailConfirmationTokenAsync(user);

                          // Uncomment to debug locally 
                          // TempData["ViewBagLink"] = callbackUrl;

                          ViewBag.Message = R.CheckYourMailAndConfirmEtc;

                          return View("Info");
                      }
                      AddErrors(result);
                  }
                  catch (InvalidApiRequestException ex)
                  {
                      Logging.Write(Server, ex.GetType().Name);
                      Logging.Write(Server, ex.Message);
                      Logging.Write(Server, ex.StackTrace);

                      // delere user if create
                      await UserManager.DeleteAsync(user);

                      throw new Exception(ex.Errors[0]);
                  }
                  catch (Exception ex)
                  {
                      Logging.Write(Server, ex.GetType().Name);
                      Logging.Write(Server, ex.Message);
                      Logging.Write(Server, ex.StackTrace);

                      // delere user if create
                      await UserManager.DeleteAsync(user);
                  }
              }

              // If we got this far, something failed, redisplay form
              return View(model);
          }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);

            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Host);
                callbackUrl = callbackUrl.Replace(Request.Url.Host + "://", "");    // MC230318 quick & dirty hack to get rid of the port 81
                // http://app.adekwasy.com:/fr-FR/Account/ResetPassword?userId=d0af39c5-5422-41fb-bd4a-9412c166a965&code=iLD3a6GYWf0lYUN3U9%2BYpCxQ0uz6agHNwBb8f0F%2FofhzdR35Kt0j%2FxHxGn14KoB6kstSvpO%2BgCy1l26ky89UkQ35i%2BVTvg2qsDfLAVKZ2PUBV33vE2HYkujPB3n3Zb5m1La9Ez%2Boi8ozXemPJ%2BlMFlmsfEV%2BBkViQxU22D9WEzpxaOgdhp9pLzsg7fWIJSB9giA7r2mAS5Q%2BbJ8Av36p2A%3D%3D
                await UserManager.SendEmailAsync(user.Id, R.ResetPassword, R.PleaseResetYourPasswordByClicking + " <a href='" + callbackUrl + "'>" + R.here + "</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            try
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                throw;
            }
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        // Utility
        // Add RoleManager
        #region public ApplicationRoleManager RoleManager
        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        // Add CreateAdminIfNeeded
        #region private void CreateAdminIfNeeded()
        private void CreateAdminIfNeeded()
        {
            // Get Admin Account
            string AdminUserName = ConfigurationManager.AppSettings["AdminUserName"];
            string AdminPassword = ConfigurationManager.AppSettings["AdminPassword"];
            // See if Admin exists
            var objAdminUser = UserManager.FindByEmail(AdminUserName);
            if (objAdminUser == null)
            {
                //See if the Admin role exists
                if (!RoleManager.RoleExists("Administrator"))
                {
                    // Create the Admin Role (if needed)
                    IdentityRole objAdminRole = new IdentityRole("Administrator");
                    RoleManager.Create(objAdminRole);
                }
                // Create Admin user
                var objNewAdminUser = new ApplicationUser { UserName = AdminUserName, Email = AdminUserName };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, AdminPassword);
                // Put user in Admin role
                UserManager.AddToRole(objNewAdminUser.Id, "Administrator");
            }
        }
        #endregion

        private async Task<string> SendEmailConfirmationTokenAsync(ApplicationUser user)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

            //
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Host);
            callbackUrl = callbackUrl.Replace(Request.Url.Host + "://", "");    // MC230318 quick & dirty hack to get rid of the port 81
            callbackUrl = "http://" + callbackUrl;                              // MC110418 another quick & dirty hack to reput http:// at the begineeing of the link

            //
            var message = "Bonjour, <br><br>"
                + "Nous vous remercions pour votre enregistrement sur le site web de Gravitas.<br><br>"
                + "Votre nom d'utilisateur est: <a href='mailto:" + user.Email + "' target='_blank'>" + user.Email + "</a>.<br><br>"
                + "Vous pouvez dorénavant vous connecter au site web pour accéder à votre espace personnel.<br><br>"
                + "Veuillez suivre ce lien pour activer votre compte:<br><br>"
                + callbackUrl + "<br><br> "
                + "Ce message est envoyé depuis une adresse technique, nous vous remercions de ne pas y répondre. Si vous désirez nous contacter, nous vous invitons à envoyer un mail à support@gravitas.ma.<br><br> "
                + "Bien Cordialement,<br><br>"
                + "L'équipe Gravitas";

            //
            await UserManager.SendEmailAsync(user.Id, "Confirmer votre compte", message);

            return callbackUrl;
        }
    }
}