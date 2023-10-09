using MVC_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.LabourModels
{
    public class MVC_SYSTEM_Models_Config : DbConfiguration
    {
        public MVC_SYSTEM_Models_Config()
        {
            AddInterceptor(new StringTrimmerInterceptor());
        }
    }
}