using System;

using System.IO;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using System.Windows;
using CAR;
using CAR.Model;

namespace RemoteProcedure
{
    internal class UserController
    {
        private UserMetadata _userInfo;
        private string _output;

        public UserController()
        {
            _userInfo = new UserMetadata();
            //Initialize();
        }

        public void Initialize()
        {
            CreateCommand();

            //string path = @"C:\Users\Public\certutil.txt";
            //string str = File.ReadAllText(path);
            
            MapUserProperties(_output);
            MapAssetProperties();
            ApplicationState.SetValue("Metadata", _userInfo);

            using (var context = new CarContext())
            {
                var asset = GetAssetInfo(context, _userInfo.Barcode);
                var user = GetUserInfo(context, _userInfo.BadgeNumber);

                ApplicationState.SetValue("Metadata", _userInfo);

                if (asset == null || user == null || (user != null && !user.DATETIME_ACCEPTED.HasValue))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)OnMainThread);
                }
            }
        }

        private void MapAssetProperties()
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();
            startinfo.FileName = @"cmd.exe";
            startinfo.Arguments = "/c wmic computersystem get model & wmic computersystem get name & wmic computersystem get manufacturer & wmic bios get serialnumber";
            Process process = new Process();
            process.StartInfo = startinfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.StandardInput.WriteLine("ls -ltr /opt/*.tmp");

            var items = new List<string>() { "model", "name", "manufacturer", "serialnumber"};
            var output = new List<string>();
            try
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    if (string.IsNullOrEmpty(line.Trim())) continue;

                    if (!items.Contains(line.Trim().ToLower()))
                        output.Add(line);
                }
            }
            catch (Exception e)
            {
            }
            
            _userInfo.CaptureAssetInfo(output);
            
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            process.Close();
        }

        private void OnMainThread()
        {
            new Register().Show();
        }

        public USER MapMetadataToUser(UserMetadata userInfo)
        {
            var user = new USER()
            {
                USER_PIV_BADGE_NUM = userInfo.BadgeNumber,
                FIRST_NAME = userInfo.FirstName,
                LAST_NAME = userInfo.LastName,
                EMAIL = userInfo.Email,
                BARCODE = userInfo.Barcode,
                LAST_UPDATED = DateTime.Now,
                CREATED = DateTime.Now
            };

            return user;
        }

        public ASSET MapMetadataToAsset(ASSET asset, UserMetadata userInfo)
        {
            //var asset = new ASSET()
            //{
            asset.COMPUTER_NAME = userInfo.ComputerName;
            asset.MANUFACTURER = userInfo.ManufacturerName;
            asset.SERIAL_NUM = userInfo.SerialNumber;
            asset.BARCODE = userInfo.Barcode;
            asset.MODEL = userInfo.Model;
            asset.LAST_UPDATED = DateTime.Now;            
            asset.DATETIME_ASSIGNED = DateTime.Now;
            //};

            return asset;
        }

        private void MapUserProperties(string str)
        {
            _userInfo.CaptureBadgeNum(str);
            _userInfo.CaptureEmail(str);
            _userInfo.CreateFullName();
            _userInfo.Created = _userInfo.LastUpdated = DateTime.Now;
        }

        public void CreateCommand()
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe");

            psi.UseShellExecute = true;
            psi.RedirectStandardInput = true;
            psi.FileName = @"cmd.exe";
            psi.Arguments = @"/c CertUtil.exe –SCInfo -silent";
            Process process = new Process();
            process.StartInfo = psi;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            _output = process.StandardOutput.ReadToEnd();
            
            process.WaitForExit();
        }

        private string CreateString()
        {
            StringBuilder sbc = new StringBuilder();
            //sbc.Append("CD/");
            //sbc.Append(Environment.NewLine);
            sbc.Append("cd C:\\Users\\Public");
            sbc.Append(Environment.NewLine);
            sbc.Append("CertUtil.exe –SCInfo");

            return sbc.ToString();
        }

        public USER GetUserInfo(CarContext context, long badgeNumber)
        {
           return  context.Users.FirstOrDefault(u => u.USER_PIV_BADGE_NUM == badgeNumber);
        }

        public ASSET GetAssetInfo(CarContext context, int barcode)
        {
            return context.Assets.FirstOrDefault(a => a.BARCODE == barcode);
        }
    }
}