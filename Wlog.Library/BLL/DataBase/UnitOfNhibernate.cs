﻿//******************************************************************************
// <copyright file="license.md" company="Wlog project  (https://github.com/arduosoft/wlog)">
// Copyright (c) 2016 Wlog project  (https://github.com/arduosoft/wlog)
// Wlog project is released under LGPL terms, see license.md file.
// </copyright>
// <author>Daniele Fontani, Emanuele Bucaelli</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wlog.DAL.NHibernate.Helpers;
using Wlog.Library.BLL.Interfaces;

//Wlog.Library.BLL.DataBase.UnitOfNhibernate
namespace Wlog.Library.BLL.DataBase
{
    internal class UnitOfNhibernate : IUnitOfWork
    {

        private ITransaction transaction;

        public ISession Session { get; private set; }

        bool uncommitted = true;

        public UnitOfNhibernate()
        {

            Session = NHibernateContext.Current.SessionFactory.OpenSession();
            BeginTransaction();
        }


        public void BeginTransaction()
        {
            transaction = Session.BeginTransaction();
            uncommitted = true;
        }

        public void Commit()
        {
            try
            {
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {

            }

            uncommitted = false;
        }

        public void Dispose()
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
            }
            finally
            {
                try
                {
                    Session.Close();
                }
                catch
                { 
                }
            }
        }

        public void SaveOrUpdate(IEntityBase entity)
        {
            Session.SaveOrUpdate(entity);
        }

        public IQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }

        public void Delete(IEntityBase entity)
        {
            Session.Delete(entity);
        }
    }
}
