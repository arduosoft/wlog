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
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Wlog.BLL.Entities;

namespace Wlog.Library.DAL.Nhibernate.Mappings
{
    public class LogEntityMap : ClassMapping<LogEntity>
    {
        public LogEntityMap()
        {
            Table("wl_logentity");
           // Schema("dbo");
            
            Id(x => x.Uid, map => {  map.Generator(Generators.Guid); });
            Property(x => x.Message,map=> { map.Length(5000); });
            Property(x => x.Level);
            Property(x => x.SourceDate);
            Property(x => x.ApplictionId);
            Property(x => x.UpdateDate);
            Property(x => x.Attributes);
        }
    }
}
