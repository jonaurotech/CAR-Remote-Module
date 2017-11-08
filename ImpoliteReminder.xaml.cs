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
    /// Interaction logic for ImpoliteReminder.xaml
    /// </summary>
    public partial class ImpoliteReminder : Window
    {
        public ImpoliteReminder()
        {
            InitializeComponent();

            var userInfo = ApplicationState.GetValue<UserMetadata>("Metadata");
            using (var context = new CarContext())
            {
                var uc = new UserController();
                var user = uc.GetUserInfo(context, userInfo.BadgeNumber);
                Message.Text = $"This is a reminder that your old asset({user?.PREVIOUS_BARCODE}) needs to be returned to your Accountable Property Officer(APO) or Property Custodial Officer (PCO). The asset has passed the return timeline. Please contact your APO or PCO now. \n Click Accept after contacting to resume work";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var context = new CarContext();
            int? badgeNum = 1;
            var user = context.Users.Where(u => u.APO_PCO_PIV_BADGE_NUM == badgeNum);
            if (user.Any())
            {
                var curUser = user.First();
                if (curUser.OUTSTANDING_ASSET.HasValue
                    && curUser.OUTSTANDING_ASSET.Value
                    && curUser.LAST_REMINDER >= DateTime.Now)
                {
                    this.Close();
                    ApplicationState.SetValue("IsImpolite", false);
                }
            }
        }
    }
}
