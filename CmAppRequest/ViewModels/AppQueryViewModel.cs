using CmAppRequest.Models;
using System.Collections.Generic;

namespace CmAppRequest.ViewModels
{
    public class AppQueryViewModel
    {
        public List<AppRequestViewModel> Requests { get; set; }
        public string PageTitle { get; set; }
        public string UserFeedback { get; set; }

        public AppQueryViewModel()
        {
            this.PageTitle = "Pending Requests";
        }

        public AppQueryViewModel(int searchType)
        {
            if (searchType != 1)
            {
                StateList stateList = (StateList)searchType;
                this.PageTitle = stateList.ToString() + " Requests";
            }
            else
            {
                this.PageTitle = "Pending Requests";
            }
        }
    }
}