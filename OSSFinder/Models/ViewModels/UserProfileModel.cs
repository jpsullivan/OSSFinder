using OSSFinder.Entities;

namespace OSSFinder.Models.ViewModels
{
    public class UserProfileModel
    {
        public UserProfileModel() { }

        public UserProfileModel(User user)
        {
            Username = user.Username;
            EmailAddress = user.EmailAddress;
            UnconfirmedEmailAddress = user.UnconfirmedEmailAddress;
        }

        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string UnconfirmedEmailAddress { get; set; }
        public int TotalPackageDownloadCount { get; set; }
    }
}