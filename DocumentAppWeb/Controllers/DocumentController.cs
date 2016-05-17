using DocumentAppWeb.Models;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocumentAppWeb.Controllers
{
    public class DocumentController : Controller
    {
        // GET: Document
        [SharePointContextFilter]
        public ActionResult Index()
        {
            //TODO: List of folders

            return View();
        }

        public ActionResult Details()
        {
            var spContext = SharePointContextProvider.Current.GetSharePointContext(HttpContext);

            using (var clientContext = spContext.CreateUserClientContextForSPHost())
            {
                var documentLibrary = clientContext.Web.Lists.GetByTitle("Documents");
                clientContext.Load(documentLibrary);
                var folder = documentLibrary.RootFolder;
                var items = documentLibrary.GetItems(CamlQuery.CreateAllItemsQuery());

                clientContext.Load(folder);
                clientContext.Load(items);
                clientContext.Load(items, its => its.Include(
                    it => it.FieldValuesAsText,
                    it => it.DisplayName,
                    it => it.File.ServerRelativeUrl
                ));

                clientContext.ExecuteQuery();

                var documents = new List<Document>();

                foreach (var item in items)
                {
                    documents.Add(CreateDocumentVM(item));
                }

                return View(documents);
            }
        }

        private Document CreateDocumentVM(ListItem item)
        {
            var vm = new Document();

            vm.Name = item.DisplayName;
            vm.Author = ((FieldUserValue)item["Author"]).LookupValue;
            vm.ModifiedBy = ((FieldUserValue)item["Editor"]).LookupValue;
            vm.CreatedDate = (DateTime)item["Created"];
            vm.ModifiedDate = (DateTime)item["Modified"];
            vm.ServerRelativeUrl = item.File.ServerRelativeUrl;

            return vm;
        }
    }
}