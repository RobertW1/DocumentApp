using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocumentAppWeb.Models
{
    public class Document
    {
        public string Name { get; set; }
        public string Author { get; set; }

        [Display(Name = "Modified by")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified")]
        public DateTime ModifiedDate { get; set; }

        public string ServerRelativeUrl { get; set; }
    }
}