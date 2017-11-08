
using CAR;
using CAR.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();

            var userInfo = ApplicationState.GetValue<UserMetadata>("Metadata");
            using (var context = new CarContext())
            {
                var uc = new UserController();
                var user = uc.GetUserInfo(context, userInfo.BadgeNumber);
                ReceiptText.Text = $"Upon receipt of your new asset, your old asset({user?.BARCODE}) should be returned to the Accountable Property Officer(APO)or Property Custodial Officer(PCO)";
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var uc = new UserController();

            var currentTime = DateTime.Now;

            using (var context = new CarContext())
            {
                var userInfo = ApplicationState.GetValue<UserMetadata>("Metadata");
                var asset = uc.GetAssetInfo(context, userInfo.Barcode);
                var user = uc.GetUserInfo(context, userInfo.BadgeNumber);
                var isNewAsset = false;

                if (asset == null)
                {
                    asset = new ASSET() { CREATED = DateTime.Now }; 
                    isNewAsset = true;
                }

                uc.MapMetadataToAsset(asset, userInfo);
                //asset.APO_PCO_PIV_BADGE_NUM = 1;
                asset.DATETIME_RECEIVED = currentTime;

                if (isNewAsset)
                    context.Assets.Add(asset);

                if (user == null)
                {
                    user = uc.MapMetadataToUser(userInfo);
                    user.APO_PCO_PIV_BADGE_NUM = asset?.APO_PCO_PIV_BADGE_NUM;
                    user.BARCODE = asset?.BARCODE ?? userInfo.Barcode;// : asset.BARCODE;
                    
                    user.DATETIME_ACCEPTED = currentTime;

                    if (asset?.STATE == "ASSIGNED")
                        user.OUTSTANDING_ASSET = false;
                    else if (asset?.STATE == "ASSIGNED_PENDING_RETURN")
                    {
                        user.OUTSTANDING_ASSET = true;
                        user.LAST_REMINDER = currentTime.AddDays(5);
                    }

                    context.Users.Add(user);
                }
                else
                {
                    user.PREVIOUS_BARCODE = user.BARCODE;
                    user.DATETIME_ACCEPTED = currentTime;
                    user.BARCODE = asset?.BARCODE;
                    user.APO_PCO_PIV_BADGE_NUM = asset?.APO_PCO_PIV_BADGE_NUM;
                    user.LAST_UPDATED = currentTime;

                    if (asset.STATE == "ASSIGNED")
                        user.OUTSTANDING_ASSET = false;
                    else if (asset.STATE == "ASSIGNED_PENDING_RETURN")
                    {
                        user.OUTSTANDING_ASSET = true;
                        user.LAST_REMINDER = currentTime.AddDays(5);
                    }

                    if (user.PREVIOUS_BARCODE.HasValue)
                    {
                        var oldAsset = uc.GetAssetInfo(context, user.PREVIOUS_BARCODE.Value);
                        oldAsset.STATE = "AWAITING_RETURNED";
                        oldAsset.LAST_UPDATED = currentTime;
                    }
                }

                asset.STATE = "ACCEPTED";

                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

                this.Close();
            }
        }
    }
}
