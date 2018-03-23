using CmAppRequest.Models;
using CmAppRequest.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Web.Mvc;

namespace CmAppRequest.Controllers
{
    public class CmAppController : Controller
    {
        static string siteServer = "housccm03.dxpe.com";
        static string nameSpace = "root\\SMS\\site_HOU";
        // TODO: Add user based impersonation to allow for admins to be
        // associated with approve/deny events they create
        //ConnectionOptions connectOptions = new ConnectionOptions();

        ManagementScope myScope = new ManagementScope("\\\\" + siteServer + "\\" + nameSpace);

        // Create a viewmodel from the WMI query results
        private AppRequestViewModel GetRequestData(ManagementObject m)
        {
            // Initialize and populate the ViewModel with the easy fields
            AppRequestViewModel newRequest = new AppRequestViewModel
            {
                Application = m.GetPropertyValue("Application").ToString(),
                Comments = m.GetPropertyValue("Comments").ToString(),
                LastModifiedBy = m.GetPropertyValue("LastModifiedBy").ToString(),
                RequestGuid = m.GetPropertyValue("RequestGuid").ToString(),
                UserName = m.GetPropertyValue("User").ToString(),
                ModifyingUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name
        };

            // Get the two that require some extra processing
            // Convert CurrentState from integer to string from enum
            StateList stateList = (StateList)Convert.ToInt16(m.GetPropertyValue("CurrentState"));
            newRequest.CurrentState = stateList.ToString();

            // Convert datetime to a DateTime object using ParseExact
            Char delim = '.';
            string[] modTime = m.GetPropertyValue("LastModifiedDate").ToString().Split(delim);
            CultureInfo provider = CultureInfo.InvariantCulture;
            newRequest.LastModifiedTime = DateTime.ParseExact(modTime[0], "yyyyMMddHHmmss", provider);
            
            return (newRequest);
        }

        // Perform a WMI Query
        private ManagementObjectCollection PerformWmiQuery(string queryString)
        {
            myScope.Connect();
            ObjectQuery query = new ObjectQuery(queryString);
            ManagementObjectSearcher mySearcher = new ManagementObjectSearcher(myScope, query);

            return (mySearcher.Get());
        }

        // GET: CmApp
        public ActionResult Index()
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE CurrentState = 1");

            // List for parsed results
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();

            foreach (ManagementObject m in results)
            {
                requests.Add(this.GetRequestData(m));
            }

            return View(requests);
        }

        public ActionResult GetAll()
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest");

            // List for parsed results
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();

            foreach (ManagementObject m in results)
            {
                requests.Add(this.GetRequestData(m));
            }

            return View(requests);
        }

        public ActionResult GetDenied()
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE CurrentState = 3");

            // List for parsed results
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();

            foreach (ManagementObject m in results)
            {
                requests.Add(this.GetRequestData(m));
            }

            return View(requests);
        }

        public ActionResult GetCanceled()
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE CurrentState = 2");

            // List for parsed results
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();

            foreach (ManagementObject m in results)
            {
                requests.Add(this.GetRequestData(m));
            }

            return View(requests);
        }

        public ActionResult GetApproved()
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE CurrentState = 4");

            // List for parsed results
            List<AppRequestViewModel> requests = new List<AppRequestViewModel>();

            foreach (ManagementObject m in results)
            {
                requests.Add(this.GetRequestData(m));
            }

            return View(requests);
        }

        public ActionResult ViewRequest(string requestGuid)
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(this.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        public ActionResult Approve(string requestGuid)
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(this.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        [HttpPost]
        public ActionResult Approve(string requestGuid, string newComments, string modUser)
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

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
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    return View(this.GetRequestData(m));
                }
            }

            return Redirect("/CmApp");
        }

        [HttpPost]
        public ActionResult Deny(string requestGuid, string newComments, string modUser)
        {
            ManagementObjectCollection results = this.PerformWmiQuery("SELECT * FROM SMS_UserApplicationRequest WHERE RequestGUID=\"" + requestGuid + "\"");

            if (results.Count.Equals(1))
            {
                foreach (ManagementObject m in results)
                {
                    // Set up our method parameters
                    ManagementBaseObject inParams = m.GetMethodParameters("Deny");
                    inParams["Comments"] = newComments;

                    // Approve the request
                    ManagementBaseObject approval = m.InvokeMethod("Deny", inParams, null);
                }
            }

            // Just redirecting back to root no matter what happens
            // TODO: Add some error catching/handling
            return Redirect("/CmApp");
        }
    }
}