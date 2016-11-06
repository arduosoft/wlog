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
using Wlog.Web.Code.Authentication;
using Wlog.Web.Code.Helpers;
using System.Web.Security;
using Wlog.BLL.Entities;
using Wlog.DAL.NHibernate.Helpers;
using Wlog.BLL.Classes;
using Wlog.Library.BLL.Reporitories;
using NLog;

namespace Wlog.Web.Controllers
{
    public class HomeController : Controller
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: /Home/
        [Authorize(Roles ="ADMIN")]
        public ActionResult Index()
        {

            logger.Debug("[Home]: Index");
            List<ApplicationHomeModel> result = new List<ApplicationHomeModel>();
            if (UserProfileContext.Current.User != null)
            {
                WLogRoleProvider roleProvider = new WLogRoleProvider();

                UserEntity currentUser=RepositoryContext.Current.Users.GetByUsername(Membership.GetUser().UserName);
                List<ApplicationEntity> apps = RepositoryContext.Current.Applications.GetAppplicationForUser(currentUser);
                result.AddRange(ConversionHelper.ConvertListEntityToListApplicationHome(apps));

            }
            return View(result);
        }
        
    }
}
