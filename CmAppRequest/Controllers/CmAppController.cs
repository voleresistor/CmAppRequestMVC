using CmAppRequest.Models;
using CmAppRequest.ViewModels;
using System.Collections.Generic;
using System.Management;
using System.Web.Mvc;

namespace CmAppRequest.Controllers
{
    public class CmAppController : Controller
    {
        // Create our WMI connector for this session
        static WmiConnector wmiConnector = new WmiConnector();

        // Index displays all pending requests
        public ActionResult Index(int searchType = 1)
        {
            /*
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();
            AppQueryViewModel resultsList = new AppQueryViewModel(searchType);
            resultsList.Requests = requests;
            return View(resultsList);
            */
            
            // Create query string from searchType int
            string queryString = "SELECT * FROM SMS_UserApplicationRequest WHERE CurrentState = " + searchType.ToString();
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery(queryString);

            // Convert query results into a list of ViewModel objects
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();
            foreach (ManagementObject m in results)
            {
                requests.Add(wmiConnector.GetRequestData(m));
            }

            AppQueryViewModel resultsList = new AppQueryViewModel(searchType);
            resultsList.Requests = requests;

            return View(resultsList);
        }

        /* ViewRequest displays more detail about a
         * particular request
         */
        public ActionResult ViewRequest(string requestGuid)
        {
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(wmiConnector.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        public ActionResult Approve(string requestGuid)
        {
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            // We know we should only get one result back from this query
            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(wmiConnector.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        [HttpPost]
        public ActionResult Approve(string requestGuid, string newComments, string modUser)
        {
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    // Set up our method parameters
                    ManagementBaseObject inParams = m.GetMethodParameters("Approve");
                    inParams["Comments"] = newComments;

                    // Approve the request
                    ManagementBaseObject approval = m.InvokeMethod("Approve", inParams, null);

                }
            }

            return Redirect("/CmApp");
        }

        public ActionResult Deny(string requestGuid)
        {
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(wmiConnector.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        [HttpPost]
        public ActionResult Deny(string requestGuid, string newComments, string modUser)
        {
            ManagementObjectCollection results = wmiConnector.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    // Set up our method parameters
                    ManagementBaseObject inParams = m.GetMethodParameters("Deny");
                    inParams["Comments"] = newComments;

                    // Approve the request
                    ManagementBaseObject approval = m.InvokeMethod("Deny", inParams, null);

                    AppRequestViewModel result = wmiConnector.GetRequestData(m);

                    // This doesn't currently appear to be functioning
                    if (approval.GetPropertyValue("StatusCode").ToString() == 0.ToString())
                    {
                        result.ActionResult = result.Application + " was successfully denied for user " + result.UserName + " at " + result.LastModifiedTime;
                    }
                    else
                    {
                        result.ActionResult = result.Application + " was not denied for user " + result.UserName + " at " + result.LastModifiedTime;
                    }
                    return RedirectToAction("ViewRequest", "CmApp", result);
                }
            }

            // Just redirecting back to root if the query goes wonky
            // TODO: Add some error catching/handling
            return Redirect("/CmApp");
        }
    }
}