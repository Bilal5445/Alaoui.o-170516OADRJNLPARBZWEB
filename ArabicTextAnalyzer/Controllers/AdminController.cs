﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using ArabicTextAnalyzer.Domain.Models;
using PagedList;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.BO;
using ArabicTextAnalyzer.Models.Repository;
using System.Configuration;
using ArabicTextAnalyzer.Business.Provider;
using OADRJNLPCommon.Models;
using OADRJNLPCommon.Business;

namespace ArabicTextAnalyzer.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        // Controllers

        // GET: /Admin/
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Index(string searchStringUserNameOrEmail)
        public ActionResult Index(string searchStringUserNameOrEmail, string currentFilter, int? page)
        {
            try
            {
                int intPage = 1;
                int intPageSize = 5;
                int intTotalItemCount = 0;

                //
                if (searchStringUserNameOrEmail != null)
                    intPage = 1;
                else if (currentFilter != null)
                {
                    searchStringUserNameOrEmail = currentFilter;
                    intPage = page ?? 1;
                }
                else
                {
                    searchStringUserNameOrEmail = "";
                    intPage = page ?? 1;
                }

                //
                ViewBag.CurrentFilter = searchStringUserNameOrEmail;
                List<ExpandedUserDTO> col_UserDTO = new List<ExpandedUserDTO>();
                int intSkip = (intPage - 1) * intPageSize;

                //
                var users = UserManager.Users.Where(x => x.UserName.Contains(searchStringUserNameOrEmail));
                var lusers = users.ToList();

                // get register apps to make a join with users and with registerusers to get get last login time
                var registerApps = new Arabizer().loaddeserializeRegisterApps_DAPPERSQL();
                var registerUsers = new Arabizer().loaddeserializeRegisterUsers_DAPPERSQL();
                var result0 = lusers.Join(registerApps, u => u.Id.ToUpper(), a => a.UserID.ToUpper(), (usr, app) => new
                {
                    app.UserID,
                    usr.UserName,
                    usr.Email,
                    usr.LockoutEndDateUtc,
                    app.TotalAppCallConsumed,
                    app.TotalAppCallLimit
                }).Join(registerUsers, r1 => r1.Email, u => u.EmailID, (res1, regusr) => new
                {
                    res1.UserID,
                    res1.UserName,
                    res1.Email,
                    res1.LockoutEndDateUtc,
                    res1.TotalAppCallConsumed,
                    res1.TotalAppCallLimit,
                    regusr.LastLoginTime
                });

                // themes count for the user
                var xtrctThemesCountPerUser = new Arabizer().loaddeserializeM_XTRCTTHEME_CountPerUser_DAPPERSQL();
                var result1 = result0.Join(xtrctThemesCountPerUser, r => r.UserID.ToUpper(), x => x.UserID.ToUpper(), (res2, xtcpu) => new
                {
                    res2.UserID,
                    res2.UserName,
                    res2.Email,
                    res2.LockoutEndDateUtc,
                    res2.TotalAppCallConsumed,
                    res2.TotalAppCallLimit,
                    res2.LastLoginTime,
                    xtcpu.CountPerUser
                });

                // fb pages count for the user & fb posts count for the user
                List<LM_CountPerTheme> fbPageCountPerTheme = new Arabizer().loaddeserializeT_FB_INFLUENCER_CountPerTheme_DAPPERSQL();
                List<M_XTRCTTHEME> xtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL();
                List<LM_CountPerTheme> fbPostsCountPerTheme = new Arabizer().loaddeserializeT_FB_POST_CountPerTheme_DAPPERSQL();
                List<LM_CountPerTheme> fbCommentsCountPerTheme = new Arabizer().loaddeserializeT_FB_Comments_CountPerTheme_DAPPERSQL();
                var usersToThemesToFbPagesCount = fbPageCountPerTheme.Join(
                    xtrctThemes,
                    fb => fb.fk_theme,
                    xt => xt.ID_XTRCTTHEME.ToString(),
                    (fb, xt) => new
                    {
                        fb.fk_theme,
                        fb.CountPerTheme,
                        fkUserID = xt.UserID
                    });
                var usersToThemesToFbPagesToFbPostsCount = usersToThemesToFbPagesCount.Join(
                    fbPostsCountPerTheme,
                    u => u.fk_theme,
                    fb => fb.fk_theme,
                    (u, fb) => new
                    {
                        u.fk_theme,
                        u.CountPerTheme,
                        u.fkUserID,
                        FBPostsCountPerUser = fb.CountPerTheme
                    });
                var usersToThemesToFbPagesToFbPostsToFBCommentsCount = usersToThemesToFbPagesToFbPostsCount.Join(
                    fbCommentsCountPerTheme,
                    u => u.fk_theme,
                    fb => fb.fk_theme,
                    (u, fb) => new
                    {
                        u.fk_theme,
                        u.CountPerTheme,
                        u.fkUserID,
                        u.FBPostsCountPerUser,
                        FBCommentsCountPerUser = fb.CountPerTheme
                    });
                var result3 = result1.GroupJoin(usersToThemesToFbPagesToFbPostsToFBCommentsCount,
                    x => x.UserID,
                    y => y.fkUserID, (x, y) => new
                    {
                        x.UserID,
                        x.UserName,
                        x.Email,
                        x.LockoutEndDateUtc,
                        x.TotalAppCallConsumed,
                        x.TotalAppCallLimit,
                        x.LastLoginTime,
                        ThemesCountPerUser = x.CountPerUser,
                        FBPagesCountPerUser = y.Sum(m => m.CountPerTheme),
                        FBPostsCountPerUser = y.Sum(m => m.FBPostsCountPerUser),
                        FBCommentsCountPerUser = y.Sum(m => m.FBCommentsCountPerUser)
                    });

                // darija entries count for the user
                var arEntriesCountPerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerUser_DAPPERSQL();
                var result = result3.Join(arEntriesCountPerUser,
                    x => x.UserID,
                    y => y.UserID, (x, y) => new
                    {
                        x.UserID,
                        x.UserName,
                        x.Email,
                        x.LockoutEndDateUtc,
                        x.TotalAppCallConsumed,
                        x.TotalAppCallLimit,
                        x.LastLoginTime,
                        x.ThemesCountPerUser,
                        x.FBPagesCountPerUser,
                        ArEntriesCountPerUser = y.CountPerUser,
                        x.FBPostsCountPerUser,
                        x.FBCommentsCountPerUser
                    });

                // items count
                intTotalItemCount = result.Count();

                // take the items for the current page only
                result = result
                    .OrderByDescending(x => x.LastLoginTime)
                    .Skip(intSkip)
                    .Take(intPageSize);

                //
                foreach (var item in result)
                {
                    ExpandedUserDTO objUserDTO = new ExpandedUserDTO();
                    objUserDTO.UserName = item.UserName;
                    objUserDTO.Email = item.Email;
                    objUserDTO.LockoutEndDateUtc = item.LockoutEndDateUtc;
                    objUserDTO.TotalAppCallLimit = item.TotalAppCallLimit;
                    objUserDTO.TotalAppCallConsumed = item.TotalAppCallConsumed;
                    objUserDTO.LastLoginTime = item.LastLoginTime;
                    objUserDTO.ThemesCountPerUser = item.ThemesCountPerUser;
                    objUserDTO.FBPagesCountPerUser = item.FBPagesCountPerUser;
                    objUserDTO.ArEntriesCountPerUser = item.ArEntriesCountPerUser;
                    objUserDTO.FBPostsCountPerUser = item.FBPostsCountPerUser;
                    objUserDTO.FBCommentsCountPerUser = item.FBCommentsCountPerUser;
                    col_UserDTO.Add(objUserDTO);
                }

                // Created the paged list and Set the number of pages
                var _UserDTOAsIPagedList = new StaticPagedList<ExpandedUserDTO>(col_UserDTO, intPage, intPageSize, intTotalItemCount);

                // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
                var userId = User.Identity.GetUserId();
                var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
                var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
                @ViewBag.UserXtrctThemes = userXtrctThemes;
                @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
                @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;

                // Fetch the data for fbPages for all themes for that user
                var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
                ViewBag.AllInfluenceVert = fbFluencerAsTheme;

                //
                return View(_UserDTOAsIPagedList);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                List<ExpandedUserDTO> col_UserDTO = new List<ExpandedUserDTO>();
                return View(col_UserDTO.ToPagedList(1, 25));
            }
        }
        #endregion

        [Authorize(Roles = "Administrator")]
        public ActionResult IndexFBs(string searchStringUserNameOrEmail, string currentFilter, int? page)
        {
            int intPage = 1;
            int intPageSize = 5;
            int intTotalItemCount = 0;

            //
            if (searchStringUserNameOrEmail != null)
                intPage = 1;
            else if (currentFilter != null)
            {
                searchStringUserNameOrEmail = currentFilter;
                intPage = page ?? 1;
            }
            else
            {
                searchStringUserNameOrEmail = "";
                intPage = page ?? 1;
            }

            //
            ViewBag.CurrentFilter = searchStringUserNameOrEmail;
            int intSkip = (intPage - 1) * intPageSize;

            //
            var users = UserManager.Users;
            var lusers = users.ToList();

            // themes for the users
            List<M_XTRCTTHEME> xtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL();

            // fb pages for the users
            List<T_FB_INFLUENCER> fbPages = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL();

            //
            var result0 = lusers.Join(xtrctThemes,
                    x => x.Id.ToUpper(),
                    y => y.UserID.ToUpper(), (x, y) => new
                    {
                        y.ThemeName,
                        y.CurrentActive,
                        x.UserName,
                        idXtrctTheme = y.ID_XTRCTTHEME.ToString()
                    });

            var result = result0.Join(fbPages,
                    x => x.idXtrctTheme,
                    y => y.fk_theme, (x, y) => new FBPageToThemeToUserViewModel
                    {
                        name = y.name,  // FB page name
                        AutoRetrieveFBPostAndComments = y.AutoRetrieveFBPostAndComments,
                        ThemeName = x.ThemeName,
                        CurrentActive = x.CurrentActive,
                        UserName = x.UserName
                    });

            result = result.Where(x => x.name.ToUpper().Contains(searchStringUserNameOrEmail.ToUpper()));

            // items count
            intTotalItemCount = result.Count();

            // take the items for the current page only
            result = result
                .OrderByDescending(x => x.name)
                .Skip(intSkip)
                .Take(intPageSize);

            // Created the paged list and Set the number of pages
            var pagedList = new StaticPagedList<FBPageToThemeToUserViewModel>(result, intPage, intPageSize, intTotalItemCount);

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userId = User.Identity.GetUserId();
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            //
            return View(pagedList);
        }

        // Users *****************************

        // GET: /Admin/Edit/Create 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult Create()
        public ActionResult Create()
        {
            ExpandedUserDTO objExpandedUserDTO = new ExpandedUserDTO();

            ViewBag.Roles = GetAllRolesAsSelectList();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userId = User.Identity.GetUserId();
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;

            return View(objExpandedUserDTO);
        }
        #endregion

        // PUT: /Admin/Create
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult Create(ExpandedUserDTO paramExpandedUserDTO)
        public ActionResult Create(ExpandedUserDTO paramExpandedUserDTO)
        {
            try
            {
                if (paramExpandedUserDTO == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var Email = paramExpandedUserDTO.Email.Trim();
                var UserName = paramExpandedUserDTO.Email.Trim();
                var Password = paramExpandedUserDTO.Password.Trim();

                if (Email == "")
                {
                    throw new Exception("No Email");
                }

                if (Password == "")
                {
                    throw new Exception("No Password");
                }

                // UserName is LowerCase of the Email
                UserName = Email.ToLower();

                // Create user
                var objNewAdminUser = new ApplicationUser { UserName = UserName, Email = Email };
                var AdminUserCreateResult = UserManager.Create(objNewAdminUser, Password);

                if (AdminUserCreateResult.Succeeded == true)
                {
                    string strNewRole = Convert.ToString(Request.Form["Roles"]);

                    // Put user in role
                    if (strNewRole != "0")
                        UserManager.AddToRole(objNewAdminUser.Id, strNewRole);

                    // create app to use the arabizi
                    var userId = objNewAdminUser.Id;
                    var appLimit = Convert.ToInt32(ConfigurationManager.AppSettings["TotalAppCallLimit"]);
                    var app = new RegisterApp { Name = userId + ".app" };
                    new AppManager().CreateApp(app, userId, false, new RegisterAppConcrete(), new ClientKeysConcrete(), appLimit);

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
                                Username = Email,
                                Password = Password,
                                CreateOn = DateTime.Now,
                                EmailID = Email,
                            });
                        }
                        else
                            registeredUser.LastLoginTime = DateTime.Now;

                        // commit
                        db.SaveChanges();
                    }

                    return Redirect("~/Admin");
                }
                else
                {
                    ViewBag.Roles = GetAllRolesAsSelectList();
                    ModelState.AddModelError(string.Empty, "Error: Failed to create the user. Check password requirements.");
                    return View(paramExpandedUserDTO);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Roles = GetAllRolesAsSelectList();
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("Create");
            }
        }
        #endregion

        // GET: /Admin/Edit/TestUser 
        [Authorize(Roles = "Administrator")]
        #region public ActionResult EditUser(string UserName)
        public ActionResult EditUser(string UserName)
        {
            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userId = User.Identity.GetUserId();
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            //
            if (UserName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExpandedUserDTO objExpandedUserDTO = GetUser(UserName);
            if (objExpandedUserDTO == null)
            {
                return HttpNotFound();
            }
            return View(objExpandedUserDTO);
        }
        #endregion

        // PUT: /Admin/EditUser
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditUser(ExpandedUserDTO paramExpandedUserDTO)
        public ActionResult EditUser(ExpandedUserDTO paramExpandedUserDTO)
        {
            try
            {
                // check before
                if (paramExpandedUserDTO == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                // real work
                ExpandedUserDTO objExpandedUserDTO = UpdateDTOUser(paramExpandedUserDTO);

                // check after
                if (objExpandedUserDTO == null)
                    return HttpNotFound();

                // create user in klipfolio
                String editorRole = "b6ddda3ff84aef09d6c1cd12596f1687";
                String email = paramExpandedUserDTO.Email;
                String first_name = Guid.NewGuid().ToString();
                String last_name = "Namate";
                String jsondata = "{"
                        + "'first_name': '" + first_name + "', "
                        + "'last_name': '" + last_name + "', "
                        + "'email': '" + email + "', "
                        + "'roles': ['" + editorRole + "'] "
                    + " }";
                var header = new Dictionary<string, string> { { "kf-api-key", "8be9c7ab65ac9f1363e1ecf5ef485164bddf4c77" } };
                // Dictionary<String, String> header = new Dictionary<string, string>();
                // header.Add("kf-api-key", "8be9c7ab65ac9f1363e1ecf5ef485164bddf4c77");
                // HtmlHelpers.Post("https://app.klipfolio.com/api/1/users", jsondata, header);
                var res = HtmlHelpers.PostBasicAuth("https://app.klipfolio.com/api/1/users", jsondata, header);
                Logging.Write(Server, res);

                //
                return Redirect("~/Admin");
            }
            catch (Exception ex)
            {
                //
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                //
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditUser", GetUser(paramExpandedUserDTO.UserName));
            }
        }
        #endregion

        // DELETE: /Admin/DeleteUser
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteUser(string UserName)
        public ActionResult DeleteUser(string UserName)
        {
            try
            {
                if (UserName == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (UserName.ToLower() == this.User.Identity.Name.ToLower())
                {
                    ModelState.AddModelError(string.Empty, "Error: Cannot delete the current user");

                    return View("EditUser");
                }

                ExpandedUserDTO objExpandedUserDTO = GetUser(UserName);

                if (objExpandedUserDTO == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    DeleteUser(objExpandedUserDTO);
                }

                return Redirect("~/Admin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditUser", GetUser(UserName));
            }
        }
        #endregion

        // GET: /Admin/EditRoles/TestUser 
        [Authorize(Roles = "Administrator")]
        #region ActionResult EditRoles(string UserName)
        public ActionResult EditRoles(string UserName)
        {
            if (UserName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            UserName = UserName.ToLower();

            // Check that we have an actual user
            ExpandedUserDTO objExpandedUserDTO = GetUser(UserName);

            if (objExpandedUserDTO == null)
            {
                return HttpNotFound();
            }

            UserAndRolesDTO objUserAndRolesDTO =
                GetUserAndRoles(UserName);

            return View(objUserAndRolesDTO);
        }
        #endregion

        // PUT: /Admin/EditRoles/TestUser 
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult EditRoles(UserAndRolesDTO paramUserAndRolesDTO)
        public ActionResult EditRoles(UserAndRolesDTO paramUserAndRolesDTO)
        {
            try
            {
                if (paramUserAndRolesDTO == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                string UserName = paramUserAndRolesDTO.UserName;
                string strNewRole = Convert.ToString(Request.Form["AddRole"]);

                if (strNewRole != "No Roles Found")
                {
                    // Go get the User
                    ApplicationUser user = UserManager.FindByName(UserName);

                    // Put user in role
                    UserManager.AddToRole(user.Id, strNewRole);
                }

                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));

                UserAndRolesDTO objUserAndRolesDTO =
                    GetUserAndRoles(UserName);

                return View(objUserAndRolesDTO);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("EditRoles");
            }
        }
        #endregion

        // DELETE: /Admin/DeleteRole?UserName="TestUser&RoleName=Administrator
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteRole(string UserName, string RoleName)
        public ActionResult DeleteRole(string UserName, string RoleName)
        {
            try
            {
                if ((UserName == null) || (RoleName == null))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                UserName = UserName.ToLower();

                // Check that we have an actual user
                ExpandedUserDTO objExpandedUserDTO = GetUser(UserName);

                if (objExpandedUserDTO == null)
                {
                    return HttpNotFound();
                }

                if (UserName.ToLower() ==
                    this.User.Identity.Name.ToLower() && RoleName == "Administrator")
                {
                    ModelState.AddModelError(string.Empty,
                        "Error: Cannot delete Administrator Role for the current user");
                }

                // Go get the User
                ApplicationUser user = UserManager.FindByName(UserName);
                // Remove User from role
                UserManager.RemoveFromRoles(user.Id, RoleName);
                UserManager.Update(user);

                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));

                return RedirectToAction("EditRoles", new { UserName = UserName });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);

                ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));

                UserAndRolesDTO objUserAndRolesDTO =
                    GetUserAndRoles(UserName);

                return View("EditRoles", objUserAndRolesDTO);
            }
        }
        #endregion

        // Roles *****************************

        // GET: /Admin/ViewAllRoles
        [Authorize(Roles = "Administrator")]
        #region public ActionResult ViewAllRoles()
        public ActionResult ViewAllRoles()
        {
            var roleManager =
                new RoleManager<IdentityRole>
                (
                    new RoleStore<IdentityRole>(new ApplicationDbContext())
                    );

            List<RoleDTO> colRoleDTO = (from objRole in roleManager.Roles
                                        select new RoleDTO
                                        {
                                            Id = objRole.Id,
                                            RoleName = objRole.Name
                                        }).ToList();

            return View(colRoleDTO);
        }
        #endregion

        // GET: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        #region public ActionResult AddRole()
        public ActionResult AddRole()
        {
            RoleDTO objRoleDTO = new RoleDTO();

            return View(objRoleDTO);
        }
        #endregion

        // PUT: /Admin/AddRole
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        #region public ActionResult AddRole(RoleDTO paramRoleDTO)
        public ActionResult AddRole(RoleDTO paramRoleDTO)
        {
            try
            {
                if (paramRoleDTO == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var RoleName = paramRoleDTO.RoleName.Trim();

                if (RoleName == "")
                {
                    throw new Exception("No RoleName");
                }

                // Create Role
                var roleManager =
                    new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(new ApplicationDbContext())
                        );

                if (!roleManager.RoleExists(RoleName))
                {
                    roleManager.Create(new IdentityRole(RoleName));
                }

                return Redirect("~/Admin/ViewAllRoles");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);
                return View("AddRole");
            }
        }
        #endregion

        // DELETE: /Admin/DeleteUserRole?RoleName=TestRole
        [Authorize(Roles = "Administrator")]
        #region public ActionResult DeleteUserRole(string RoleName)
        public ActionResult DeleteUserRole(string RoleName)
        {
            try
            {
                if (RoleName == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (RoleName.ToLower() == "administrator")
                {
                    throw new Exception(String.Format("Cannot delete {0} Role.", RoleName));
                }

                var roleManager =
                    new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(new ApplicationDbContext()));

                var UsersInRole = roleManager.FindByName(RoleName).Users.Count();
                if (UsersInRole > 0)
                {
                    throw new Exception(
                        String.Format(
                            "Canot delete {0} Role because it still has users.",
                            RoleName)
                            );
                }

                var objRoleToDelete = (from objRole in roleManager.Roles
                                       where objRole.Name == RoleName
                                       select objRole).FirstOrDefault();
                if (objRoleToDelete != null)
                {
                    roleManager.Delete(objRoleToDelete);
                }
                else
                {
                    throw new Exception(
                        String.Format(
                            "Canot delete {0} Role does not exist.",
                            RoleName)
                            );
                }

                List<RoleDTO> colRoleDTO = (from objRole in roleManager.Roles
                                            select new RoleDTO
                                            {
                                                Id = objRole.Id,
                                                RoleName = objRole.Name
                                            }).ToList();

                return View("ViewAllRoles", colRoleDTO);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error: " + ex);

                var roleManager =
                    new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(new ApplicationDbContext()));

                List<RoleDTO> colRoleDTO = (from objRole in roleManager.Roles
                                            select new RoleDTO
                                            {
                                                Id = objRole.Id,
                                                RoleName = objRole.Name
                                            }).ToList();

                return View("ViewAllRoles", colRoleDTO);
            }
        }
        #endregion


        // Utility

        #region public ApplicationUserManager UserManager
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
        #endregion

        #region public ApplicationRoleManager RoleManager
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ??
                    HttpContext.GetOwinContext()
                    .GetUserManager<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        #endregion

        #region private List<SelectListItem> GetAllRolesAsSelectList()
        private List<SelectListItem> GetAllRolesAsSelectList()
        {
            List<SelectListItem> SelectRoleListItems =
                new List<SelectListItem>();

            var roleManager =
                new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(new ApplicationDbContext()));

            var colRoleSelectList = roleManager.Roles.OrderBy(x => x.Name).ToList();

            SelectRoleListItems.Add(
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0"
                });

            foreach (var item in colRoleSelectList)
            {
                SelectRoleListItems.Add(
                    new SelectListItem
                    {
                        Text = item.Name.ToString(),
                        Value = item.Name.ToString()
                    });
            }

            return SelectRoleListItems;
        }
        #endregion

        #region private ExpandedUserDTO GetUser(string paramUserName)
        private ExpandedUserDTO GetUser(string paramUserName)
        {
            ExpandedUserDTO objExpandedUserDTO = new ExpandedUserDTO();

            var result = UserManager.FindByName(paramUserName);

            // If we could not find the user, throw an exception
            if (result == null) throw new Exception("Could not find the User");

            objExpandedUserDTO.UserName = result.UserName;
            objExpandedUserDTO.Email = result.Email;
            objExpandedUserDTO.LockoutEndDateUtc = result.LockoutEndDateUtc;
            objExpandedUserDTO.AccessFailedCount = result.AccessFailedCount;
            objExpandedUserDTO.PhoneNumber = result.PhoneNumber;

            return objExpandedUserDTO;
        }
        #endregion

        #region private ExpandedUserDTO UpdateDTOUser(ExpandedUserDTO objExpandedUserDTO)
        private ExpandedUserDTO UpdateDTOUser(ExpandedUserDTO paramExpandedUserDTO)
        {
            ApplicationUser result =
                UserManager.FindByName(paramExpandedUserDTO.UserName);

            // If we could not find the user, throw an exception
            if (result == null)
            {
                throw new Exception("Could not find the User");
            }

            result.Email = paramExpandedUserDTO.Email;

            // Lets check if the account needs to be unlocked
            if (UserManager.IsLockedOut(result.Id))
            {
                // Unlock user
                UserManager.ResetAccessFailedCountAsync(result.Id);
            }

            UserManager.Update(result);

            // Was a password sent across?
            if (!string.IsNullOrEmpty(paramExpandedUserDTO.Password))
            {
                // Remove current password
                var removePassword = UserManager.RemovePassword(result.Id);
                if (removePassword.Succeeded)
                {
                    // Add new password
                    var AddPassword =
                        UserManager.AddPassword(
                            result.Id,
                            paramExpandedUserDTO.Password
                            );

                    if (AddPassword.Errors.Count() > 0)
                    {
                        throw new Exception(AddPassword.Errors.FirstOrDefault());
                    }
                }
            }

            return paramExpandedUserDTO;
        }
        #endregion

        #region private void DeleteUser(ExpandedUserDTO paramExpandedUserDTO)
        private void DeleteUser(ExpandedUserDTO paramExpandedUserDTO)
        {
            ApplicationUser user = UserManager.FindByName(paramExpandedUserDTO.UserName);

            // If we could not find the user, throw an exception
            if (user == null)
            {
                throw new Exception("Could not find the User");
            }

            UserManager.RemoveFromRoles(user.Id, UserManager.GetRoles(user.Id).ToArray());
            UserManager.Update(user);
            UserManager.Delete(user);
        }
        #endregion

        #region private UserAndRolesDTO GetUserAndRoles(string UserName)
        private UserAndRolesDTO GetUserAndRoles(string UserName)
        {
            // Go get the User
            ApplicationUser user = UserManager.FindByName(UserName);

            List<UserRoleDTO> colUserRoleDTO =
                (from objRole in UserManager.GetRoles(user.Id)
                 select new UserRoleDTO
                 {
                     RoleName = objRole,
                     UserName = UserName
                 }).ToList();

            if (colUserRoleDTO.Count() == 0)
            {
                colUserRoleDTO.Add(new UserRoleDTO { RoleName = "No Roles Found" });
            }

            ViewBag.AddRole = new SelectList(RolesUserIsNotIn(UserName));

            // Create UserRolesAndPermissionsDTO
            UserAndRolesDTO objUserAndRolesDTO =
                new UserAndRolesDTO();
            objUserAndRolesDTO.UserName = UserName;
            objUserAndRolesDTO.colUserRoleDTO = colUserRoleDTO;
            return objUserAndRolesDTO;
        }
        #endregion

        #region private List<string> RolesUserIsNotIn(string UserName)
        private List<string> RolesUserIsNotIn(string UserName)
        {
            // Get roles the user is not in
            var colAllRoles = RoleManager.Roles.Select(x => x.Name).ToList();

            // Go get the roles for an individual
            ApplicationUser user = UserManager.FindByName(UserName);

            // If we could not find the user, throw an exception
            if (user == null)
            {
                throw new Exception("Could not find the User");
            }

            var colRolesForUser = UserManager.GetRoles(user.Id).ToList();
            var colRolesUserInNotIn = (from objRole in colAllRoles
                                       where !colRolesForUser.Contains(objRole)
                                       select objRole).ToList();

            if (colRolesUserInNotIn.Count() == 0)
            {
                colRolesUserInNotIn.Add("No Roles Found");
            }

            return colRolesUserInNotIn;
        }
        #endregion
    }
}