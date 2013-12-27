using System.Windows;
using Microsoft.Phone.Shell;
using VikingApi.Api;

namespace Fuel.LoginControl
{
    public delegate void LoginFinishedEventHandler(object sender, ApiBrowserEventArgs args);

    public partial class OauthLogin
    {
        public OauthLogin()
        {
            InitializeComponent();
        }

        public event LoginFinishedEventHandler LoginFinished;

        protected void OnLoginFinished(ApiBrowserEventArgs args)
        {
            if (LoginFinished != null)
            {
                LoginFinished(this, args);
            }
        }

        #region buttons

        private async void Login_OnTap(object sender, RoutedEventArgs e)
        {
            /*if (string.IsNullOrWhiteSpace(UserName.Text))
            {
                Tools.Message.ShowToast("Please provide a username");
                return;
            }
            if (string.IsNullOrWhiteSpace(PassWord.Password))
            {
                Tools.Message.ShowToast("Please provide a password");
                return;
            }*/

            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "hold on while we request access";
            using (var client = new VikingsApi())
            {
                if (await client.Authorize("jan.de.dobbeleer@gmail.com", "Sgom1981jj?"))
                {
                    var finished = new ApiBrowserEventArgs { Success = true };
                    OnLoginFinished(finished);
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Tools.Message.ShowToast("login unsuccessful, please try again");
                }
            }
        }
        #endregion
    }
}