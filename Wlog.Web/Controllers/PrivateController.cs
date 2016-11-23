﻿//******************************************************************************
// <copyright file="license.md" company="Wlog project  (https://github.com/arduosoft/wlog)">
// Copyright (c) 2016 Wlog project  (https://github.com/arduosoft/wlog)
// Wlog project is released under LGPL terms, see license.md file.
// </copyright>
// <author>Daniele Fontani, Emanuele Bucaelli</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wlog.Web.Models;
using Wlog.Web.Code.Helpers;
using System.Web.Security;
using PagedList;
using Wlog.Web.Models.User;
using Wlog.Web.Code.Authentication;
using Wlog.Web.Models.Application;
using Wlog.BLL.Entities;
using Wlog.Library.BLL.Reporitories;
using Wlog.BLL.Classes;
using Wlog.Library.BLL.Classes;
using Wlog.Library.BLL.Enums;
using Wlog.Library.BLL.Helpers;
using Wlog.Library.BLL.Configuration;
using Wlog.Web.Filters;
using NLog;

namespace Wlog.Web.Controllers
{
    public class PrivateController : Controller
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.WriteLog, Constants.Roles.ReadLog)]
        public ActionResult Index()
        {

            logger.Debug("[Private]: Index");
            string username = Membership.GetUser().UserName;
            var applicationsForUser = UserHelper.GetAppsIdsForUser(username);

            LogsSearchSettings logSearch = new LogsSearchSettings()
            {
                Applications = applicationsForUser,
                PageNumber = 1,
                PageSize = 10
            };

            IPagedList<LogEntity> lastestLog = RepositoryContext.Current.Logs.SearchLog(logSearch);
            DashboardModel dm = new DashboardModel();
            dm.ErrorCount = RepositoryContext.Current.Logs.CountByLevel(StandardLogLevels.ERROR);
            dm.InfoCount = RepositoryContext.Current.Logs.CountByLevel(StandardLogLevels.INFO);
            dm.LogCount = RepositoryContext.Current.Logs.CountByLevel(StandardLogLevels.ALL_LEVELS);
            dm.WarnCount = RepositoryContext.Current.Logs.CountByLevel(StandardLogLevels.WARNING);
            dm.LastTen = ConversionHelper.ConvertLogEntityToMessage(lastestLog.ToList());
            dm.QueueLoad = LogQueue.Current.QueueLoad;
            dm.AppLastTen = new List<MessagesListModel>();

            IPagedList<LogEntity> logOfCurrentApp;
            foreach (ApplicationEntity app in UserHelper.GetAppsForUser(username))
            {
                logSearch = new LogsSearchSettings()
                {
                    PageNumber = 1,
                    PageSize = 10

                };

                logSearch.Applications.Add(app.IdApplication);
                logOfCurrentApp = RepositoryContext.Current.Logs.SearchLog(logSearch);
                MessagesListModel list = new MessagesListModel();
                list.ApplicationName = app.ApplicationName;
                list.IdApplication = app.IdApplication;

                list.Messages = ConversionHelper.ConvertLogEntityToMessage(logOfCurrentApp.ToList());
                dm.AppLastTen.Add(list);

            }

            return View(dm);

        }

        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.WriteLog, Constants.Roles.ReadLog)]
        public ActionResult Logs(Guid? applicationId, string level, string sortOrder, string sortBy, string serchMessage, int? page, int? pageSize)
        {
            logger.Debug("[Private]: Logs");
            //TDOD: CHECK USER
            List<Guid> alloweApps = UserHelper.GetAppsIdsForUser(Membership.GetUser().UserName);

            LogListModel mm = new LogListModel()
            {
                ApplicationId = applicationId ?? alloweApps.FirstOrDefault(),
                Level = level,
                SortOrder = sortOrder,
                SortBy = sortBy,
                SerchMessage = serchMessage,


            };
            MembershipUser current = Membership.GetUser();
            mm.Apps = UserHelper.GetAppsForUser(current.UserName);


            // mm.Items = LogHelper.GetLogs(mm.ApplicationId, mm.SortOrder, mm.SortBy, mm.SerchMessage??"", 30, page ?? 1);

            return View(mm);
        }


        public JsonResult Search(Guid? applicationId, string sortOrder, string sortBy, string serchMessage, int page, int pageSize)
        {

            logger.Debug("[Private]: Search");
            //TDOD: CHECK USER
            var result = new JsonResult();

            IPagedList list = LogHelper.GetLogs(applicationId.Value, sortOrder, sortBy, serchMessage ?? "", pageSize, page);

            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            result.Data = new
            {
                draw = Request["draw"],
                recordsTotal = list.TotalItemCount,
                recordsFiltered = list.TotalItemCount,
                data = list
            };
            return result;
        }

        // Get  /Private/ListUsers
        // TODO: set role [Authorize(Roles="ADMIN")]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult ListUsers(string serchMessage, int? page, int? pageSize)
        {
            logger.Debug("[Private]: ListUsers");
            ListUser model = new ListUser
            {
                SerchMessage = serchMessage
            };


            model.UserList = UserHelper.FilterUserList(serchMessage, page ?? 1, pageSize ?? 30);

            return View(model);
        }

        //Get Private/EditUser/1
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult EditUser(Guid id)
        {
            logger.Debug("[Private]: EditUser({0})", id);
            UserEntity user = UserHelper.GetById(id);
            ViewBag.Title = user.Username;
            EditUser model = new EditUser();
            model.DataUser = user;
            model.Apps = UserHelper.GetApp(id);
            return View(model);
        }


        //Post Private/EditUser/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult EditUser(EditUser model)
        {
            logger.Debug("[Private]: EditUser(model)");
            if (ModelState.IsValid)
            {
                try
                {
                    UserHelper.UpdateUser(model.DataUser);

                    List<AppUserRoleEntity> newRoleList = new List<AppUserRoleEntity>();

                    foreach (UserApps app in model.Apps)
                    {

                        if (app.RoleId != Guid.Empty)
                        {
                            newRoleList.Add(new AppUserRoleEntity { UserId = model.DataUser.Id, ApplicationId = app.IdApplication, RoleId = app.RoleId });
                        }
                    }

                    RepositoryContext.Current.Applications.ResetUserRoles(model.DataUser, newRoleList);


                    return RedirectToAction("ListUsers");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, "Error");
                }
            }
            ModelState.AddModelError(String.Empty, "Error");
            return View(model);
        }

        //Get Private/NewUser
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult NewUser()
        {
            logger.Debug("[Private]: NewUser()");
            return View(new NewUser());
        }

        //Post Private/NewUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult NewUser(NewUser user)
        {
            logger.Debug("[Private]: NewUser(user)");
            if (ModelState.IsValid)
            {
                try
                {
                    MembershipCreateStatus status;
                    WLogMembershipProvider provider = new WLogMembershipProvider();
                    provider.CreateUser(user.UserName, user.Password, user.Email, null, null, true, null, out status);
                    if (status == MembershipCreateStatus.Success)
                    {
                        UserEntity entity = UserHelper.GetByUsername(user.UserName);
                        entity.IsAdmin = UserHelper.IsUserAdmin(user.Profile);
                        entity.ProfileId = user.Profile;
                        UserHelper.UpdateUser(entity);
                        return RedirectToAction("EditUser", "Private", new { Id = entity.Id });
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, ErrorCodeToString(status));
                    }

                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError(String.Empty, ErrorCodeToString(e.StatusCode));
                }
            }

            // Se si arriva a questo punto, significa che si è verificato un errore, rivisualizzare il form
            return View(user);
        }

        //Get Private/DeleteUser/1
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult DeleteUser(Guid id)
        {
            logger.Debug("[Private]: DeleteUser({0})", id);

            UserEntity user = UserHelper.GetById(id);
            UserData result = new UserData
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CreationDate = user.CreationDate,
                IsAdmin = user.IsAdmin,
                IsOnLine = user.IsOnLine,
                LastLoginDate = user.LastLoginDate
            };
            return View(result);
        }

        //Post Private/DeleteUser/1
        [HttpPost]
        [AuthorizeRolesAttribute(Constants.Roles.Admin)]
        public ActionResult DeleteUser(UserData user)
        {
            logger.Debug("[Private]: DeleteUser(userdata)");
            if (UserHelper.DeleteById(user.Id))
            {
                return RedirectToAction("ListUsers");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Si è Verificato un Errore.");
            }
            return View(user);
        }


        #region Application
        // Get  /Private/ListApps
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.ReadLog)]
        public ActionResult ListApps(string serchMessage, int? page, int? pageSize)
        {
            logger.Debug("[Private]: ListApps({0},{1},{2})", serchMessage, page, pageSize);
            string username = Membership.GetUser().UserName;

            ApplicationList model = new ApplicationList
            {
                SerchMessage = serchMessage
            };

            model.AppList = ApplicationHelper.FilterApplicationList(serchMessage, page ?? 1, pageSize ?? 30, username);

            return View(model);
        }

        //Get Private/NewApp
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult NewApp()
        {
            logger.Debug("[Private]: NewApp()");
            return View(new ApplicationModel());
        }

        //Post Private/NewApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult NewApp(ApplicationModel model)
        {
            logger.Debug("[Private]: NewApp(model)");
            if (ModelState.IsValid)
            {
                ApplicationEntity entity = new ApplicationEntity();
                entity.ApplicationName = model.ApplicationName;
                entity.IsActive = true;
                entity.StartDate = model.StartDate;
                entity.PublicKey = model.PublicKey;
                entity.PublicKey = Guid.NewGuid();
                RepositoryContext.Current.Applications.Save(entity);

                UserEntity user = UserHelper.GetByUsername(Membership.GetUser().UserName);

                if (!user.IsAdmin)
                {
                    var role = RepositoryContext.Current.Roles.GetAllRolesForUser(user).SingleOrDefault(x => x.RoleName == Constants.Roles.CreateApp);
                    RepositoryContext.Current.Applications.AssignRoleToUser(entity, user, role);
                }

                return RedirectToAction("ListApps");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "error");
            }

            return View(model);
        }


        //Get Private/EditApp/1
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult EditApp(Guid id)
        {
            logger.Debug("[Private]: EditApp({0})", id);
            return View(ApplicationHelper.GetById(id));
        }


        //Post Private/EditApp/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult EditApp(ApplicationModel model)
        {
            logger.Debug("[Private]: EditApp(model)");
            if (ModelState.IsValid)
            {
                ApplicationEntity entity = RepositoryContext.Current.Applications.GetById(model.IdApplication);
                entity.ApplicationName = model.ApplicationName;
                entity.IsActive = model.IsActive;
                entity.StartDate = model.StartDate;
                entity.EndDate = model.EndDate;
                //entity.PublicKey=model.PublicKey; Do not change this is not editable
                RepositoryContext.Current.Applications.Save(entity);
                return RedirectToAction("ListApps");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Error");
            }

            return View(model);
        }

        //Get Private/DeleteApp/1
        [HttpGet]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult DeleteApp(Guid id)
        {
            logger.Debug("[Private]: DeleteApp({0})", id);
            return View(ApplicationHelper.GetById(id));
        }

        //Post Private/DeleteApp/1
        [HttpPost]
        [AuthorizeRolesAttribute(Constants.Roles.Admin, Constants.Roles.CreateApp)]
        public ActionResult DeleteApp(ApplicationModel model)
        {
            logger.Debug("[Private]: DeleteApp(model)");

            if (ApplicationHelper.DeleteById(model.IdApplication))
            {
                return RedirectToAction("ListApps");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Si è Verificato un Errore.");
            }
            return View(model);
        }

        #endregion

        #region Helper
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            logger.Debug("[Private]: ErrorCodeToString({0})", createStatus);
            // Vedere http://go.microsoft.com/fwlink/?LinkID=177550 per
            // un elenco completo di codici di stato.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return Resources.Private.DuplicateUserName;

                case MembershipCreateStatus.DuplicateEmail:
                    return Resources.Private.DuplicateEmail;

                case MembershipCreateStatus.InvalidPassword:
                    return Resources.Private.InvalidPassword;

                case MembershipCreateStatus.InvalidEmail:
                    return Resources.Private.InvalidEmail;

                case MembershipCreateStatus.InvalidAnswer:
                    return Resources.Private.InvalidAnswer;

                case MembershipCreateStatus.InvalidQuestion:
                    return Resources.Private.InvalidQuestion;

                case MembershipCreateStatus.InvalidUserName:
                    return Resources.Private.InvalidUserName;

                case MembershipCreateStatus.ProviderError:
                    return Resources.Private.ProviderError;

                case MembershipCreateStatus.UserRejected:
                    return Resources.Private.UserRejected;

                default:
                    return Resources.Private.UnknownAuthorizationError;
            }
        }
        #endregion

        #region Info

        //Get Private/info

        public ActionResult Info()
        {
            var model = InfoHelper.GetInfoPage(InfoPageConfigurator.Configuration);
            return View(model);
        }
        #endregion

        #region Background Jobs

        public ActionResult BackgroundJobs()
        {
            var models = new List<JobsConfigurationModel> {
                new JobsConfigurationModel()
                {
                    Active = true,
                    CronExpression = "0*///",
                    Description = "Test descirption",
                    JobName = "Move to bin job"
                },
                new JobsConfigurationModel()
                {
                    Active = true,
                    CronExpression = "0*///5555",
                    Description = "Test descirptionffffffffffffffffffffffffff",
                    JobName = "Delete logs job"
                },
            };

            return View(models);
        }

        public ActionResult EditBackgroundJobs(JobsConfigurationModel jobs)
        {
            var models = new List<JobsConfigurationModel> {
                new JobsConfigurationModel()
                {
                    Active = true,
                    CronExpression = "0*///",
                    Description = "Test descirption",
                    JobName = "Move to bin job"
                },
                new JobsConfigurationModel()
                {
                    Active = true,
                    CronExpression = "0*///5555",
                    Description = "Test descirptionffffffffffffffffffffffffff",
                    JobName = "Delete logs job"
                },
            };

            return View(models);
        }

        #endregion
    }
}
