using CmAppRequest.ViewModels;
using System;
using System.Globalization;
using System.Management;

namespace CmAppRequest.Models
{
    public class WmiConnector
    {
        public string SiteServer { get; set; }
        public string SiteCode { get; set; }
        public string NameSpace { get; set; }
        public string FQWmiPath { get; set; }
        public ManagementScope CmAppScope { get; set; }

        public WmiConnector()
        {
            // Initialize our WMI information and connection
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/");
            if (rootWebConfig.AppSettings.Settings.Count > 0)
            {
                SiteServer = rootWebConfig.AppSettings.Settings["SiteServer"].Value.ToString();
                SiteCode = rootWebConfig.AppSettings.Settings["SiteCode"].Value.ToString();
                NameSpace = "root\\SMS\\site_" + rootWebConfig.AppSettings.Settings["SiteCode"].Value.ToString();
                FQWmiPath = "\\\\" + SiteServer + "\\" + NameSpace;
            }

            CmAppScope = new ManagementScope(FQWmiPath,
                new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
        }

        // Create a viewmodel from the WMI query results
        public AppRequestViewModel GetRequestData(ManagementObject m)
        {
            // Initialize and populate the ViewModel with the easy fields
            AppRequestViewModel newRequest = new AppRequestViewModel
            {
                Application = m.GetPropertyValue("Application").ToString(),
                Comments = m.GetPropertyValue("Comments").ToString(),
                LastModifiedBy = m.GetPropertyValue("LastModifiedBy").ToString(),
                RequestGuid = m.GetPropertyValue("RequestGuid").ToString(),
                UserName = m.GetPropertyValue("User").ToString(),
                //ModifyingUser = System.Security.Principal.WindowsIdentity.GetCurrent()
                ModifyingUser = System.Web.HttpContext.Current.User.Identity.Name
            };

            // Get the two that require some extra processing

            /* 
             * We get CurrentState as an integer from the SCCM WMI
             * database, so we need to use the StateList enum to
             * convert that to a string that makes sense to the user
             */
            StateList stateList = (StateList)Convert.ToInt16(m.GetPropertyValue("CurrentState"));
            newRequest.CurrentState = stateList.ToString();

            /*
             * Convert LastModifiedTime to a DateTime object using ParseExact,
             * then convert to user local time for times that make sense
             */
            Char delim = '.';
            string[] modTime = m.GetPropertyValue("LastModifiedDate").ToString().Split(delim);
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime iKnowThisIsUtc = DateTime.ParseExact(modTime[0], "yyyyMMddHHmmss", provider);
            DateTime runTimeKnowsThisIsUtc = DateTime.SpecifyKind(
                iKnowThisIsUtc,
                DateTimeKind.Utc);
            newRequest.LastModifiedTime = runTimeKnowsThisIsUtc.ToLocalTime();

            return (newRequest);
        }

        // Perform a WMI Query
        public ManagementObjectCollection PerformWmiQuery(string queryString)
        {
            CmAppScope.Connect();
            ObjectQuery query = new ObjectQuery(queryString);
            ManagementObjectSearcher mySearcher = new ManagementObjectSearcher(CmAppScope, query);

            return (mySearcher.Get());
        }
    }
}