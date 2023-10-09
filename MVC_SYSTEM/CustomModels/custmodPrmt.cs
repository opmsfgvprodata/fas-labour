using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVC_SYSTEM.Models;
using System.ComponentModel.DataAnnotations;
using MVC_SYSTEM.LabourModels;
using MVC_SYSTEM.MasterModels;

namespace MVC_SYSTEM.CustomModels
{
    public class custmodPrmt
    {

        public  List<tbl_LbrDataInfo> tbl_LbrDataInfo { get; set; }

        public tbl_SAPCCPUP tbl_SAPCCPUP { get; set; }

    }
}