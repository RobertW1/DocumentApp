using DocumentAppWeb.Models;
using DocumentAppWeb.Utils;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DocumentAppWeb.Controllers
{
    public class DocumentController : Controller
    {
        public ActionResult SearchAllDocuments(string searchString, string libraryTitle)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {
                ViewBag.LibraryTitle = libraryTitle;
                var vm = DocumentHelper.GetAllDocumentsInLibrary(context, libraryTitle, searchString);
                return View("AllDocuments", vm);
            }
        }

        public ActionResult Search(string searchString, string libraryTitle)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {
                ViewBag.LibraryTitle = libraryTitle;
                var vm = DocumentHelper.GetSharedDocumentsInLibrary(context, libraryTitle, searchString);
                return View("SharedDocuments", vm);
            }
        }

        public ActionResult AllDocuments(string libraryTitle, string folderPath = null)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {
                ViewBag.LibraryTitle = libraryTitle;
                var vm = DocumentHelper.GetAllItemsInFolder(context, libraryTitle, folderPath);
                return View(vm);
            }
        }

        public ActionResult SharedDocuments(string libraryTitle, string folderPath)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {               
                ViewBag.LibraryTitle = libraryTitle;
                var vm = DocumentHelper.GetSharedItemsInFolder(context, libraryTitle, folderPath);
                return View(vm);
            }
        }

        public ActionResult AddLink(string url, string libraryTitle, string itemFolderUrl = null)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {             
                var link = DocumentHelper.AddGuestLink(context, url, true);

                return RedirectToAction("AllDocuments", new { libraryTitle = libraryTitle, folderPath = itemFolderUrl });
            }
        }

        public ActionResult DeleteLink(string url, string libraryTitle, string itemFolderUrl = null)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {
                DocumentHelper.DeleteGuestLink(context, url, true);

                return RedirectToAction("AllDocuments", new { libraryTitle = libraryTitle, folderPath = itemFolderUrl });
            }
        }

        [HttpPost]
        public PartialViewResult DeleteLink2(ListItemVM item)
        {
            using (var context = ContextProvider.CreateAppOnlyContext())
            {
                DocumentHelper.DeleteGuestLink(context, item.FullUrl, true);
                item.IsSharedWithGuest = false;
                item.EditLink = null;
                item.ViewLink = null;

                return PartialView("ListItemAdmin", item);
            }
        }
    }
}