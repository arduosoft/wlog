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

namespace Wlog.BLL.Classes
{
    public class LogMessage
    {
        public DateTime SourceDate { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public string ApplicationKey { get; private set; }
        public string Stacktrace { get; set; }
        /// <summary>
        /// JSON string field to store custom attributes
        /// </summary>
        public string Attributes { get; set; }

        public void SetApplication(string applicationKey)
        {
            this.ApplicationKey = applicationKey;
        }
    }
}