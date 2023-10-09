using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVC_SYSTEM.Models;
using System.ComponentModel.DataAnnotations;
using MVC_SYSTEM.LabourModels;

namespace MVC_SYSTEM.CustomModels
{
    public class MYEGTemplate
    {
        public List<tbl_LbrDataInfo> tbl_LbrDataInfo { get; set; }
        public string fld_EstateName { get; set; }
    }
}