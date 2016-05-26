using DocumentAppWeb.Models;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Sharing;
using Microsoft.SharePoint.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentAppWeb.Utils
{
    public class DocumentHelper
    {
        public static string AddGuestLink(ClientContext context, string url, bool isEditLink)
        {
            var result = Web.CreateAnonymousLink(context, url, isEditLink);
            context.ExecuteQuery();
            return result.Value;
        }

        public static void DeleteGuestLink(ClientContext context, string url, bool isEditLink)
        {
            Web.DeleteAnonymousLinkForObject(context, url, isEditLink, false);
            context.ExecuteQuery();
        }

        public static List<ListItemVM> GetAllDocumentsInLibrary(ClientContext context, string libraryTitle, string filename = null)
        {
            var VMs = new List<ListItemVM>();

            var documentLibrary = context.Web.Lists.GetByTitle(libraryTitle);
            var query = string.IsNullOrEmpty(filename) ? CamlQuery.CreateAllItemsQuery() : CreateFilenameQuery(filename);
            var items = documentLibrary.GetItems(query);

            context.Load(items, its => its.Where(
            it => it.ContentType.Name == "Document")
                  .Include(
                    it => it.Id,
                    it => it.DisplayName,
                    it => it.ContentType.Name,
                    it => it["FileLeafRef"],
                    it => it["FileRef"],
                    it => it["Author"],
                    it => it["Editor"],
                    it => it["Created"],
                    it => it["Modified"]
                  )
            );
            context.ExecuteQuery();

            var itemInfoDict = GetItemInformation(context, items);
            context.ExecuteQuery();

            foreach (var item in items)
            {
                VMs.Add(GetListItemVM(context, item, itemInfoDict[item], libraryTitle));
            }

            return VMs;
        }

        public static List<ListItemVM> GetAllItemsInFolder(ClientContext context, string libraryTitle, string folderPath = null, string filename = null)
        {
            var VMs = new List<ListItemVM>();

            var documentLibrary = context.Web.Lists.GetByTitle(libraryTitle);
            CamlQuery query = CreateAllItemsInFolderQuery(context, documentLibrary, folderPath);

            var items = documentLibrary.GetItems(query);

            context.Load(items, its => its.Where(
            it => it.ContentType.Name == "Folder" || it.ContentType.Name == "Document")
                  .Include(
                    it => it.Id,
                    it => it.DisplayName,
                    it => it.ContentType.Name,
                    it => it["FileLeafRef"],
                    it => it["FileRef"],
                    it => it["Author"],
                    it => it["Editor"],
                    it => it["Created"],
                    it => it["Modified"]
                  )
            );
            context.ExecuteQuery();

            var itemInfoDict = GetItemInformation(context, items);
            context.ExecuteQuery();

            foreach (var item in items)
            {
                VMs.Add(GetListItemVM(context, item, itemInfoDict[item], libraryTitle));
            }

            return VMs;
        }

        public static List<ListItemVM> GetSharedDocumentsInLibrary(ClientContext context, string libraryTitle, string filename = null)
        {
            var VMs = new List<ListItemVM>();

            var documentLibrary = context.Web.Lists.GetByTitle(libraryTitle);
            var query = string.IsNullOrEmpty(filename) ? CamlQuery.CreateAllItemsQuery() : CreateFilenameQuery(filename);
            var items = documentLibrary.GetItems(query);

            context.Load(items, its => its.Where(
            it => it.ContentType.Name == "Document" && ObjectSharingInformation.GetObjectSharingInformation(context, it, true, true, true, false, false, false, false).IsSharedWithGuest)
                  .Include(
                    it => it.Id,
                    it => it.DisplayName,
                    it => it.ContentType.Name,
                    it => it["FileLeafRef"],
                    it => it["FileRef"],
                    it => it["Author"],
                    it => it["Editor"],
                    it => it["Created"],
                    it => it["Modified"]
                  )
            );
            context.ExecuteQuery();

            var itemInfoDict = GetItemInformation(context, items);
            context.ExecuteQuery();

            foreach (var item in items)
            {
                VMs.Add(GetListItemVM(context, item, itemInfoDict[item], libraryTitle));
            }

            return VMs;
        }

        public static List<ListItemVM> GetSharedItemsInFolder(ClientContext context, string libraryTitle, string folderPath = null, string filename = null)
        {
            var VMs = new List<ListItemVM>();

            var documentLibrary = context.Web.Lists.GetByTitle(libraryTitle);
            CamlQuery query = CreateAllItemsInFolderQuery(context, documentLibrary, folderPath);

            var items = documentLibrary.GetItems(query);

            context.Load(items, its => its.Where(
            it => ObjectSharingInformation.GetObjectSharingInformation(context, it, true, true, true, false, false, false, false).IsSharedWithGuest
                  || it.ContentType.Name == "Folder")
                  .Include(
                    it => it.Id,
                    it => it.DisplayName,
                    it => it.ContentType.Name,
                    it => it["FileLeafRef"],
                    it => it["FileRef"],
                    it => it["Author"],
                    it => it["Editor"],
                    it => it["Created"],
                    it => it["Modified"]
                  )
            );
            context.ExecuteQuery();

            var itemInfoDict = GetItemInformation(context, items);
            context.ExecuteQuery();

            foreach (var item in items)
            {
                VMs.Add(GetListItemVM(context, item, itemInfoDict[item], libraryTitle));
            }

            return VMs;
        }

        private static CamlQuery CreateAllItemsInFolderQuery(ClientContext context, List library, string folderPath)
        {
            string path;
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                path = folderPath;
            }
            else
            {
                context.Load(library, dl => dl.RootFolder.ServerRelativeUrl);
                context.ExecuteQuery();
                path = library.RootFolder.ServerRelativeUrl;
            }

            var query = new CamlQuery();
            query.ViewXml = "<View Scope=\"RecursiveAll\"> " +
                                "<Query>" +
                                    "<Where>" +
                                        "<Eq>" +
                                            "<FieldRef Name=\"FileDirRef\" />" +
                                            "<Value Type=\"Text\">" + path + "</Value>" +
                                        "</Eq>" +
                                    "</Where>" +
                                "</Query>" +
                             "</View>";

            return query;
        }

        private static CamlQuery CreateFilenameQuery(string filename)
        {
            var query = new CamlQuery();
            query.ViewXml = "<View Scope=\"RecursiveAll\"> " +
                                "<Query>" +
                                    "<Where>" +
                                        "<BeginsWith>" +
                                            "<FieldRef Name=\"FileLeafRef\" />" +
                                            "<Value Type=\"Text\">" + filename + "</Value>" +
                                        "</BeginsWith>" +
                                    "</Where>" +
                                "</Query>" +
                            "</View>";

            return query;
        }

        private static Dictionary<ListItem, ItemInfo> GetItemInformation(ClientContext context, ListItemCollection items)
        {
            var itemSharingInfo = new Dictionary<ListItem, ItemInfo>();
            foreach (var item in items)
            {
                var itemInfo = new ItemInfo();
                if (item.ContentType.Name == "Document")
                {
                    itemInfo.SharingInfo = ObjectSharingInformation.GetObjectSharingInformation(context, item, false, true, false, true, true, true, true);
                    context.Load(itemInfo.SharingInfo, o => o.IsSharedWithGuest, o => o.AnonymousEditLink, o => o.AnonymousViewLink);
                    itemInfo.Icon = context.Web.MapToIcon((string)item["FileLeafRef"], string.Empty, IconSize.Size16);
                }

                itemSharingInfo[item] = itemInfo;
            }

            return itemSharingInfo;
        }

        private static ListItemVM GetListItemVM(ClientContext context, ListItem item, ItemInfo itemInfo, string libraryTitle)
        {
            var vm = new ListItemVM();

            vm.Id = item.Id;
            vm.Name = item.DisplayName;
            vm.FileName = (string)item["FileLeafRef"];
            vm.Author = ((FieldUserValue)item["Author"]).LookupValue;
            vm.ModifiedBy = ((FieldUserValue)item["Editor"]).LookupValue;
            vm.CreatedDate = (DateTime)item["Created"];
            vm.ModifiedDate = (DateTime)item["Modified"];
            vm.Path = (string)item["FileRef"];
            vm.LibraryTitle = libraryTitle;
            vm.ContentType = item.ContentType.Name;
            vm.FullUrl = new Uri(context.Url).GetLeftPart(UriPartial.Authority) + vm.Path;            

            if (item.ContentType.Name == "Document")
            {
                vm.FolderUrl = vm.Path.Replace("/" + vm.FileName, "");
                vm.Icon = "~/images/" + itemInfo.Icon.Value;
                vm.IsSharedWithGuest = itemInfo.SharingInfo.IsSharedWithGuest;
                if (vm.IsSharedWithGuest)
                {
                    vm.ViewLink = itemInfo.SharingInfo.AnonymousViewLink;
                    vm.EditLink = itemInfo.SharingInfo.AnonymousEditLink;
                }
            }
            else if (item.ContentType.Name == "Folder")
            {
                vm.Icon = "~/images/folder.gif";
            }

            return vm;
        }

        private class ItemInfo
        {
            public ObjectSharingInformation SharingInfo { get; set; }
            public ClientResult<string> Icon { get; set; }
        }
    }
}