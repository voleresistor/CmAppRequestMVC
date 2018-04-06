using CmAppRequest.Models;
using System.Collections.Generic;

namespace CmAppRequest.ViewModels
{
    public class AppQueryViewModel
    {
        public List<AppRequestViewModel> Requests { get; set; }
        public string PageTitle { get; set; }
        public string RequestState { get; set; }
        public string UserFeedback { get; set; }

        public AppQueryViewModel() {}

        public AppQueryViewModel(int searchType)
        {
            StateList stateList = (StateList)searchType;
            if (searchType != 1)
            {
                this.RequestState = stateList.ToString();
                this.PageTitle = RequestState + " Requests";
            }
            else
            {
                // Need to be more specific with PageTitle here because grammar gets wacky
                this.RequestState = stateList.ToString();
                this.PageTitle = "Pending Requests";
            }
        }
    }
}