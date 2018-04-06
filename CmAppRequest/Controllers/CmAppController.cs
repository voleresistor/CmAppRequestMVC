using CmAppRequest.Models;
using CmAppRequest.ViewModels;
using System.Collections.Generic;
using System.Management;
using System.Web.Mvc;

namespace CmAppRequest.Controllers
{
    public class CmAppController : Controller
    {
        /*
         * Changed wmiConnector from a static property of this controller
         * to a dynamic object created by the calling method. I assume this
         * will generate significantly more orphaned connections, but should
         * prevent issues that appear to stem from UserB getting WMI access
         * errors after UserA establishes the static session.
         * 
         * I'd like to explicitly clean up those connections after every method
         * call, but Finalize() doesn't appear to be a particularly user
         * friendly method so I'm just letting C# garbage collection handle
         * the whole thing for now.
         */

        /*
         * Index takes an optional searchType that maps to the states
         * in StateList.cs and displays results accordingly.
         */
        public ActionResult Index(int searchType = 1)
        {
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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

        /* 
         * ViewRequest displays more detail about a
         * particular request
         */
        public ActionResult ViewRequest(string requestGuid)
        {
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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
            // Create our WMI connector for this session
            WmiConnector wmiConnector = new WmiConnector();

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