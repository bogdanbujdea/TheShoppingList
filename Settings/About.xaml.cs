using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheShoppingList.Settings
{
    public sealed partial class About : UserControl
    {
        public About()
        {
            this.InitializeComponent();
            CultureInfo culture = CultureInfo.CurrentCulture;
            var license = MainPage.Page.licenseInformation;
            if(license.IsActive && license.IsTrial)
                txtTrialTime.Text = MainPage.Page.licenseInformation.ExpirationDate.ToString("D", culture);
            else
            {
                txtTrial.Visibility = Visibility.Collapsed;
                txtTrialTime.Visibility = Visibility.Collapsed;
            }
        }
    }
}
