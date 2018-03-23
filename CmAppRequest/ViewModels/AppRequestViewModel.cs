using System;

namespace CmAppRequest.ViewModels
{
    public class AppRequestViewModel
    {
        public string Application { get; set; }
        public string CurrentState { get; set; }
        public string LastModifiedBy { get; set; }
        public string RequestGuid { get; set; }
        public string UserName { get; set; }
        public string Comments { get; set; }
        public string NewComments { get; set; }
        public DateTime LastModifiedTime { get; set; }

        public string ModifyingUser { get; set; }

        public AppRequestViewModel() { }
    }
}