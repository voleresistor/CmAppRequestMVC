﻿using System;
using System.Security.Principal;

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
        public string ActionResult { get; set; }
        public DateTime LastModifiedTime { get; set; }

        //public WindowsIdentity ModifyingUser { get; set; }
        public String ModifyingUser { get; set; }

        public AppRequestViewModel() { }
    }
}