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
using System.Text;
using System.Threading.Tasks;
using PagedList;
using Wlog.BLL.Entities;
using Wlog.Library.BLL.Classes;
using Wlog.Library.BLL.Interfaces;
using Wlog.DAL.NHibernate.Helpers;
using Wlog.BLL.Classes;
using Wlog.Library.BLL.DataBase;

namespace Wlog.Library.BLL.Reporitories
{
    public class ApplicationRepository: EntityRepository
    {

        public ApplicationRepository()
        {
            
        }

        public ApplicationEntity GetById(Guid id)
        {
            logger.Debug("[repo] entering GetById");
            ApplicationEntity app;
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                app = uow.Query<ApplicationEntity>().Where(x => x.IdApplication.Equals(id)).FirstOrDefault();
                return app;
            }
        }

        public void ResetUserRoles(UserEntity entity, List<AppUserRoleEntity> role)
        {
            logger.Debug("[repo] entering ResetUserRoles");
            try
            {
                Guid idapp=role.Select(x=>x.ApplicationId).Distinct().First();
                using (IUnitOfWork uow = BeginUnitOfWork())
                {
                    uow.BeginTransaction();
                    List<AppUserRoleEntity> deleterole = uow.Query<AppUserRoleEntity>()
                        .Where(x => x.UserId.Equals(entity.Id) && x.ApplicationId.Equals(idapp)).ToList();

                    foreach (AppUserRoleEntity del in deleterole)
                    {
                        uow.Delete(del);
                    }

                    foreach (AppUserRoleEntity rol in role)
                    {
                        uow.SaveOrUpdate(rol);
                    }

                    uow.Commit();
                }
            }
            catch (Exception err)
            {
                logger.Error(err);
            }
        }

        /// <summary>
        /// Deletes the application from table AppUserRoleEntity
        /// </summary>
        /// <param name="app">the application that need to be deleted</param>
        public void DeleteApplicationRole(ApplicationEntity app)
        {

            logger.Debug("[repo] entering DeleteApplicationRole");
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();

                List<AppUserRoleEntity> deleterole = uow.Query<AppUserRoleEntity>()
                        .Where(x =>  x.ApplicationId.Equals(app.IdApplication)).ToList();

                foreach (AppUserRoleEntity del in deleterole)
                {
                    uow.Delete(del);
                }

                uow.Commit();
            }
        }

        public IPagedList<ApplicationEntity> Search(ApplicationSearchSettings searchSettings)
        {
            logger.Debug("[repo] entering Search");

            List<ApplicationEntity> entity;

            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                if (string.IsNullOrEmpty(searchSettings.SerchFilter))
                {
                    entity = uow.Query<ApplicationEntity>().OrderBy(x => x.ApplicationName).ToList();
                }
                else
                {
                    entity = uow.Query<ApplicationEntity>()
                        .Where(x => x.ApplicationName.Contains(searchSettings.SerchFilter))
                        .OrderBy(x => x.ApplicationName).ToList();
                }

                UserEntity user = RepositoryContext.Current.Users.GetByUsername(searchSettings.UserName);

                if (!user.IsAdmin)
                {
                    var applicationsForUser = GetAppplicationsIdsByUsername(user.Username);
                    entity = entity.Where(x => applicationsForUser.Contains(x.IdApplication)).ToList();
                }
            }

            return new StaticPagedList<ApplicationEntity>(entity, searchSettings.PageNumber, searchSettings.PageSize, entity.Count);
        }

        public void Delete(ApplicationEntity app)
        {
            logger.Debug("[repo] entering Delete");
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                ApplicationEntity appToDelete = uow.Query<ApplicationEntity>().Where(x => x.IdApplication.Equals(app.IdApplication)).FirstOrDefault();

                List<LogEntity> logs = uow.Query<LogEntity>().Where(x => x.ApplictionId.Equals( app.IdApplication)).ToList();
                foreach (LogEntity e in logs)
                {
                    uow.Delete(e);
                }

                DeleteApplicationRole(app);

                uow.Delete(app);

                uow.Commit();
            }
        }

        public List<ApplicationEntity> GetAppplicationsByUsername(string userName)
        {
            logger.Debug("[repo] entering GetAppplicationsByUsername");
            UserEntity user = RepositoryContext.Current.Users.GetByUsername(userName);
            List<Guid> ids = GetAppplicationsIdsForUser(user);
            return RepositoryContext.Current.Applications.GetByIds(ids);
        }

        public List<ApplicationEntity> GetByIds(List<Guid> ids)
        {
            logger.Debug("[repo] entering GetByIds");

            List<ApplicationEntity> result = new List<ApplicationEntity>();
     
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                foreach (Guid id in ids)
                {
                    result.Add(GetById(id));
                }
                
            }
            return result;
        }

        public void Save(ApplicationEntity app)
        {
            logger.Debug("[repo] entering Save");
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                uow.SaveOrUpdate(app);
                uow.Commit();
            }
        }

        public List<Guid> GetAppplicationsIdsByUsername(string userName)
        {
            logger.Debug("[repo] entering GetAppplicationsIdsByUsername");
            UserEntity user = RepositoryContext.Current.Users.GetByUsername(userName);
            List<Guid> ids = GetAppplicationsIdsForUser(user);
            return ids;
        }

        public List<Guid> GetAppplicationsIdsForUser(UserEntity user)
        {
            logger.Debug("[repo] entering GetAppplicationsIdsForUser");
            return GetAppplicationForUser(user).Select(x => x.IdApplication).ToList();

        }

        public List<ApplicationEntity> GetAppplicationForUser(UserEntity user)
        {
            logger.Debug("[repo] entering GetAppplicationForUser");
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                List<ApplicationEntity> applications;
                if (UserProfileContext.Current.User.IsAdmin)
                {
                    applications = uow.Query<ApplicationEntity>().ToList();
                }
                else
                {
                    List<Guid> appLinks = uow
                        .Query<AppUserRoleEntity>()
                        .Where(x => x.UserId == UserProfileContext.Current.User.Id)
                        .Select(x => x.ApplicationId)
                        .ToList();

                    applications = uow
                        .Query<ApplicationEntity>()
                        .Where(x => appLinks.Contains(x.IdApplication))
                        .ToList();
                }
                return applications;
            }
        }

        public ApplicationEntity GetByApplicationKey(string applicationKey)
        {
            logger.Debug("[repo] entering GetByApplicationKey");
            Guid pk = new Guid(applicationKey);
       
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                return uow.Query<ApplicationEntity>().Where(x => x.IdApplication.Equals(pk) || x.PublicKey.Equals(pk)).FirstOrDefault();

            }
        }

        public bool AssignRoleToUser(ApplicationEntity application, UserEntity user, RolesEntity role)
        {
            logger.Debug("[repo] entering AssignRoleToUser");
            try
            {
          
                using (IUnitOfWork uow = BeginUnitOfWork())
                {
                    uow.BeginTransaction();
                    if (!uow.Query<AppUserRoleEntity>().Any(x => x.ApplicationId.Equals(application.IdApplication) && x.RoleId.Equals(role.Id) && x.UserId.Equals(user.Id)))
                    {
                        AppUserRoleEntity app = new AppUserRoleEntity();
                        app.ApplicationId = application.IdApplication;
                        app.RoleId = role.Id;
                        app.UserId = user.Id;
                        uow.SaveOrUpdate(app);

                        uow.Commit();
                        return true;
                    }
                }
            }
            catch (Exception err)
            {
                logger.Error(err);
                return false;
            }
            return true;
        }
    }
}
