﻿//******************************************************************************
// <copyright file="license.md" company="Wlog project  (https://github.com/arduosoft/wlog)">
// Copyright (c) 2016 Wlog project  (https://github.com/arduosoft/wlog)
// Wlog project is released under LGPL terms, see license.md file.
// </copyright>
// <author>Daniele Fontani, Emanuele Bucaelli</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlog.BLL.Entities;
using Wlog.DAL.NHibernate.Helpers;
using Wlog.Library.BLL.Classes;
using Wlog.Library.BLL.DataBase;
using Wlog.Library.BLL.Enums;
using Wlog.Library.BLL.Interfaces;

namespace Wlog.Library.BLL.Reporitories
{
    /// <summary>
    /// Repo used to store roles
    /// </summary>
    public class RolesRepository : EntityRepository<RolesEntity>
    {
        

        public RolesRepository()
        {
        }


        /// <summary>
        /// Get all application Entity
        /// </summary>
        /// <param name="applicationEntity"></param>
        /// <returns></returns>
        public List<RolesEntity> GetAllApplicationRoles(ApplicationEntity applicationEntity)
        {
            //TODO: apply paging input 
            logger.Debug("[repo] entering GetAllApplicationRoles");

            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                if (applicationEntity != null)
                {

                    List<Guid> ids = uow.Query<AppUserRoleEntity>()
                        .Where(x => x.ApplicationId.Equals(applicationEntity.Id))
                        .Select(x => x.RoleId).ToList();
                    return uow.Query<RolesEntity>().Where(x => ids.Contains(x.Id)).ToList();
                }
            }
            return null;
        }

        /// <summary>
        /// Get all application roe
        /// </summary>
        /// <param name="userEntity"></param>
        /// <returns></returns>
        public List<AppUserRoleEntity> GetApplicationRolesForUser(UserEntity userEntity)
        {
            logger.Debug("[repo] entering GetApplicationRolesForUser");

            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                return uow.Query<AppUserRoleEntity>().Where(c => c.UserId.Equals(userEntity.Id)).ToList();
            }
           
        }

        /// <summary>
        /// Get role by name, name is case sensitive
        /// </summary>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public RolesEntity GetRoleByName(string rolename)
        {
            logger.Debug("[repo] entering GetRoleByName");
            return this.FirstOrDefault(x => x.RoleName == rolename);
        }


        /// <summary>
        /// Return list of all roles
        /// </summary>
        /// <returns></returns>
        public List<RolesEntity> GetAllRoles()
        {
            //TODO: yest there is few rows, but to have coerence with other repos, should we add paging inputs?
            logger.Debug("[repo] entering GetAllRoles");
            return GetAllRoles(RoleScope.All);
        }



        /// <summary>
        /// Get all roles for a given scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public List<RolesEntity> GetAllRoles(RoleScope scope)
        {
            //TODO: why GetAllRoles is not an overload of me? adding also paging parameter...
            logger.Debug("[repo] entering GetAllRoles({0})",scope);
            List<RolesEntity> result = new List<RolesEntity>();
            try
            {

                using (IUnitOfWork uow = BeginUnitOfWork())
                {
                    var query = uow.Query<RolesEntity>();
                    if (scope == RoleScope.None)
                    {
                        return new List<RolesEntity>();
                    }

                    if (scope != RoleScope.All)
                    {
                        if (scope == RoleScope.Application)
                        {
                            query = query.Where(x => x.ApplicationScope == true);
                        }

                        if (scope == RoleScope.Global)
                        {
                            query = query.Where(x => x.GlobalScope == true);
                        }
                    }
                    result.AddRange(query.ToList());
                }
            }
            catch (Exception err)
            {
                logger.Error(err);
            }

            return result;
        }


        /// <summary>
        /// return all roles inherited from profile
        /// </summary>
        /// <param name="userEntity"></param>
        /// <returns></returns>
        public List<ProfilesRolesEntity> GetProfilesRolesForUser(UserEntity userEntity)
        {
            logger.Debug("[repo] entering GetProfilesRolesForUser");
            using (IUnitOfWork uow = BeginUnitOfWork())
            {
                uow.BeginTransaction();
                return uow.Query<ProfilesRolesEntity>().Where(c => c.ProfileId.Equals(userEntity.ProfileId)).ToList();
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        /// <param name="userEntity"></param>
        /// <returns></returns>
        public List<RolesEntity> GetAllRolesForUser(UserEntity userEntity)
        {

            logger.Debug("[repo] entering GetAllRolesForUser");
            List<RolesEntity> result = new List<RolesEntity>();

            try
            {
                var rolesIds = GetProfilesRolesForUser(userEntity).Select(x => x.RoleId);

                using (IUnitOfWork uow = BeginUnitOfWork())
                {
                    result.AddRange(uow.Query<RolesEntity>().Where(x => rolesIds.Contains(x.Id)).ToList());
                }
            }
            catch (Exception err)
            {
                logger.Error(err);
            }

            return result;
        }
    }
}
