using CAR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace RemoteProcedure
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var userController = new UserController();
            userController.Initialize();

            var timer = new Timer();
            timer.Interval = 25000;
            timer.Elapsed += OnTimedEvent;

            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
             if (ApplicationState.GetValue<bool>("IsImpolite")) return;

            var context = new CarContext();
            var userInfo = ApplicationState.GetValue<UserMetadata>("Metadata");
            long badgeNum = userInfo.BadgeNumber;
            var user = context.Users.Where(u => u.USER_PIV_BADGE_NUM == badgeNum);


            if (user.Any())
            {
                var curUser = user.First();
                var apo = context.Pcos.Where(u => u.APO_PCO_PIV_BADGE_NUM == curUser.APO_PCO_PIV_BADGE_NUM);
                if (curUser.OUTSTANDING_ASSET.HasValue
                    && curUser.OUTSTANDING_ASSET.Value)
                {
                    if ((apo?.First().CAN_SEND_REMINDER ?? true) && curUser.LAST_REMINDER != null && curUser.LAST_REMINDER < DateTime.Now)
                    {
                        Current.Dispatcher.BeginInvoke((Action)OnMainThread);
                        ApplicationState.SetValue("IsImpolite", true);
                    }
                    else
                        Current.Dispatcher.BeginInvoke((Action)OpenPoliteReminder);
                }

            }
        }

        private void OpenPoliteReminder()
        {
            new RemoteCall().Show();
        }

        private void OnMainThread()
        {
            new ImpoliteReminder().Show();
        }
    }
}
