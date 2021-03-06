﻿using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models.Repository;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.ViewModels;
using Dapper;
using Newtonsoft.Json;
using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using OADRJNLPCommon.Models;
using System.Threading.Tasks;
using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.BO;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Web.SessionState;
using ArabicTextAnalyzer.Content.Resources;
using MimeKit;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity.Owin;
using static ArabicTextAnalyzer.BO.Arabizer;
// using ScrapyWeb.Business;

namespace ArabicTextAnalyzer.Controllers
{
    [Authorize]
    // [AuthenticateFilter]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class TrainController : BaseController
    {
        // global lock to queur concurrent access
        private static Object thisLock = new Object();

        // authentication in constructor
        IAuthenticate _IAuthenticate;
        IRegisterApp _IRegister;
        public TrainController()
        {
            _IAuthenticate = new AuthenticateConcrete();
            _IRegister = new RegisterAppConcrete();
        }

        // data access mode (ef-sql, dapper-sql, xml)
        // AccessMode _accessMode = AccessMode.dappersql;

        // GET: Train
        [Authorize]
        public ActionResult Index()
        {
            //
            var userId = User.Identity.GetUserId();

            // token management
            ManageToken(userId);

            //
            string errMessage = string.Empty;
            List<RegisterApp> _registerApp = _IRegister.ListofApps(userId).ToList();
            ViewBag.registerApp = _registerApp;

            // send size of corpus & co-data
            @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
            @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();
            @ViewBag.BidictSize = new TextFrequency().GetBidictNumberOfLine();
            var arabiziEntriesCount = loaddeserializeM_ARABIZIENTRY_DAPPERSQL().Count;
            @ViewBag.ArabiziEntriesCount = arabiziEntriesCount;
            @ViewBag.RatioLatinWordsOnEntries = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL().Where(m => String.IsNullOrWhiteSpace(m.Translation) == true).ToList().Count / (double)arabiziEntriesCount;

            @ViewBag.EntitiesCount = new TextFrequency().GetEntitiesCount();

            // deserialize/send twingly accounts
            @ViewBag.TwinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // communication
            @ViewBag.showAlertWarning = TempData["showAlertWarning"] != null ? TempData["showAlertWarning"] : false;
            @ViewBag.showAlertSuccess = TempData["showAlertSuccess"] != null ? TempData["showAlertSuccess"] : false;
            @ViewBag.msgAlert = TempData["msgAlert"] != null ? TempData["msgAlert"] : String.Empty;
            TempData.Remove("showAlertWarning");
            TempData.Remove("showAlertSuccess");
            TempData.Remove("msgAlert");

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            //
            @ViewBag.NersWithCounts = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_StatAllNersCounts_DAPPERSQL();

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            //
            return View();
        }

        [Authorize]
        public ActionResult IndexTranslateArabizi()
        {
            //
            var userId = User.Identity.GetUserId();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            return View();
        }

        [AllowAnonymous]
        public ActionResult IndexTranslateArabiziFree()
        {
            return View();
        }

        [Authorize]
        // [NonAction] // tmp
        public ActionResult IndexStats()
        {
            //
            var userId = User.Identity.GetUserId();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            // chart.js NER per theme
            var statNerCountPerTheme = new Arabizer().StatNerCountPerTheme(userActiveXtrctTheme.ID_XTRCTTHEME.ToString());
            ViewBag.Data = String.Join(",", statNerCountPerTheme.Select(m => m.CountPerKeyword));
            ViewBag.ObjectName = String.Join(",", statNerCountPerTheme.Select(m => "'" + m.Keyword + "'")); //list of "strings" that you need to show on the chart

            // charts.js NER types per theme
            var statNerTypeCountPerTheme = new Arabizer().StatNerTypeCountPerTheme(userActiveXtrctTheme.ID_XTRCTTHEME.ToString());
            ViewBag.DataNerTypeCountPerTheme = String.Join(",", statNerTypeCountPerTheme.Select(m => m.CountPerKeyword));
            ViewBag.ObjectNameNerTypeCountPerTheme = String.Join(",", statNerTypeCountPerTheme.Select(m => "'" + m.Keyword + "'"));

            // charts.js SA per theme
            var statSACountPerTheme = new Arabizer().StatSACountPerTheme(userActiveXtrctTheme.ID_XTRCTTHEME.ToString());
            ViewBag.DataSACountPerTheme = String.Join(",", statSACountPerTheme.Select(m => m.CountPerKeyword));
            ViewBag.ObjectNameSACountPerTheme = String.Join(",", statSACountPerTheme.Select(m => "'" + m.Keyword + "'"));

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            return View();
        }

        [Authorize]
        // [NonAction] // tmp
        public ActionResult IndexFBs(String idXtrctTheme = null)
        {
            //
            var userId = User.Identity.GetUserId();

            // when called without parameters optional idXtrctTheme == Guid.Empty will be true
            // if not empty change theme
            if (idXtrctTheme != null)
            {
                ApplyNewActiveTheme(new Guid(idXtrctTheme), userId);
            }

            // token management
            ManageToken(userId);

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            return View();
        }

        [Authorize]
        // [NonAction] // tmp
        public ActionResult IndexSocialSearch()
        {
            //
            var userId = User.Identity.GetUserId();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            //
            @ViewBag.NersWithCounts = new Arabizer().loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_StatAllNersCounts_SocialSearch_DAPPERSQL();
            @ViewBag.FBPagesWithCommentsCounts = new Arabizer().loaddeserializeT_FB_INFLUENCER_CommentsCount_DAPPERSQL();

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            return View();
        }

        [Authorize]
        public ActionResult IndexSplash()
        {
            //
            var userId = User.Identity.GetUserId();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var userXtrctThemes = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new Arabizer().loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL(userId);
            var userActiveXtrctTheme = userXtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.UserXtrctThemes = userXtrctThemes;
            @ViewBag.XtrctThemesPlain = userXtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == userActiveXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.UserActiveXtrctTheme = userActiveXtrctTheme;
            @ViewBag.ActiveXtrctThemeNegTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "NEGATIVE" || m.Keyword_Type == "OPPOSE" || m.Keyword_Type == "EXPLETIVE" || m.Keyword_Type == "SENSITIVE").ToList();
            @ViewBag.ActiveXtrctThemePosTags = xtrctThemesKeywords.Where(m => m.Keyword_Type == "POSITIVE" || m.Keyword_Type == "SUPPORT").ToList();
            @ViewBag.ActiveXtrctThemeOtherTags = xtrctThemesKeywords.Where(m => m.Keyword_Type != "POSITIVE" && m.Keyword_Type != "SUPPORT" && m.Keyword_Type != "NEGATIVE" && m.Keyword_Type != "OPPOSE" && m.Keyword_Type != "EXPLETIVE" && m.Keyword_Type != "SENSITIVE").ToList();

            // Fetch the data for fbPages for all themes for that user
            var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_DAPPERSQL(userId);
            ViewBag.AllInfluenceVert = fbFluencerAsTheme;

            // working data entries count
            List<LM_CountPerThemePerUser> entriesCountsperThemePerUser = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_CountPerThemePerUser_DAPPERSQL(userId);
            @ViewBag.EntriesCountsperThemePerUser = entriesCountsperThemePerUser;

            // fb pages comments count
            List<LM_CountPerInfluencer> commentsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Comments_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.CommentsCountsPerInfluencer = commentsCountsPerInfluencer;

            // fb pages posts count
            List<LM_CountPerInfluencer> postsCountsPerInfluencer = new Arabizer().loaddeserializeT_FB_Posts_CountPerInfluencer_DAPPERSQL(userId);
            @ViewBag.PostsCountsPerInfluencer = postsCountsPerInfluencer;

            // scocial searc posys count & comment count
            @ViewBag.PostsCounts = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();
            @ViewBag.CommentsCounts = new Arabizer().loaddeserializeT_FB_Comments_Count_DAPPERSQL();

            // DBG
            ViewBag.UserName = User.Identity.GetUserName();

            return View();
        }

        [HttpPost]
        public ActionResult ArabicDarijaEntryPartialView(bool adminModeShowAll = false, PartialViewType partialViewType = PartialViewType.all)
        {
            try
            {
                var token = Session["_T0k@n_"];
                // if (token != null && !string.IsNullOrEmpty(token as string))
                {
                    if (partialViewType == PartialViewType.all || partialViewType == PartialViewType.FBPagesOnly)
                    {
                        // Fetch the data for fbPage as only for that theme
                        var userId = User.Identity.GetUserId();
                        var fbFluencerAsTheme = new Arabizer().loadDeserializeT_FB_INFLUENCERs_ActiveTheme_DAPPERSQL(userId);
                        ViewBag.AllInfluence = fbFluencerAsTheme;
                    }

                    // pass partialViewType
                    ViewBag.PartialViewType = partialViewType;

                    // pass adminModeShowAll
                    ViewBag.AdminModeShowAll = adminModeShowAll;
                }

                // pass entries to partial view via the model (instead of the bag for a view)
                return PartialView("_IndexPartialPage_arabicDarijaEntries");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        #region FRONT YARD ACTIONS TRAIN
        [Authorize]
        [HttpPost]
        public ActionResult TrainStepOne(M_ARABIZIENTRY arabiziEntry, String mainEntity)
        {
            try
            {
                // make sure last day did not exceed 100 calls
                var userId = User.Identity.GetUserId();
                var clientKeyToolkit = new ClientKeysConcrete();
                var clientkeys = clientKeyToolkit.GetGenerateUniqueKeyByUserID(userId);
                int nbrOfCallsInTheLast24hour = new Arabizer().getNbrOfCallsInTheLast24hours(clientkeys.RegisterAppId.Value);
                if (nbrOfCallsInTheLast24hour > 100)
                {
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = R.DailyConsumptionExceeded;
                    return RedirectToAction("Index");
                }

                // token management
                var token = Session["_T0k@n_"];
                bool istokenexpire = _IAuthenticate.IsTokenExpire(Convert.ToString(token));
                if (token == null || istokenexpire)
                {
                    // MC191217 tokens are good to track who calls the arabizi function (api or web app), but web app user does not need to generate manually a token,
                    // the fact that he is logged on, means he should be able to use translation, means a token should be always ready for him. Solution (workaround?) generate
                    // on the fly a new token each time the token is expired

                    // generate token (and fill session token)
                    string message = string.Empty;
                    bool isAppValid = clientKeyToolkit.IsAppValid(clientkeys);
                    if (isAppValid == false)
                        message = "No More calls";
                    else
                    {
                        String sessiontoken;
                        String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                        var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out sessiontoken);
                        //TMP Session["_T0k@n_"] = sessiontoken;
                        token = sessiontoken;
                        if (String.IsNullOrEmpty(sessiontoken))
                            message = tokenmessage; // pass message only if token not generated
                    }
                }

                // 
                string errMessage = string.Empty;
                if (token == null || !_IAuthenticate.IsTokenValid(Convert.ToString(token), "TrainStepOne", out errMessage))
                {
                    // Session["_T0k@n_"] = String.Empty;
                    // Session["message"] = String.Empty;
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = errMessage;  // "Not a valid token";
                    return RedirectToAction("Index");
                }

                // Arabizi to arabic script via direct call to perl script
                var res = new Arabizer(Server).train(arabiziEntry, mainEntity, thisLock: thisLock);   // count time
                if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY == Guid.Empty)
                {
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = "Text is required.";
                    return RedirectToAction("Index");
                }

                //
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                throw;
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult TrainStepOneAjax(M_ARABIZIENTRY arabiziEntry, String mainEntity)
        {
            try
            {
                // check before
                if (arabiziEntry == null || String.IsNullOrWhiteSpace(mainEntity))
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Invalid Parameter"
                    }), "application/json");
                }

                // make sure last day did not exceed 100 calls
                var userId = User.Identity.GetUserId();
                var clientkeys = new ClientKeysConcrete().GetGenerateUniqueKeyByUserID(userId);
                int nbrOfCallsInTheLast24hour = new Arabizer().getNbrOfCallsInTheLast24hours(clientkeys.RegisterAppId.Value);
                if (nbrOfCallsInTheLast24hour > 100)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = R.DailyConsumptionExceeded
                    }), "application/json");
                }

                // consume one call : IsTokenValid check token and consumes one call : means increment registerApp.TotalAppCallConsumed
                String token, errMessage;
                String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out token);
                if (_IAuthenticate.IsTokenValid(Convert.ToString(token), "TrainStepOneAjax", out errMessage) == false)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = errMessage
                    }), "application/json");
                }

                // Arabizi to arabic script via direct call to perl script
                var res = new Arabizer(Server).train(arabiziEntry, mainEntity, thisLock: thisLock);   // count time
                if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY == Guid.Empty)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Text is required."
                    }), "application/json");
                }

                //
                var json = JsonConvert.SerializeObject(res);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                }), "application/json");
            }
        }

        private ApplicationUserManager _userManager;
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

        [AllowAnonymous]
        [HttpPost]
        public ActionResult TrainStepOneAjaxFree(M_ARABIZIENTRY arabiziEntry)
        {
            var hasUsedCookie = Request.Cookies["hasUsed"];
            if (hasUsedCookie == null)
            {
                // never used, create cookie and set its value to 1
                Response.Cookies.Add(new HttpCookie("hasUsed", 1.ToString()));
            }
            else if (Convert.ToInt32(hasUsedCookie.Value) > 2)
            {
                // cookie value more than 2, then returns saying quota consumed
                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Free quota used, please register."
                }), "application/json");
            }
            else
            {
                // cookie value less than or equal to 2, increase cookie value
                Response.Cookies.Add(new HttpCookie("hasUsed", (Convert.ToInt32(hasUsedCookie.Value) + 1).ToString()));
            }

            // since it is a free test for anybody, pass the user id as admin : Get Admin Account
            string AdminUserName = ConfigurationManager.AppSettings["AdminUserName"];
            string AdminPassword = ConfigurationManager.AppSettings["AdminPassword"];
            var objAdminUser = UserManager.FindByEmail(AdminUserName);
            var userId = objAdminUser.Id;

            // since it is a free test for anybody, passe the user id as admin and pass the theme id as admin active theme
            var adminActiveTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
            String mainEntity = adminActiveTheme.ThemeName;
            arabiziEntry.ID_XTRCTTHEME = adminActiveTheme.ID_XTRCTTHEME;

            try
            {
                // check before
                if (arabiziEntry == null || String.IsNullOrWhiteSpace(mainEntity))
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Invalid Parameter"
                    }), "application/json");
                }

                // make sure last day did not exceed 100 calls
                var clientkeys = new ClientKeysConcrete().GetGenerateUniqueKeyByUserID(userId);
                int nbrOfCallsInTheLast24hour = new Arabizer().getNbrOfCallsInTheLast24hours(clientkeys.RegisterAppId.Value);
                if (nbrOfCallsInTheLast24hour > 100)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = R.DailyConsumptionExceeded
                    }), "application/json");
                }

                // consume one call : IsTokenValid check token and consumes one call : means increment registerApp.TotalAppCallConsumed
                String token, errMessage;
                String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out token);
                if (_IAuthenticate.IsTokenValid(Convert.ToString(token), "TrainStepOneAjax", out errMessage) == false)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = errMessage
                    }), "application/json");
                }

                // Arabizi to arabic script via direct call to perl script
                var res = new Arabizer(Server).train(arabiziEntry, mainEntity, thisLock: thisLock);   // count time
                if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY == Guid.Empty)
                {
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Text is required."
                    }), "application/json");
                }

                //
                var json = JsonConvert.SerializeObject(res);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                }), "application/json");
            }
        }

        [HttpGet]
        public ActionResult Train_AddToCorpus(String arabiziWord, Guid arabiziWordGuid)
        {
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiUrl = "https://data.twingly.net/socialfeed/a/api/";

            // obtain current twingly key
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();
            String twinglyApiKey = twinglyAccounts.Find(m => m.CurrentActive == "active").ID_TWINGLYACCOUNT_API_KEY.ToString();

            // deserialize M_ARABICDARIJAENTRY_LATINWORD to check if most popular already found
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();
            var latinWordsEntry = latinWordsEntries.Find(m => m.ID_ARABICDARIJAENTRY_LATINWORD == arabiziWordGuid);
            if (latinWordsEntry == null)
            {
                ViewBag.MostPopularVariant = "No M_ARABICDARIJAENTRY_LATINWORD found";

                return RedirectToAction("Index");
            }
            if (String.IsNullOrEmpty(latinWordsEntry.MostPopularVariant) == false)
            {
                // ViewBag.MostPopularVariant = "MostPopularVariant already found";
                // return View("Index");
                // we already have the most popular
                // if not in corpus let us add it via twningly
                if (new TextFrequency().CorpusContainsWord(latinWordsEntry.MostPopularVariant))
                {
                    ViewBag.MostPopularVariant = "MostPopularVariant already found attached to latin word, and already in corpus. Try to convert again";

                    return RedirectToAction("Index");
                }
                else
                {
                    // lets look for a post via twigly to add to corpus
                    // 7 get a post containing this keyword
                    var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(latinWordsEntry.MostPopularVariant, twinglyApi15Url, twinglyApiKey, true);
                    if (postText == String.Empty) // if no results, look everywhere
                        postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(latinWordsEntry.MostPopularVariant, twinglyApi15Url, twinglyApiKey, false);
                    // upddate count twingly account
                    new TwinglyTools().upddateCountTwinglyAccount(twinglyApiUrl, twinglyApiKey, Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt"));
                    //
                    if (String.IsNullOrEmpty(postText))
                    {
                        ViewBag.MostPopularVariant = "No post found containing the most popular variant";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // 8 add this post to dict
                        var textFrequency = new TextFrequency();
                        if (textFrequency.CorpusContainsSentence(postText) == false)
                            textFrequency.AddPhraseToCorpus(postText);

                        // 9 recompile the dict
                        textConverter.CatCorpusDict();  // 5s
                        textConverter.SrilmLmDict();    // 1mn25s => 55s

                        ViewBag.MostPopularVariant = "New post added into corpus with the most popular variant. Try to convert again";
                        ViewBag.Post = postText;

                        return RedirectToAction("Index");
                    }
                }
            }

            // standard behaviour when we are about to find the right variant
            var variants = textConverter.GetAllTranscriptions(arabiziWord);
            if (variants.Count > 100)
            {
                ViewBag.MostPopularVariant = "More than 100 variants";

                return RedirectToAction("Index");
            }
            else
            {
                // 5 get most popular keyword
                var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

                // upddate count twingly account
                new TwinglyTools().upddateCountTwinglyAccount(twinglyApiUrl, twinglyApiKey, Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt"));

                //
                if (String.IsNullOrEmpty(mostPopularKeyword))
                {
                    ViewBag.MostPopularVariant = "No Most Popular Keyword";

                    return RedirectToAction("Index");
                }
                else
                {
                    if (arabiziWordGuid != Guid.Empty)
                    {
                        // save mostPopularKeyword
                        var path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                        new TextPersist().Serialize_Update_M_ARABICDARIJAENTRY_LATINWORD_MostPopularVariant(arabiziWordGuid, mostPopularKeyword, path);
                    }

                    //
                    ViewBag.MostPopularVariant = mostPopularKeyword;

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        public ActionResult Train_DeleteEntry(Guid arabiziWordGuid)
        {
            new Arabizer().Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL(arabiziWordGuid);

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_RefreshEntry(Guid arabiziWordGuid)
        {
            var arabizer = new Arabizer(Server);

            // get data existing arabizi
            var backupARABIZIENTR = loaddeserializeM_ARABIZIENTRY_DAPPERSQL(arabiziWordGuid);

            // get data existing theme
            var backupXTRCTTHEME = arabizer.loadDeserializeM_XTRCTTHEME_DAPPERSQL(backupARABIZIENTR.ID_XTRCTTHEME);

            //
            using (var db = new ArabiziDbContext())
            {
                // delete
                arabizer.Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL_uow(arabiziWordGuid, db, isEndOfScope: false);

                // recreate
                arabizer.train_uow(new M_ARABIZIENTRY
                {
                    ArabiziText = backupARABIZIENTR.ArabiziText.Trim(new char[] { ' ', '\t' }),
                    ArabiziEntryDate = backupARABIZIENTR.ArabiziEntryDate,
                    IsFR = backupARABIZIENTR.IsFR,
                    ID_XTRCTTHEME = backupARABIZIENTR.ID_XTRCTTHEME
                }, backupXTRCTTHEME.ThemeName, db, isEndOfScope: false);

                //
                db.SaveChanges();
            }

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_DeleteEntries(String arabiziWordGuids)
        {
            var larabiziWordGuids = arabiziWordGuids.Split(new char[] { ',' });

            foreach (var larabiziWordGuid in larabiziWordGuids)
            {
                // minor : trim leading '='
                Guid arabiziWordGuid = new Guid(larabiziWordGuid.TrimStart(new char[] { '=' }));

                new Arabizer().Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL(arabiziWordGuid);
            }

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_RefreshEntries(String arabiziWordGuids)
        {
            var larabiziWordGuids = arabiziWordGuids.Split(new char[] { ',' });

            var arabizer = new Arabizer(Server);

            // get data existing theme : all posts are under same theme, so use first
            // minor : trim leading '='
            var firstLArabiziWordGuid = larabiziWordGuids[0];
            Guid firstArabiziWordGuid = new Guid(firstLArabiziWordGuid.TrimStart(new char[] { '=' }));
            var backupFirstARABIZIENTR = loaddeserializeM_ARABIZIENTRY_DAPPERSQL(firstArabiziWordGuid);
            var backupXTRCTTHEME = arabizer.loadDeserializeM_XTRCTTHEME_DAPPERSQL(backupFirstARABIZIENTR.ID_XTRCTTHEME);

            //
            using (var db = new ArabiziDbContext())
            {
                foreach (var larabiziWordGuid in larabiziWordGuids)
                {
                    // minor : trim leading '='
                    Guid arabiziWordGuid = new Guid(larabiziWordGuid.TrimStart(new char[] { '=' }));

                    // get data existing arabizi
                    var backupARABIZIENTR = loaddeserializeM_ARABIZIENTRY_EFSQL_uow(arabiziWordGuid, db);

                    // get data existing theme
                    // var backupXTRCTTHEME = loadDeserializeM_XTRCTTHEME_DAPPERSQL(backupARABIZIENTR.ID_XTRCTTHEME);

                    // delete
                    arabizer.Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL_uow(arabiziWordGuid, db, isEndOfScope: false);

                    // recreate
                    arabizer.train_uow(new M_ARABIZIENTRY
                    {
                        ArabiziText = backupARABIZIENTR.ArabiziText.Trim(new char[] { ' ', '\t' }),
                        ArabiziEntryDate = backupARABIZIENTR.ArabiziEntryDate,
                        IsFR = backupARABIZIENTR.IsFR,
                        ID_XTRCTTHEME = backupARABIZIENTR.ID_XTRCTTHEME
                    }, backupXTRCTTHEME.ThemeName, db, isEndOfScope: false);
                }

                //
                db.SaveChanges();
            }

            //
            return RedirectToAction("Index");
        }

        // This action applies a new main tag/entity/theme/keyword to a post
        [HttpGet]
        public ActionResult Train_ApplyNewMainTag(Guid idArabicDarijaEntry, String mainEntity)
        {
            // Arg mainEntity is the active theme name
            var activeTheme = mainEntity;

            // MC111217 since we may have in the DB cases where there are more than one main entities for the same post (due to previous bug in code), we need to 
            // find them and to clean them (delete the extra main entities)
            var arabicDarijaEntryOtherMainEntities = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL().FindAll(m => m.ID_ARABICDARIJAENTRY == idArabicDarijaEntry && m.TextEntity.Mention != activeTheme && m.TextEntity.Type == "MAIN ENTITY");
            new Arabizer().serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(arabicDarijaEntryOtherMainEntities);

            // load M_ARABICDARIJAENTRY_TEXTENTITY
            List<M_ARABICDARIJAENTRY_TEXTENTITY> arabicDarijaEntryTextEntities = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();

            // Check before if this entry has already a main entity
            if (arabicDarijaEntryTextEntities.Find(m => m.ID_ARABICDARIJAENTRY == idArabicDarijaEntry && m.TextEntity.Mention == mainEntity && m.TextEntity.Type == "MAIN ENTITY") != null)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "'" + mainEntity + "' is already MAIN ENTITY for the post";
                return RedirectToAction("Index");
            }

            // apply new main tag
            var m_arabicdarijaentry_textentity = new M_ARABICDARIJAENTRY_TEXTENTITY
            {
                ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                ID_ARABICDARIJAENTRY = idArabicDarijaEntry,
                TextEntity = new TextEntity
                {
                    Count = 1,
                    Mention = mainEntity,
                    Type = "MAIN ENTITY"
                }
            };
            new Arabizer().saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(m_arabicdarijaentry_textentity);

            //
            TempData["showAlertSuccess"] = true;
            TempData["msgAlert"] = "'" + mainEntity + "' is now MAIN ENTITY for the post";
            return RedirectToAction("Index");
        }
        #endregion

        #region FRONT YARD ACTIONS TRAIN USER CONTRIB
        [Authorize]
        [HttpPost]
        public ActionResult SaveTranslationContributionAjax(String idarabicdarijaentry, String contrib)
        {
            try
            {
                bool status;
                String message = String.Empty;

                // check before
                if (String.IsNullOrWhiteSpace(idarabicdarijaentry) || new Guid(idarabicdarijaentry) == Guid.Empty)
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Entry Id is invalid"
                    }), "application/json");

                // check before
                if (String.IsNullOrWhiteSpace(contrib))
                    return Content(JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "Contribution is empty"
                    }), "application/json");

                // real work
                using (var db = new ArabiziDbContext())
                {
                    var arabicdarijaentry = db.M_ARABICDARIJAENTRYs.Where(m => m.ID_ARABICDARIJAENTRY == new Guid(idarabicdarijaentry)).SingleOrDefault();
                    arabicdarijaentry.ContribArabicDarijaText = contrib;
                    db.SaveChanges();
                }

                // result
                status = true;
                message = "Thank You ! Your contribution will be used to improve translation quality and may be shown to users anonymously";

                //
                var json = JsonConvert.SerializeObject(new
                {
                    status = status,
                    message = message
                });
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return Content(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                }), "application/json");
            }
        }
        #endregion

        #region FRONT YARD ACTIONS FB
        [HttpGet]
        public async Task<object> AddFBInfluencer(String url_name, String pro_or_anti)
        {
            //
            var userId = User.Identity.GetUserId();

            //
            String errMessage = string.Empty;
            bool status = false;
            String translatedString = String.Empty;
            String result = null;

            //
            M_XTRCTTHEME activeTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
            activeTheme = (activeTheme != null) ? activeTheme : new M_XTRCTTHEME();

            //
            Tuple<String, String> results = new Tuple<string, string>(String.Empty, String.Empty);

            //
            if (activeTheme.ID_XTRCTTHEME != null)
            {
                // Check/Clean before
                url_name = url_name.Trim();
                url_name = url_name.TrimEnd(new[] { '/' });
                if (url_name.StartsWith("https://www.facebook.com/"))
                    url_name = url_name.Substring("https://www.facebook.com/".Length);

                // real work
                var themeid = activeTheme.ID_XTRCTTHEME;
                var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "AccountPanel/AddFBInfluencer?url_name=" + url_name + "&pro_or_anti=" + pro_or_anti + "&id=1&themeid=" + themeid + "&CallFrom=AddFBInfluencer";
                // ex : url : http://localhost:8081/AccountPanel/AddFBInfluencer?url_name=telquelofficiel&pro_or_anti=Anti&id=1&themeid=fd6590b9-dbd1-4341-9329-4a9cae8047eb&CallFrom=AddFBInfluencer
                results = await HtmlHelpers.PostAPIRequest_message(url, String.Empty, type: "POST");
                result = results.Item1;
            }
            else
                result = "No theme id can get.";

            if (result.ToLower().Contains("true"))
            {
                status = true;
                translatedString = result;
            }
            else
            {
                status = false;
                // errMessage = result;
                // errMessage += " - " + results.Item2;
                if (results.Item2 == "NotFound")
                    errMessage = R.NotFound;
                else
                    errMessage = results.Item2;
            }

            //
            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedString,
                message = errMessage
            });
        }

        /// <summary>
        /// Method for retrieving the fb posts (and comments as well) from ScrappyWeb
        /// </summary>
        [HttpGet]
        public async Task<object> RetrieveFBPosts(string influencerurl_name)
        {
            String result = String.Empty;

            try
            {
                String errMessage = string.Empty;
                bool status = false;
                int retrievedPostsCount = -1;
                int retrievedCommentsCount = -1;

                var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "Data/FetchFBInfluencerPosts?CallFrom=" + influencerurl_name;
                // ex : url : http://localhost:8081/Data/FetchFBInfluencerPosts?...
                result = await HtmlHelpers.PostAPIRequest_result(url, "");

                // parse result
                JObject jObject = JObject.Parse(result);

                //
                if (result.ToLower().Contains("true"))
                {
                    status = true;

                    // parse for retrieved posts count
                    JValue jretrievedPostsCount = (JValue)jObject["retrievedPostsCount"];
                    retrievedPostsCount = Convert.ToInt32(jretrievedPostsCount);

                    // parse for retrieved posts count
                    JValue jretrievedCommentsCount = (JValue)jObject["retrievedCommentsCount"];
                    retrievedCommentsCount = Convert.ToInt32(jretrievedCommentsCount);
                }
                else
                {
                    // parse for error message
                    JValue jmessage = (JValue)jObject["message"];
                    errMessage = Convert.ToString(jmessage);

                    // log
                    Logging.Write(Server, errMessage);
                }

                return JsonConvert.SerializeObject(new
                {
                    status = status,
                    retrievedPostsCount = retrievedPostsCount,
                    retrievedCommentsCount = retrievedCommentsCount,
                    message = errMessage
                });
            }
            catch (JsonReaderException ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, result);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        // Method for translate the fb posts
        [HttpGet]
        public async Task<object> TranslateFbPost(String content)
        {
            // get current active theme for the current user
            var userId = User.Identity.GetUserId();
            var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);

            //
            string errMessage = string.Empty;
            bool status = false;
            String translatedstring = string.Empty;

            // MC081217 translate via train to populate NER, analysis data, ...
            // Arabizi to arabic script via direct call to perl script
            var res = new Arabizer().train(new M_ARABIZIENTRY
            {
                ArabiziText = content.Trim(),
                ArabiziEntryDate = DateTime.Now,
                ID_XTRCTTHEME = userActiveXtrctTheme.ID_XTRCTTHEME
            }, userActiveXtrctTheme.ThemeName, thisLock: thisLock);

            if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY != Guid.Empty)
            {
                status = true;
                translatedstring = res.M_ARABICDARIJAENTRY.ArabicDarijaText;
            }
            else
                errMessage = "Text is required.";

            //
            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedstring,
                message = errMessage
            });
        }

        [HttpGet]
        public async Task<object> TranslateFbComments(string ids)
        {
            try
            {
                // get current active theme for the current user
                var userId = User.Identity.GetUserId();
                var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);

                //
                string errMessage = string.Empty;
                bool status = false;
                string translatedstring = "";
                if (!string.IsNullOrEmpty(ids))
                {
                    // get list of not yet translated comments with the specified ids (can be one comment to translate or can be many checked)
                    List<FBFeedComment> allComments = new Arabizer().loaddeserializeT_FB_Comments_By_Ids_DAPPERSQL(ids);

                    //
                    if (allComments != null && allComments.Count > 0)
                    {
                        foreach (var item in allComments)
                        {
                            // in FB, some posts text can be empty (image, ...)
                            if (String.IsNullOrWhiteSpace(item.message))
                                continue;

                            // MC081217 translate via train to populate NER, analysis data, ...
                            // Arabizi to arabic script via direct call to perl script
                            var res = new Arabizer().train(new M_ARABIZIENTRY
                            {
                                ArabiziText = item.message.Trim(),
                                ArabiziEntryDate = DateTime.Now,
                                ID_XTRCTTHEME = userActiveXtrctTheme.ID_XTRCTTHEME
                            }, userActiveXtrctTheme.ThemeName, thisLock: thisLock);

                            if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY != Guid.Empty)
                            {
                                status = true;
                                translatedstring = res.M_ARABICDARIJAENTRY.ArabicDarijaText;
                                new Arabizer().SaveTranslatedComments(item.Id, translatedstring);
                            }
                            else
                                errMessage = "Text is required.";
                        }
                    }
                    else
                        errMessage = "All selected comments are already translated.";
                }

                //
                return JsonConvert.SerializeObject(new
                {
                    status = status,
                    // recordsFiltered = translatedstring,
                    message = errMessage
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        [HttpGet]
        public async Task<object> Train_FB_Posts_woBingGoogleRosette(string postsIds)
        {
            // Check before
            if (string.IsNullOrEmpty(postsIds))
                return JsonConvert.SerializeObject(new { status = false, message = "Error. Please check your parameters" });

            try
            {
                // get list of not yet translated comments with the specified ids (can be one comment to translate or can be many checked)
                List<FB_POST> posts = new Arabizer().loaddeserializeT_FB_POSTs_By_Ids_DAPPERSQL(postsIds);

                // btw, refresh post (nbr of likes, nbr of comments, ...)
                // TODO : do it for all posts not only first
                var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "Data/FetchPost?postsId=" + posts[0].id;
                await HtmlHelpers.PostAPIRequest_result(url, "");

                //
                foreach (var post in posts)
                {
                    // in FB, some posts text can be empty (image, ...)
                    if (String.IsNullOrWhiteSpace(post.post_text))
                        continue;

                    // btw, refresh comments for the post
                    // TODO post.id

                    // MC081217 translate via train to populate NER, analysis data, ...
                    // Arabizi to arabic script via direct call to perl script
                    var res = new Arabizer().train_woBingGoogleRosette(post.post_text.Trim(), post.id, (int)EntryType.post);
                    if (res.status == false)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            status = false,
                            message = "Error during the extraction."
                        });
                    }

                    // save translated posts
                    string translatedstring = res.ArabicDarijaText;
                    new Arabizer().SaveTranslatedPosts(post.id, translatedstring);
                }

                //
                return JsonConvert.SerializeObject(new
                {
                    status = true,
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<object> Train_FB_Comments_woBingGoogleRosette(string commentsIds)
        {
            // Check before
            if (string.IsNullOrEmpty(commentsIds))
            {
                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = "Error. Please check your parameters"
                });
            }

            try
            {
                // get list of not yet translated comments with the specified ids (can be one comment to translate or can be many checked)
                List<FBFeedComment> comments = new Arabizer().loaddeserializeT_FB_Comments_By_Ids_DAPPERSQL(commentsIds);

                // check before
                if (comments == null || comments.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        status = false,
                        message = "All selected comments are already translated."
                    });
                }

                //
                foreach (var comment in comments)
                {
                    // in FB, some posts text can be empty (image, ...)
                    if (String.IsNullOrWhiteSpace(comment.message))
                        continue;

                    // MC081217 translate via train to populate NER, analysis data, ...
                    // Arabizi to arabic script via direct call to perl script
                    var res = new Arabizer().train_woBingGoogleRosette(comment.message.Trim(), comment.Id, (int)EntryType.comment);
                    if (res.status == false)
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            status = false,
                            message = "Error during the extraction."
                        });
                    }

                    //
                    string translatedstring = res.ArabicDarijaText;
                    new Arabizer().SaveTranslatedComments(comment.Id, translatedstring);
                }

                //
                return JsonConvert.SerializeObject(new
                {
                    status = true,
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // Translate and extract Ner from FB Posts and Comments on a time interval.
        [HttpGet]
        public object TranslateAndExtractNERFBPostsAndComments(string influencerid)
        {
            // get current theme id
            var userId = User.Identity.GetUserId();
            var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
            var themeId = userActiveXtrctTheme.ID_XTRCTTHEME;

            bool status = false;
            string errMessage = string.Empty;
            string urlForMail = ""; // string for send the url with the negative word for send email.

            // Check before
            if (string.IsNullOrEmpty(influencerid))
            {
                return JsonConvert.SerializeObject(new
                {
                    status = status,
                    message = "Param influencer id empty",
                    fct = "TranslateAndExtractNERFBPostsAndComments"
                });
            }

            try
            {
                //
                T_FB_INFLUENCER influencer = new Arabizer().loadDeserializeT_FB_INFLUENCER_DAPPERSQL(influencerid, themeId);
                string targetEntities = influencer.TargetEntities;
                String urlName = influencer.url_name;
                string pageName = influencer.name;

                //
                List<FB_POST> fbPosts = new Arabizer().loaddeserializeT_FB_POST_DAPPERSQL(influencerid).ToList();
                if (fbPosts != null && fbPosts.Count() > 0)
                {
                    fbPosts = fbPosts.OrderByDescending(c => c.date_publishing).Take(100).ToList();
                    foreach (var fbPostForTranslate in fbPosts)
                    {
                        bool isNegative = false;
                        bool isNegativeComments = false;

                        // for each post, translate and detect if there is negativity
                        if (!string.IsNullOrEmpty(fbPostForTranslate.post_text) && string.IsNullOrEmpty(fbPostForTranslate.translated_text))
                            TranslatePostAndCatchExpletive(fbPostForTranslate, targetEntities, out status, ref urlForMail, out isNegative);

                        // for each comment, translate and detect if there is negativity
                        if (!string.IsNullOrEmpty(fbPostForTranslate.id))
                        {
                            string postid = fbPostForTranslate.id.Split('_')[1];
                            var fbComments = new Arabizer().loaddeserializeT_FB_Comments_DAPPERSQL(postid);
                            var fbCommentsForTranslate = fbComments.Where(c => c.message != null && c.translated_message == null).OrderByDescending(c => c.created_time).Take(100).ToList();
                            foreach (var fbCommentForTranslate in fbCommentsForTranslate)
                                TranslateCommentAndCatchExpletive(fbCommentForTranslate, targetEntities, out status, ref urlForMail, out isNegativeComments);
                        }

                        //
                        if (isNegative == true || isNegativeComments == true)
                        {
                            string postURL = "<a href='www.facebook.com/" + urlName + "/posts/" + fbPostForTranslate.id.Split('_')[1] + "'>www.facebook.com/" + urlName + "/posts/" + fbPostForTranslate.id.Split('_')[1] + "</a>";
                            string body = "Hello user,<br/>Some bad words has been detected on your facebook page " + pageName + ".<br/>" + postURL + "<br/><table>" + urlForMail + "</table></br>Please remove the words from the page.<br/>Thanks.";
                            if (string.IsNullOrEmpty(fbPostForTranslate.MailBody))
                                updateFb_PostMailBody(fbPostForTranslate.id, mailBody: body);
                        }
                    }
                }

                // sending mail about negative NERs
                SendingMailAboutNegativeNERs(influencerid, ref status, ref errMessage);
            }
            catch (Exception e)
            {
                status = false;
                errMessage = e.Message;
            }

            return JsonConvert.SerializeObject(new
            {
                status = status,
                message = errMessage,
                fct = "TranslateAndExtractNERFBPostsAndComments"
            });
        }

        // Method for Add target entities on the influencer table.
        [HttpGet]
        public async Task<object> AddTextEntity(string influencerid, string targetText, bool? isAutoRetrieveFBPostandComments)
        {
            string errMessage = string.Empty;
            bool status = false;

            try
            {
                // get current theme id
                var userId = User.Identity.GetUserId();
                var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
                var themeId = userActiveXtrctTheme.ID_XTRCTTHEME;

                // Get the influencer if it exists
                var influencer = new Arabizer().loadDeserializeT_FB_INFLUENCER_DAPPERSQL(influencerid, themeId);
                if (influencer != null && influencer.id != null)
                {
                    // Update influencer target entities value.
                    if (!string.IsNullOrEmpty(targetText))
                    {
                        updateT_FB_INFLUENCERAsId(influencerid, influencer.fk_theme, targetText, isAutoRetrieveFBPostandComments);
                        status = true;
                        errMessage = "Target text added successfully.";
                    }
                }

                //
                return JsonConvert.SerializeObject(new
                {
                    status = status,
                    message = errMessage
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }
        #endregion

        #region BACK YARD ACTIONS FB
        private void SendingMailAboutNegativeNERs(String influencerid, ref bool status, ref String errMessage)
        {
            // sending mail about negative NERs
            List<FB_POST> sendMailPosts = new Arabizer().loaddeserializeT_FB_POST_DAPPERSQL(influencerid, isForSendMail: true);
            if (sendMailPosts != null && sendMailPosts.Count > 0)
            {
                var userid = User.Identity.GetUserId();
                var db1 = new ApplicationDbContext();
                var user = db1.Users.FirstOrDefault(c => c.Id == userid);
                if (user != null)
                {
                    var email = user.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        foreach (var posts in sendMailPosts)
                        {
                            if (!string.IsNullOrEmpty(posts.MailBody))
                            {
                                string subject = "Gravitas Warning! Some bad words has been detected in your page.";
                                string body = posts.MailBody;
                                int n = 0;
                                if (posts.NoOfTimeMailSend < 2 || posts.NoOfTimeMailSend == null)
                                {
                                    if (posts.NoOfTimeMailSend == null || posts.NoOfTimeMailSend == 0)  // send mail on first time to the user if there is any negative/expletive words on posts and comments.
                                    {
                                        status = SendEmailImplicitSsl(email, subject, body);
                                        n = 1;
                                    }
                                    else if (posts.LastMailSendOn <= DateTime.Now.AddHours(-6)) // send mail on second time in a day after 6 hours of first mail.
                                    {
                                        status = SendEmailImplicitSsl(email, subject, body);
                                        n = 2;
                                    }
                                    if (status == true)
                                        updateFb_PostMailBody(posts.id, noOfTimeMailSend: n);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Method for translate (using train) a Fb post or a FB comment (and save correspounding translated text column) in a generic way.
        private ReturnData TranslateFBPostOrFBComment(String content, string postId = "", string commentId = "")
        {
            var userId = User.Identity.GetUserId();
            string errMessage = string.Empty;
            bool status = false;
            string translatedstring = "";
            var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();

            // MC081217 translate via train to populate NER, analysis data, ...
            // Arabizi to arabic script via direct call to perl script
            var res = new Arabizer().train(new M_ARABIZIENTRY
            {
                ArabiziText = content.Trim(),
                ArabiziEntryDate = DateTime.Now,
                ID_XTRCTTHEME = userActiveXtrctTheme.ID_XTRCTTHEME
            }, userActiveXtrctTheme.ThemeName, thisLock: thisLock);

            // and save correspounding translated text column, either FB post or FB comment
            if (res.M_ARABICDARIJAENTRY.ID_ARABICDARIJAENTRY != Guid.Empty)
            {
                status = true;
                translatedstring = res.M_ARABICDARIJAENTRY.ArabicDarijaText;
                if (!string.IsNullOrEmpty(postId))
                    SaveTranslatedPost(postId, translatedstring);
                else if (!string.IsNullOrEmpty(commentId))
                    new Arabizer().SaveTranslatedComments(commentId, translatedstring);
            }
            else
                errMessage = "Text is required.";

            // return extracted entities for next step (filter against negative/expletive)
            textEntities = res.M_ARABICDARIJAENTRY_TEXTENTITYs;

            //
            ReturnData returndata = new ReturnData();
            returndata.TextEntities = textEntities;
            returndata.Status = status;
            returndata.Message = errMessage;
            returndata.TranslatedText = translatedstring;

            return returndata;
        }

        private void TranslatePostAndCatchExpletive(FB_POST fbPostForTranslate, String targetWatchEntitiesStr, out bool status, ref string urlForMail, out bool isNegative)
        {
            //
            status = false;
            isNegative = false;

            // check before
            if (string.IsNullOrEmpty(targetWatchEntitiesStr))
                return;

            // translate and save translation to db
            var returndata = TranslateFBPostOrFBComment(content: fbPostForTranslate.post_text, postId: fbPostForTranslate.id);
            if (returndata.Status != true)
                return;

            //
            var extractedNERs = returndata.TextEntities.Select(m => new { Mention = m.TextEntity.Mention, Type = m.TextEntity.Type });
            var extractedNegExplNERs = extractedNERs.Where(c => c.Type == "NEGATIVE" || c.Type == "EXPLETIVE" || c.Type == "SENSITIVE" || c.Type == "OPPOSE");
            var targetWatchEntityes = targetWatchEntitiesStr.Split(',').ToList();
            var originalText = fbPostForTranslate.post_text;
            var translatedText = returndata.TranslatedText;

            // process if any expletive NER
            if (extractedNegExplNERs.Count() > 0)
            {
                foreach (var targetWatchEntity in targetWatchEntityes)
                {
                    if (originalText.ToUpper().Contains(targetWatchEntity.ToUpper()) || translatedText.ToUpper().Contains(targetWatchEntity.ToUpper()))
                    {
                        var strToFind = targetWatchEntity; // + " " + item.TextEntity.Mention;
                        isNegative = true;
                        urlForMail = urlForMail + "<tr>" + strToFind + "</tr>";

                        // one negative/expletive match is enough
                        break;
                    }
                }
            }

            status = true;
        }

        private void TranslateCommentAndCatchExpletive(FBFeedComment fbCommentForTranslate, String targetWatchEntitiesStr, out bool status, ref string urlForMail, out bool isNegative)
        {
            //
            status = false;
            isNegative = false;

            // check before
            if (string.IsNullOrEmpty(targetWatchEntitiesStr))
                return;

            // translate and save translation to db
            var returndata = TranslateFBPostOrFBComment(content: fbCommentForTranslate.message, commentId: fbCommentForTranslate.Id);
            if (returndata.Status != true)
                return;

            //
            var extractedNERs = returndata.TextEntities.Select(m => new { Mention = m.TextEntity.Mention, Type = m.TextEntity.Type });
            var extractedNegExplNERs = extractedNERs.Where(c => c.Type == "NEGATIVE" || c.Type == "EXPLETIVE" || c.Type == "EXPLETIVE" || c.Type == "OPPOSE");
            var targetWatchEntityes = targetWatchEntitiesStr.Split(',').ToList();
            var originalText = fbCommentForTranslate.message;
            var translatedText = returndata.TranslatedText;

            // process if any expletive NER
            if (extractedNegExplNERs.Count() > 0)
            {
                foreach (var targetWatchEntity in targetWatchEntityes)
                {
                    if (originalText.ToUpper().Contains(targetWatchEntity.ToUpper()) || translatedText.ToUpper().Contains(targetWatchEntity.ToUpper()))
                    {
                        var strToFind = targetWatchEntity; // + " " + item.TextEntity.Mention;
                        isNegative = true;
                        urlForMail = urlForMail + "<tr>" + strToFind + "</tr>";

                        // one negative/expletive match is enough
                        break;
                    }
                }
            }

            status = true;
        }

        public static bool SendEmailImplicitSsl(string toEmailAddress, string subject, string body)
        {
            bool status = false;

            var emailsetting = UtilityFunctions.GetEmailSetting();
            var username = emailsetting.SMTPServerLoginName;
            string password = emailsetting.SMTPServerPassword;

            var msg = new MimeMessage();
            msg.Subject = subject;

            msg.From.Add(new MailboxAddress("Gravitas Support", emailsetting.NoReplyEmailAddress));

            foreach (var email in toEmailAddress.Split(','))
            {
                msg.To.Add(new MailboxAddress("user", email));
            }
            /*var bccAddress = ConfigurationManager.AppSettings["BCCEmailAddress"];
            if (!string.IsNullOrEmpty(bccAddress))
            {
                msg.Bcc.Add(new MailAddress(bccAddress));
            }*/
            msg.Bcc.Add(new MailboxAddress("oa", "alaoui.o@gmail.com"));
            msg.Bcc.Add(new MailboxAddress("dv", "dv@mughamrat.com"));

            msg.Body = new TextPart("html") { Text = body };

            using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
            {
                //
                smtpClient.Connect(emailsetting.SMTPServerUrl, emailsetting.SMTPServerPort);

                // Note: only needed if the SMTP server requires authentication
                smtpClient.Authenticate(username, password);

                //
                smtpClient.Send(msg);
                smtpClient.Disconnect(true);
            }

            status = true;

            return status;
        }
        #endregion

        #region FRONT YARD ACTIONS TWINGLY
        [HttpGet]
        public ActionResult TwinglySetup()
        {
            // use one time (from browser) to fill twingly accounts data - delete later - should have a UI to add new twinlgy accounts

            //
            var twinglyAccounts = new List<M_TWINGLYACCOUNT>();
            var contactATallpecmediaDOTcom = new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = new Guid("E8E23C4E-F19E-4675-9FCB-D69B16ECB7AE"),
                UserName = "contact@allpecmedia.com",
                calls_free = 1000
            };

            //
            var alaouiDOToATgmailDOTcom = new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = new Guid("0910880B-2D6A-47C2-8E5A-AF5365D062CE"),
                UserName = "alaoui.o@gmail.com",
                calls_free = 1000,
                CurrentActive = "active"
            };

            // Save to Serialization
            String path = Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt");
            new TextPersist().Serialize<M_TWINGLYACCOUNT>(contactATallpecmediaDOTcom, path);
            new TextPersist().Serialize<M_TWINGLYACCOUNT>(alaouiDOToATgmailDOTcom, path);

            //
            // return View("Index");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_changeActiveAccount(Guid newlyActive_id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // disable active one
            twinglyAccounts.Find(m => m.CurrentActive == "active").CurrentActive = String.Empty;

            // active the selected one
            twinglyAccounts.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == newlyActive_id_twinglyaccount_api_key).CurrentActive = "active";

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_AddNewActiveAccount(Guid newlyActive_id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // disable active one
            twinglyAccounts.Find(m => m.CurrentActive == "active").CurrentActive = String.Empty;

            //
            twinglyAccounts.Add(new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = newlyActive_id_twinglyaccount_api_key,
                UserName = "",
                calls_free = 1000,
                CurrentActive = "active"
            });

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_deleteAccount(Guid id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // find the one to delete
            var accountToDelete = twinglyAccounts.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == id_twinglyaccount_api_key);

            // disable active one
            var activeAccount = twinglyAccounts.Find(m => m.CurrentActive == "active");

            // we can not delete active account
            if (accountToDelete == activeAccount)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "you cannot delete the active Twingly account";
                return RedirectToAction("Index");
            }

            // remove
            twinglyAccounts.Remove(accountToDelete);

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }
        #endregion

        #region FRONT YARD ACTIONS THEME
        [HttpPost]
        public ActionResult XtrctTheme_AddNew(String themename, String themetags)
        {
            //
            var userId = User.Identity.GetUserId();

            // create the theme
            var newXtrctTheme = new M_XTRCTTHEME
            {
                ID_XTRCTTHEME = Guid.NewGuid(),
                ThemeName = themename.Trim(),
                UserID = userId
            };

            // Save to Serialization
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL(newXtrctTheme);

            // create the associated tags
            if (themetags != null)
            {
                foreach (var themetag in themetags.Split(new char[] { ',' }))
                {
                    var newXrtctThemeKeyword = new M_XTRCTTHEME_KEYWORD
                    {
                        ID_XTRCTTHEME_KEYWORD = Guid.NewGuid(),
                        ID_XTRCTTHEME = newXtrctTheme.ID_XTRCTTHEME,
                        Keyword = themetag
                    };

                    // Save to Serialization to DB
                    new Arabizer().saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(newXrtctThemeKeyword);
                }
            }

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult XtrctTheme_AddNewAjax(String themename)
        {
            //
            var userId = User.Identity.GetUserId();

            // check before create theme
            var themeWithSameName = new Arabizer().loadDeserializeM_XTRCTTHEME_DAPPERSQL(userId, themename.Trim());
            if (themeWithSameName != null)
                return Json(new
                {
                    success = false,
                    responseText = "Theme name already exists in database !!!!"
                }, JsonRequestBehavior.AllowGet);

            // create the theme
            var newXtrctTheme = new M_XTRCTTHEME
            {
                ID_XTRCTTHEME = Guid.NewGuid(),
                ThemeName = themename.Trim(),
                UserID = userId
            };

            // Save to Serialization
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL(newXtrctTheme);

            // return success
            return Json(new
            {
                success = true,
                responseText = themename.Trim()
            }, JsonRequestBehavior.AllowGet);
        }

        // [HttpPost]
        [HttpGet]
        public ActionResult XtrctTheme_EditName(Guid idXtrctTheme, String themeNewName)
        {
            //
            var userId = User.Identity.GetUserId();

            // check before rename theme
            var themeWithSameName = new Arabizer().loadDeserializeM_XTRCTTHEME_DAPPERSQL(userId, themeNewName.Trim());
            if (themeWithSameName != null)
                return Json(new
                {
                    success = false,
                    responseText = "Theme name already exists in database !!!!"
                }, JsonRequestBehavior.AllowGet);

            // Save to Serialization
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL_Rename(idXtrctTheme, themeNewName);

            //
            // return RedirectToAction("Index");
            // return success
            return Json(new
            {
                success = true,
                // responseText = themename.Trim()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult XtrctTheme_Delete(Guid idXtrctTheme)
        {
            //
            var result = new Arabizer().Serialize_Delete_M_XTRCTTHEME_Cascading_EFSQL(idXtrctTheme);

            //
            if (result.Result == false)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = result.ErrMessage;
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult XtrctTheme_ApplyNewActive(String themename)
        {
            //
            var userId = User.Identity.GetUserId();
            ApplyNewActiveTheme(themename, userId);

            //
            return RedirectToAction("Index");
        }

        private static void ApplyNewActiveTheme(string themename, string userId)
        {
            // find previous active, and disable it
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL_Deactivate(userId);

            // find to-be-active by name, and make it active
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL_Active(themename, userId);
        }

        private static void ApplyNewActiveTheme(Guid idXtrctTheme, string userId)
        {
            // find previous active, and disable it
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL_Deactivate(userId);

            // find to-be-active by name, and make it active
            new Arabizer().saveserializeM_XTRCTTHEME_EFSQL_Active(idXtrctTheme);
        }

        [HttpGet]
        public ActionResult XtrctTheme_Keywords_Reload(String themename)
        {
            //
            var userId = User.Identity.GetUserId();

            //
            List<THEMETAGSCOUNT> tagscounts = new Arabizer().loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_THEMETAGSCOUNT_DAPPERSQL(themename, userId);

            //
            var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);

            //
            List<M_XTRCTTHEME_KEYWORD> userActiveXtrctThemeKeywords = new List<M_XTRCTTHEME_KEYWORD>();
            foreach (var tagcount in tagscounts)
            {
                userActiveXtrctThemeKeywords.Add(new M_XTRCTTHEME_KEYWORD
                {
                    ID_XTRCTTHEME_KEYWORD = Guid.NewGuid(),
                    ID_XTRCTTHEME = userActiveXtrctTheme.ID_XTRCTTHEME,
                    Keyword = tagcount.TextEntity_Mention,
                    Keyword_Type = tagcount.TextEntity_Type,
                    Keyword_Count = tagcount.SUM_TextEntity_Count
                });
            }

            // save
            new Arabizer().saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(userActiveXtrctThemeKeywords, userActiveXtrctTheme);

            //
            return RedirectToAction("Index");
        }
        #endregion

        //
        private static IDictionary<Guid, int> indexForUploadInProgress = new Dictionary<Guid, int>();

        // This action handles the form POST and the upload
        [HttpPost]
        public ActionResult Data_Upload(HttpPostedFileBase file, String mainEntity)
        {
            try
            {
                // get current active theme for the current user
                var userId = User.Identity.GetUserId();
                var userActiveXtrctTheme = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);

                Logging.Write(Server, "Data_Upload - 1");

                // check before if the user selected a file
                if (file == null || file.ContentLength == 0)
                {
                    // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = "No file has been chosen.";
                    return RedirectToAction("Index");
                }

                Logging.Write(Server, "Data_Upload - 2");

                // check before if mainEntity is not empty
                if (String.IsNullOrWhiteSpace(mainEntity))
                {
                    // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = "No main entity has been entered.";
                    return RedirectToAction("Index");
                }

                // check extension
                var allowedExtensions = new[] { ".tsv", ".txt", ".csv" };
                var checkextension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(checkextension))
                {
                    // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = "Select tsv or txt or csv file with one column.";
                    return RedirectToAction("Index");
                }

                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);

                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);

                var token = Session["_T0k@n_"];
                string errMessage = string.Empty;
                if (token != null && _IAuthenticate.IsTokenValid(Convert.ToString(token), "Data_Upload", out errMessage))
                {
                    //
                    var lines = System.IO.File.ReadLines(path).ToList();
                    if (lines.Count > 20)
                    {
                        // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                        TempData["showAlertWarning"] = true;
                        TempData["msgAlert"] = "Select tsv or txt or csv file with one column with 20 lines max";
                        return RedirectToAction("Index");
                    }

                    // the id to track the progress of upload
                    var uploadInProgressId = new Guid(userId);
                    indexForUploadInProgress.Add(uploadInProgressId, 0);

                    // loop and process each one
                    int i = 0;
                    foreach (string line in lines)
                    {
                        if (String.IsNullOrWhiteSpace(line))
                            continue;

                        // translate
                        new Arabizer().train(new M_ARABIZIENTRY
                        {
                            ArabiziText = line.Trim(),
                            ArabiziEntryDate = DateTime.Now,
                            ID_XTRCTTHEME = userActiveXtrctTheme.ID_XTRCTTHEME
                        }, mainEntity, thisLock: thisLock);

                        // progress
                        i++;
                        indexForUploadInProgress[uploadInProgressId] = i; // lines.IndexOf(line);
                    }

                    // mark how many rows been translated
                    TempData["showAlertSuccess"] = true;
                    // TempData["msgAlert"] = lines.Count.ToString() + " rows has been imported.";
                    TempData["msgAlert"] = i + " rows has been imported.";

                    // end progress
                    indexForUploadInProgress[uploadInProgressId] = -1;
                    indexForUploadInProgress.Remove(uploadInProgressId);
                }
                else
                {
                    //TMP Session["_T0k@n_"] = "";
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = errMessage;  // "Not a valid token";
                }

                // delete just uploaded file in uploads
                System.IO.File.Delete(path);

                // redirect back to the index action to show the form once again
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                throw;
            }
        }

        /*[HttpPost]
        public ActionResult Data_Upload_Progress()
        {
            // get current active theme for the current user
            var userId = User.Identity.GetUserId();
            var userGuid = new Guid(userId);

            //
            int progress = 0;
            if (indexForUploadInProgress.Keys.Contains(userGuid))
                progress = indexForUploadInProgress[userGuid];

            //
            return Json(progress);
        }*/

        [AllowAnonymous]
        [HttpPost]
        public void Log(String message)
        {
            /*lock (thisLock)
            {*/
            Logging.Write(Server, message);
            // }
        }

        [HttpPost]
        public object DataTablesNet_ServerSide_GetList(bool adminModeShowAll = false)
        {
            try
            {
                int start = 0;
                int itemsPerPage = 10;

                //
                var userId = User.Identity.GetUserId();

                // from client side
                int.TryParse(this.Request.Form["start"], out start);            // POST : from where we start the paging
                int.TryParse(this.Request.Form["length"], out itemsPerPage);    // POST : to which length the paging goes
                string searchValue = this.Request.Form["search[value]"];        // POST : search word
                var min = this.Request.Form["min"];  // date range
                var max = this.Request.Form["max"];  // date range

                // get from client search word : trim extra tabs, spaces, ...
                if (String.IsNullOrEmpty(searchValue) == false) searchValue = searchValue.Trim(new char[] { ' ', '\'', '\t' });

                // POST
                /*string searchAccount = this.Request.Form["columns[0][search][value]"];
                string searchSource = this.Request.Form["columns[1][search][value]"];
                string searchEntity = this.Request.Form["columns[2][search][value]"];
                string searchName = this.Request.Form["columns[3][search][value]"];*/

                // get main (whole) data from DB first
                List<ArabiziToArabicViewModel> items = loadArabiziToArabicViewModel_DAPPERSQL(activeThemeOnly: true, userId: userId, adminModeShowAll: adminModeShowAll);

                // get the number of entries
                var itemsCount = items.Count;

                // adjust itemsPerPage case show all
                if (itemsPerPage == -1)
                    itemsPerPage = itemsCount;

                // filter on search term if any
                if (!String.IsNullOrEmpty(searchValue))
                    items = items.Where(a => a.ArabiziText.ToUpper().Contains(searchValue.ToUpper()) || a.ArabicDarijaText.ToUpper().Contains(searchValue.ToUpper())).ToList();

                // filter on date range
                if (min != null)
                {
                    var minDate = Convert.ToDateTime(min);
                    items = items.Where(a => a.ArabiziEntryDate >= minDate).ToList();
                }
                if (max != null)
                {
                    var maxDate = Convert.ToDateTime(max).AddDays(1);
                    items = items.Where(a => a.ArabiziEntryDate <= maxDate).ToList();
                }

                //
                var itemsFilteredCount = items.Count;

                // page as per request (index of page and length)
                items = items.Skip(start).Take(itemsPerPage).ToList();

                // get other helper data from DB
                List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();
                List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();
                List<M_XTRCTTHEME> mainEntities = new Arabizer().loaddeserializeM_XTRCTTHEME_DAPPERSQL(userId);

                // excludes POS NER (PRONOMS, PREPOSITIONS, ...), plus also MAIN
                textEntities.RemoveAll(m => m.TextEntity.Type == "PREPOSITION"
                                    || m.TextEntity.Type == "PRONOUN"
                                    || m.TextEntity.Type == "ADVERB"
                                    || m.TextEntity.Type == "CONJUNCTION"
                                    || m.TextEntity.Type == "MAIN ENTITY");

                // Visual formatting before sending back
                items.ForEach(s =>
                {
                    s.PositionHash = s.Rank;
                    s.FormattedArabiziEntryDate = s.ArabiziEntryDate.ToString("yy-MM-dd HH:mm");
                    s.FormattedArabicDarijaText = TextTools.HighlightExtractedLatinWords(s.ArabicDarijaText, s.ID_ARABICDARIJAENTRY, arabicDarijaEntryLatinWords);
                    s.FormattedEntitiesTypes = TextTools.DisplayEntitiesType(s.ID_ARABICDARIJAENTRY, textEntities);
                    s.FormattedEntities = TextTools.DisplayEntities(s.ID_ARABICDARIJAENTRY, textEntities);
                    s.FormattedRemoveAndApplyTagCol = TextTools.DisplayRemoveAndApplyTagCol(s.ID_ARABIZIENTRY, s.ID_ARABICDARIJAENTRY, mainEntities);
                });

                // final filtering (ners, ...) : get ners
                var nersjson = this.Request.Form["ners"];
                if (!String.IsNullOrWhiteSpace(nersjson))
                {
                    JArray a = JArray.Parse(nersjson);
                    foreach (var item in a)
                    {
                        items = items.Where(c => c.FormattedEntities != null && c.FormattedEntities.Contains((String)item)).ToList();
                    }
                }

                //
                return JsonConvert.SerializeObject(new
                {
                    recordsTotal = itemsCount.ToString(),
                    recordsFiltered = itemsFilteredCount.ToString(),
                    data = items
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        [HttpGet]
        public object DataTablesNet_ServerSide_FB_Posts_GetList(string fluencerid)
        {
            int start = 0;
            int itemsPerPage = 10;

            // get from client side
            int.TryParse(this.Request.QueryString["start"], out start);         // GET : from where we start the paging
            int.TryParse(this.Request.QueryString["length"], out itemsPerPage); // GET : to which length the paging goes
            string searchValue = this.Request.QueryString["search[value]"];     // GET : search word

            // get from client search word : trim extra tabs, spaces, ...
            if (String.IsNullOrEmpty(searchValue) == false) searchValue = searchValue.Trim(new char[] { ' ', '\'', '\t' });

            // get the number of all entries (before applyng any filter)
            var totalItemsCount = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL(fluencerid);

            // get main (whole) data from DB first while filtering on search term if any
            var items = new Arabizer().loaddeserializeT_FB_POST_JOIN_COMMENT_Like_Filter_DAPPERSQL(fluencerid, searchValue).Select(c => new
            {
                id = c.id,
                // fk_i = c.fk_influencer,
                pt = c.post_text,
                tt = c.translated_text,
                lc = c.likes_count,
                cc = c.comments_count,
                dp = c.date_publishing.ToString("yy-MM-dd HH:mm")
            }).ToList();

            // adjust itemsPerPage case show all
            if (itemsPerPage == -1)
                itemsPerPage = totalItemsCount;

            //
            var itemsFilteredCount = items.Count;

            // page as per request (index of page and length)
            items = items.Skip(start).Take(itemsPerPage).ToList();

            // if only one found, return and uncollapse it out
            /*String extraData = null;
            if (itemsFilteredCount == 1)
                extraData = items[0].id;*/

            //
            return JsonConvert.SerializeObject(new
            {
                recordsTotal = totalItemsCount.ToString(),
                recordsFiltered = itemsFilteredCount.ToString(),
                data = items,
                // extraData
            });
        }

        [HttpGet]
        public object DataTablesNet_ServerSide_SocialSearch_GetList()
        {
            int start = 0;
            int itemsPerPage = 10;

            // from client sides
            int.TryParse(this.Request.QueryString["start"], out start);                 // GET : from where we start the paging
            int.TryParse(this.Request.QueryString["length"], out itemsPerPage);         // GET : to which length the paging goes
            string searchValue = this.Request.QueryString["search[value]"];             // GET : search word
            var wholeWord = Convert.ToBoolean(this.Request.QueryString["wholeWord"]);   // whole word
            var min = this.Request.QueryString["min"];  // date range
            DateTime? minDate = min == null ? (DateTime?)null : Convert.ToDateTime(min);
            var max = this.Request.QueryString["max"];  // date range
            DateTime? maxDate = max == null ? (DateTime?)null : Convert.ToDateTime(max).AddDays(1);
            var nersjson = this.Request.QueryString["ners"];    // final filtering (ners, ...) : get ners
            var nersa = nersjson == null ? null : JArray.Parse(nersjson).ToObject<String[]>();
            var fbpgsjson = this.Request.QueryString["fbpgs"];    // final filtering (fb pages, ...) : get fbpgs
            var fbpgs = fbpgsjson == null ? null : JArray.Parse(fbpgsjson).ToObject<String[]>();

            // get from client search word : trim extra tabs, spaces, ...
            if (String.IsNullOrEmpty(searchValue) == false) searchValue = searchValue.Trim(new char[] { ' ', '\'', '\t' });

            // get the number of all entries (before applyng any filter) : fast because we query only count
            var totalItemsCount = new Arabizer().loaddeserializeT_FB_Posts_Count_DAPPERSQL();

            // get main (whole) data from DB first while filtering on search term if any
            // List<a> items;
            List<b> items;
            if (wholeWord && !String.IsNullOrEmpty(searchValue))
            {
                // load and search items : do it at the level of the query using contains full-text sql index
                items = new Arabizer().loaddeserializeT_FB_POST_JOIN_COMMENT_FullText_Filter_DAPPERSQL(searchValue, minDate, maxDate, fbpgs)
                    .Select(c => new b
                    {
                        id = c.id,
                        fbPageName = c.fbPageName,
                        pt = String.IsNullOrWhiteSpace(searchValue) ? c.pt.TruncateTo(400) : truncateBySent(c.pt, searchValue, sentPrepend: 0, sentAppend: 0, viewMoreTag: "[...]", startTruncTag: "[...]", returnSourceIfKeywordNotFound: true),
                        tt = c.tt,
                        lc = c.lc,
                        cc = c.cc,
                        dp = Convert.ToDateTime(c.dp).ToString("yy-MM-dd HH:mm")
                    }).ToList();
            }
            else
            {
                // load items
                items = new Arabizer().loaddeserializeT_FB_POST_JOIN_COMMENT_Like_Filter_DAPPERSQL(searchValue, minDate, maxDate, fbpgs)
                    .Select(c => new b
                    {
                        id = c.id,
                        fbPageName = c.fbPageName,
                        pt = String.IsNullOrWhiteSpace(searchValue) ? c.pt.TruncateTo(400) : truncateBySent(c.pt, searchValue, sentPrepend: 0, sentAppend: 0, viewMoreTag: "[...]", startTruncTag: "[...]", returnSourceIfKeywordNotFound: true),
                        tt = c.tt,
                        lc = c.lc,
                        cc = c.cc,
                        dp = Convert.ToDateTime(c.dp).ToString("yy-MM-dd HH:mm")
                    }).ToList();
            }

            // adjust itemsPerPage case show all
            if (itemsPerPage == -1)
                itemsPerPage = totalItemsCount;

            //
            var itemsFilteredCount = items.Count;

            // page as per request (index of page and length)
            items = items.Skip(start).Take(itemsPerPage).ToList();

            // get other helper data from DB
            List<M_ARABICDARIJAENTRY_TEXTENTITY> ners = new Arabizer().loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_SocialSearch_DAPPERSQL();

            // for comments, if only one found, return and uncollapse it out
            /*String extraData = null;
            if (itemsFilteredCount == 1)
                // extraData = items[0].id;
                extraData = items[0].id;*/

            // excludes POS NER (PRONOMS, PREPOSITIONS, ...), plus also MAIN
            ners.RemoveAll(m => m.TextEntity.Type == "PREPOSITION"
                                || m.TextEntity.Type == "PRONOUN"
                                || m.TextEntity.Type == "ADVERB"
                                || m.TextEntity.Type == "CONJUNCTION"
                                || m.TextEntity.Type == "MAIN ENTITY");

            // left join : Visual formatting before sending back
            // var items1 = items0.ToList();
            var items1 = items.ToList();
            items1.ForEach(s =>
            {
                s.FormattedEntities = TextTools.DisplayEntities(s.id, ners);
            });

            // final filtering (ners, ...) : get ners
            if (!String.IsNullOrWhiteSpace(nersjson))
            {
                JArray a = JArray.Parse(nersjson);
                foreach (var item in a)
                {
                    items1 = items1.Where(c => c.FormattedEntities != null && c.FormattedEntities.Contains((String)item)).ToList();
                }
            }

            //
            return JsonConvert.SerializeObject(new
            {
                recordsTotal = totalItemsCount.ToString(),
                recordsFiltered = itemsFilteredCount.ToString(),
                // data = items,
                // data = items0,
                data = items1,
                // extraData
            });
        }

        /*private static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }*/

        public static string truncateBySent(string source, string searchWord, int sentPrepend = 1, int sentAppend = 1, bool onlyShowFirst = true, string viewMoreTag = "", bool alwaysShowViewMoreTag = false, string startTruncTag = "", bool returnSourceIfKeywordNotFound = false, string returnNotFound = "")
        {
            // going to be the final string
            string truncated = "";

            // parse source sentences
            string[] sents = Regex.Split(source, @"(?<=[.?!;])\s+(?=\p{Lu})");

            // create some search start & end holders
            int i = 0;
            int ssent = -1;
            int esent = 0;

            // find start / end
            foreach (string sent in sents)
            {
                // search using regex for word boundaries \b
                // if (Regex.IsMatch(sent, "\\b" + searchWord + "\\b", RegexOptions.IgnoreCase))
                if (Regex.IsMatch(sent, searchWord, RegexOptions.IgnoreCase))
                {
                    if (ssent == -1)
                        ssent = i;
                    else
                        esent = i;
                }

                i++;
            }

            // make final string:
            if (esent == 0 || onlyShowFirst == true) esent = ssent;

            i = 0;

            foreach (string sent in sents)
            {
                if (i == ssent - sentPrepend || i == ssent || i == esent + sentAppend || (i >= ssent - sentPrepend && i <= esent + sentAppend))
                {
                    truncated = truncated + sent + " ";
                }

                i++;
            }

            // add view more
            if (esent + sentAppend + 1 < sents.Count() || alwaysShowViewMoreTag == true)
            {
                truncated = truncated + viewMoreTag;
            }

            // add beginning tag
            if (ssent - sentPrepend > 0)
            {
                truncated = startTruncTag + truncated;
            }

            // check if anything was even found:
            if (ssent == -1)
            {
                if (returnSourceIfKeywordNotFound)
                {
                    truncated = source;
                }
                else
                {
                    truncated = returnNotFound;
                }
            }

            // and now return the final string - do a trim and remove double spaces.
            // did i ever mention how much i despise double spaces?
            return truncated.Trim().Replace("  ", " ");
        }

        public class b
        {
            public String id;
            public String fk_influencer;
            public String pt;
            public String fbPageName;
            public String FormattedEntities;
            public String tt;
            public int lc;
            public int cc;
            public String dp;
        }

        [HttpGet]
        public object DataTablesNet_ServerSide_FB_Comments_GetList(string id)
        {
            //
            if (string.IsNullOrEmpty(id))
                return null;

            // get from client side, from where we start the paging
            int start = 0;
            int.TryParse(this.Request.QueryString["start"], out start);            // GET

            // get from client side, to which length the paging goes
            int itemsPerPage = 10;
            int.TryParse(this.Request.QueryString["length"], out itemsPerPage);    // GET

            // get from client search word
            string searchValue = this.Request.QueryString["search[value]"]; // GET
            if (String.IsNullOrEmpty(searchValue) == false) searchValue = searchValue.Trim(new char[] { ' ', '\'', '\t' });

            // get main (whole) data from DB first
            var items = new Arabizer().loaddeserializeT_FB_Comments_DAPPERSQL(id).Select(c => new
            {
                Id = c.Id,
                message = c.message,
                translated_message = c.translated_message,
                created_time = c.created_time.ToString("yy-MM-dd HH:mm")
            }).ToList();

            // get the number of entries
            var itemsCount = items.Count;

            // adjust itemsPerPage case show all
            if (itemsPerPage == -1)
                itemsPerPage = itemsCount;

            // filter on search term if any
            if (!String.IsNullOrEmpty(searchValue))
                items = items.Where(a => a.message.ToUpper().Contains(searchValue.ToUpper()) || (a.translated_message != null && a.translated_message.ToUpper().Contains(searchValue.ToUpper()))).ToList();

            var itemsFilteredCount = items.Count;

            // page as per request (index of page and length)
            items = items.Skip(start).Take(itemsPerPage).ToList();

            //
            return JsonConvert.SerializeObject(new
            {
                recordsTotal = itemsCount.ToString(),
                recordsFiltered = itemsFilteredCount.ToString(),
                data = items
            });
        }

        #region FRONT YARD ACTIONS SETUP DB
        [HttpGet]
        public void SetupCreateFillDBFromXML()
        {
            // Warning this methiod has to be executed (on VM and on server) once to make the swicth between XML serialization to DB. Otherwise any new entry 
            // to DB will be erased by the ones previously in xml

            using (var db = new ArabiziDbContext())
            {
                var M_TWINGLYACCOUNTs = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRYs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRY_LATINWORDs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRY_TEXTENTITYs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_TEXTENTITY>(Server.MapPath("~/App_Data"));
                var M_ARABIZIENTRYs = new TextPersist().Deserialize<M_ARABIZIENTRY>(Server.MapPath("~/App_Data"));
                var M_XTRCTTHEMEs = new TextPersist().Deserialize<M_XTRCTTHEME>(Server.MapPath("~/App_Data"));
                var M_XTRCTTHEME_KEYWORDs = new TextPersist().Deserialize<M_XTRCTTHEME_KEYWORD>(Server.MapPath("~/App_Data"));

                // clean too early date => 2017-01-01 00:00:00 (DateTime.MinValue)
                M_ARABIZIENTRYs.Where(w => w.ArabiziEntryDate == DateTime.MinValue).ToList().ForEach(s => s.ArabiziEntryDate = DateTime.Now.AddYears(-1));

                //
                db.M_TWINGLYACCOUNTs.AddRange(M_TWINGLYACCOUNTs);
                db.M_ARABICDARIJAENTRYs.AddRange(M_ARABICDARIJAENTRYs);
                db.M_ARABICDARIJAENTRY_LATINWORDs.AddRange(M_ARABICDARIJAENTRY_LATINWORDs);
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.AddRange(M_ARABICDARIJAENTRY_TEXTENTITYs);
                db.M_ARABIZIENTRYs.AddRange(M_ARABIZIENTRYs);
                db.M_XTRCTTHEMEs.AddRange(M_XTRCTTHEMEs);
                db.M_XTRCTTHEME_KEYWORDs.AddRange(M_XTRCTTHEME_KEYWORDs);

                // commit
                db.SaveChanges();
            }
        }

        [HttpGet]
        public void SetupDeleteDB()
        {
            using (var db = new ArabiziDbContext())
            {
                db.Database.Delete();
            }
        }
        #endregion

        #region BACK YARD BO LOAD
        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY(AccessMode accessMode)
        {
            /*if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABICDARIJAENTRY();
            else*/
            if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABICDARIJAENTRY_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABICDARIJAENTRY_DAPPERSQL();

            return null;
        }

        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRYs.ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY ";

                conn.Open();
                return conn.Query<M_ARABICDARIJAENTRY>(qry).ToList();
            }
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY(AccessMode accessMode)
        {
            /*if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABIZIENTRY();
            else*/
            if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABIZIENTRY_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABIZIENTRY_DAPPERSQL();

            return null;
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABIZIENTRYs.ToList();
            }
        }

        private M_ARABIZIENTRY loaddeserializeM_ARABIZIENTRY_EFSQL_uow(Guid arabiziWordGuid, ArabiziDbContext db)
        {
            // using (var db = new ArabiziDbContext())
            // {
            return db.M_ARABIZIENTRYs.SingleOrDefault(m => m.ID_ARABIZIENTRY == arabiziWordGuid);
            // }
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABIZIENTRY";

                conn.Open();
                return conn.Query<M_ARABIZIENTRY>(qry).ToList();
            }
        }

        private M_ARABIZIENTRY loaddeserializeM_ARABIZIENTRY_DAPPERSQL(Guid arabiziWordGuid)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABIZIENTRY WHERE ID_ARABIZIENTRY = '" + arabiziWordGuid + "'";

                conn.Open();
                return conn.QueryFirst<M_ARABIZIENTRY>(qry);
            }
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD(AccessMode accessMode)
        {
            /*if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD();
            else*/
            if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();

            return null;
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRY_LATINWORDs.ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY_LATINWORD";

                conn.Open();
                return conn.Query<M_ARABICDARIJAENTRY_LATINWORD>(qry).ToList();
            }
        }

        private List<M_TWINGLYACCOUNT> loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_TWINGLYACCOUNT";

                conn.Open();
                return conn.Query<M_TWINGLYACCOUNT>(qry).ToList();
            }
        }

        private void updateT_FB_INFLUENCERAsId(string influencerid, string themeId, string Text, bool? isAutoRetrieveFBPostandComments = false)
        {
            if (!string.IsNullOrEmpty(influencerid) && !string.IsNullOrEmpty(Text))
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    // MC300118 update target entities for the current FB page and the current theme
                    String qry = "UPDATE T_FB_INFLUENCER SET TargetEntities = N'" + Text + "', AutoRetrieveFBPostAndComments = '" + isAutoRetrieveFBPostandComments + "' "
                                + "WHERE id = '" + influencerid + "' "
                                + "AND fk_theme = '" + themeId + "' ";
                    using (SqlCommand cmd = new SqlCommand(qry, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
        }

        /*ivate List<FBFeedComment> GetComments(string ids)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "";
                if (!string.IsNullOrEmpty(ids))
                {
                    qry = "SELECT * FROM FBFeedComments WHERE id IN (" + ids + ") AND translated_message IS NULL ";
                }

                conn.Open();
                return conn.Query<FBFeedComment>(qry).ToList();
            }
        }*/

        private void SaveTranslatedPost(string postid, string translatedText)
        {
            // Check before
            if (string.IsNullOrEmpty(postid) || string.IsNullOrEmpty(translatedText))
                return;

            // clean 
            translatedText = translatedText.Replace("'", "''");

            //
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "UPDATE T_FB_POST SET translated_text = N'" + translatedText + "' WHERE id = '" + postid + "'";
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        /*private void SaveTranslatedComments(string commentid, string translatedText)
        {
            // Check before
            if (string.IsNullOrEmpty(commentid) || string.IsNullOrEmpty(translatedText))
                return;

            // clean 
            translatedText = translatedText.Replace("'", "''");

            //
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "UPDATE FBFeedComments SET translated_message = N'" + translatedText + "' WHERE id = '" + commentid + "'";
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }*/

        private void updateFb_PostMailBody(string postid, string mailBody = "", int? noOfTimeMailSend = 0)
        {
            // Check before
            if (string.IsNullOrEmpty(mailBody) && (noOfTimeMailSend <= 0))
                return;

            // Check before
            if (string.IsNullOrEmpty(postid))
                return;

            //
            String qry = "UPDATE T_FB_POST SET ";

            // clean 
            mailBody = mailBody.Replace("'", "''");

            //
            if (!string.IsNullOrEmpty(mailBody))
                qry = qry + "MailBody = N'" + mailBody + "' ";

            //
            if (noOfTimeMailSend > 0)
                qry = qry + "NoOfTimeMailSend = '" + noOfTimeMailSend + "', LastMailSendOn = '" + DateTime.Now + "' ";

            //
            qry = qry + "WHERE id = '" + postid + "'";

            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(qry, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        private List<ArabiziToArabicViewModel> loadArabiziToArabicViewModel_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT "
                        + "ROW_NUMBER() OVER (ORDER BY ARZ.ArabiziEntryDate) AS Rank, "
                        + "ARZ.ID_ARABIZIENTRY, "
                        + "ARZ.ArabiziEntryDate, "
                        + "ARZ.ArabiziText, "
                        + "AR.ID_ARABICDARIJAENTRY, "
                        + "AR.ArabicDarijaText "
                    + "FROM T_ARABIZIENTRY ARZ "
                    + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                    + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                conn.Open();
                return conn.Query<ArabiziToArabicViewModel>(qry).ToList();
            }
        }

        private List<ArabiziToArabicViewModel> loadArabiziToArabicViewModel_DAPPERSQL(bool activeThemeOnly, String userId, bool adminModeShowAll = false)
        {
            if (adminModeShowAll == true)
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT "
                            + "ROW_NUMBER() OVER (ORDER BY ARZ.ArabiziEntryDate) AS Rank, "
                            + "ARZ.ID_ARABIZIENTRY, "
                            + "ARZ.ArabiziEntryDate, "
                            + "ARZ.ArabiziText, "
                            + "AR.ID_ARABICDARIJAENTRY, "
                            + "AR.ArabicDarijaText, "
                            + "ARTE.TextEntity_Mention "
                        + "FROM T_ARABIZIENTRY ARZ "
                        + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                        + "INNER JOIN T_ARABICDARIJAENTRY_TEXTENTITY ARTE ON AR.ID_ARABICDARIJAENTRY = ARTE.ID_ARABICDARIJAENTRY AND TextEntity_Type = 'MAIN ENTITY' "
                        + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                    conn.Open();
                    return conn.Query<ArabiziToArabicViewModel>(qry).ToList();
                }
            }
            else if (activeThemeOnly)
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT "
                            + "ROW_NUMBER() OVER (ORDER BY ARZ.ArabiziEntryDate) AS Rank, "
                            + "ARZ.ID_ARABIZIENTRY, "
                            + "ARZ.ArabiziEntryDate, "
                            + "ARZ.ArabiziText, "
                            + "AR.ID_ARABICDARIJAENTRY, "
                            + "AR.ArabicDarijaText, "
                            + "ARTE.TextEntity_Mention "
                        + "FROM T_ARABIZIENTRY ARZ "
                        + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                        + "INNER JOIN T_ARABICDARIJAENTRY_TEXTENTITY ARTE ON AR.ID_ARABICDARIJAENTRY = ARTE.ID_ARABICDARIJAENTRY AND TextEntity_Type = 'MAIN ENTITY' "
                        + "INNER JOIN T_XTRCTTHEME XT ON XT.ThemeName = ARTE.TextEntity_Mention AND XT.CurrentActive = 'active' AND XT.UserID = '" + userId + "' AND ARZ.ID_XTRCTTHEME = XT.ID_XTRCTTHEME "
                        + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                    conn.Open();
                    return conn.Query<ArabiziToArabicViewModel>(qry).ToList();
                }
            }
            else
                return loadArabiziToArabicViewModel_DAPPERSQL();
        }

        private int loadArabiziToArabicViewModelCount_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT COUNT(*) "
                    + "FROM T_ARABIZIENTRY ";

                conn.Open();
                return conn.QueryFirst<int>(qry);
            }
        }
        #endregion

        #region BACK YARD BO HELPERS
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        #endregion

        #region BACK YARD BO TOKEN MGT
        private void ManageToken(string userId)
        {
            var token = Session["_T0k@n_"];
            bool istokenexpire = _IAuthenticate.IsTokenExpire(Convert.ToString(token));
            if (token == null || istokenexpire)
            {
                // MC191217 tokens are good to track who calls the arabizi function (api or web app), but web app user does not need to generate manually a token,
                // the fact that he is logged on, means he should be able to use translation, means a token should be always ready for him. Solution (workaround?) generate
                // on the fly a new token each time the token is expired

                /*Session["_T0k@n_"] = String.Empty;
                Session["message"] = String.Empty;
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "Your token is expired.";*/

                // generate token (and fill session token)
                var clientKeyToolkit = new ClientKeysConcrete();
                var clientkeys = clientKeyToolkit.GetGenerateUniqueKeyByUserID(userId);
                string message = string.Empty;
                bool isAppValid = clientKeyToolkit.IsAppValid(clientkeys);
                if (isAppValid == false)
                    message = "No More calls";
                else
                {
                    String sessiontoken;
                    String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                    var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out sessiontoken);
                    Session["_T0k@n_"] = sessiontoken;
                    if (String.IsNullOrEmpty(sessiontoken))
                        message = tokenmessage; // pass message only if token not generated
                }

                // fill session data
                Session["message"] = message;
                if (clientkeys != null)
                    Session["userId"] = clientkeys.UserID;
            }
        }
        #endregion
    }

    public static class JoinHelper
    {
        public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
        {
            return outer
                .GroupJoin(inner, outerKeySelector, innerKeySelector, (outerObj, inners) =>
                new
                {
                    outerObj,
                    inners = inners.DefaultIfEmpty()
                })
            .SelectMany(a => a.inners.Select(innerObj => resultSelector(a.outerObj, innerObj)));
        }
    }

    public static class TruncateHelper
    {
        public static string TruncateTo(this string input, int truncateLength, bool addEllipsis = true)
        {
            // Check to see if we even need to bother truncating
            if (string.IsNullOrWhiteSpace(input) || input.Length <= truncateLength)
                return input;

            // copy the input
            var temp = input;

            // Define a list of characters that we can safely break on
            char[] anyOf = { '.', ',', ';', ':', '?', '!', '،', '؟' };

            // LastIndexOfAny starts at the max position works backwards thru' the string
            var truncatePosition = temp.LastIndexOfAny(anyOf, truncateLength);

            // no appropriate place to truncate. 
            if (truncatePosition == -1)
            {
                // Return original string but you could fall back to splitting on last space
                return temp;
            }
            temp = temp.Substring(0, truncatePosition + (addEllipsis ? 0 : 1));
            return addEllipsis
                ? temp + "..."
                : temp;
        }
    }

    // Class for return the method call after translate Fb Posts and comments
    public class ReturnData
    {
        public string Message { get; set; }
        public bool Status { get; set; }
        public string TranslatedText { get; set; }
        public List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities { get; set; }
    }

    public static class UtilityFunctions
    {
        public static EmailSettings GetEmailSetting()
        {
            EmailSettings emailsetting = new EmailSettings();
            emailsetting.SMTPServerUrl = ConfigurationManager.AppSettings["SMTPServerUrl1"];
            emailsetting.SMTPServerPort = Convert.ToInt16(ConfigurationManager.AppSettings["SMTPServerPort1"]);
            emailsetting.SMTPSecureConnectionRequired = Convert.ToBoolean(ConfigurationManager.AppSettings["SMTPSecureConnectionRequired1"]);
            emailsetting.SMTPServerLoginName = ConfigurationManager.AppSettings["SMTPServerLoginName1"];
            emailsetting.SMTPServerPassword = ConfigurationManager.AppSettings["SMTPServerPassword1"];
            emailsetting.NoReplyEmailAddress = ConfigurationManager.AppSettings["NoReplyEmailAddress1"];
            return emailsetting;
        }
    }

    public class EmailSettings
    {
        public string SMTPServerUrl { get; set; }
        public int SMTPServerPort { get; set; }
        public bool SMTPSecureConnectionRequired { get; set; }
        public string SMTPServerLoginName { get; set; }
        public string SMTPServerPassword { get; set; }
        public string NoReplyEmailAddress { get; set; }
    }

    public enum PartialViewType
    {
        all = 0,
        workingDataOnly,
        FBPagesOnly,
        StatsOnly
    }
}