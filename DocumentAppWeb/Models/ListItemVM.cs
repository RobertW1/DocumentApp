using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocumentAppWeb.Models
{
    public class ListItemVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string FileName { get; set; }

        [Display(Name = "Modified by")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ModifiedDate { get; set; }

        public string Path { get; set; }

        public string LibraryTitle { get; set; }

        public string ContentType { get; set; }

        public bool IsSharedWithGuest { get; set; }

        public string EditLink { get; set; }

        public string ViewLink { get; set; }

        public string Icon { get; set; }

        public string FullUrl { get; set; }

        public string FolderUrl { get; set; }
    }
}