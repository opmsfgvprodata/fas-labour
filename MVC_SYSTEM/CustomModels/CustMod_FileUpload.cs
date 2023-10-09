using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_SYSTEM.CustomModels
{
    public class CustMod_FileUpload
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public DateTime DateTimeModified { get; set; }

        [Display(Name = "Date Time Created")]
        public DateTime DateTimeCreated { get; set; }

        [Display(Name = "Size (K)")]
        public long SizeFile { get; set; }

        public int? RefID { get; set; }
    }

    public class CustMod_FileUpload2
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        public DateTime DateTimeModified { get; set; }

        [Display(Name = "Date Time Created")]
        public DateTime DateTimeCreated { get; set; }

        [Display(Name = "Size (K)")]
        public long SizeFile { get; set; }

        public Guid RefID { get; set; }
    }
}