using CAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RemoteProcedure
{
    /// <summary>
    /// Interaction logic for RemoteCall.xaml
    /// </summary>
    public partial class RemoteCall : Window
    {
        public RemoteCall()
        {
            InitializeComponent();
            var userInfo = ApplicationState.GetValue<UserMetadata>("Metadata");
            using (var context = new CarContext())
            {
                var uc = new UserController();
                var user = uc.GetUserInfo(context, userInfo.BadgeNumber);
                PoliteMessage.Text = $"This is a reminder that your old asset({user?.PREVIOUS_BARCODE}) needs to be returned to your Accountable Property Officer(APO) or Property Custodial Officer (PCO).Please contact your APO or PCO to arrange for the asset to be returned in a timely manner.If the asset is not received within 7 business days, your supervisor will be contacted.";
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
