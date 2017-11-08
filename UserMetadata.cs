using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteProcedure
{
    class UserMetadata
    {
        public long BadgeNumber { get; set; }
        public string ComputerName { get; set; }
        public int Barcode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Created { get; set; }
        public string Apo { get; set; }
        public string ManufacturerName { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        

        public void CreateBarcode(string computerName)
        {
            Regex regex = new Regex("[COMPUTERNAME=]{13}${7}");
            Barcode = Convert.ToInt32(regex.Match(computerName).Value.Split(']')[1]);
        }

        public void CaptureBadgeNum(string str)
        {
            Regex badgeReg = new Regex("[OID]{3}\\.[0-9]{1}\\.[0-9]{1}\\.[0-9]{4}\\.[0-9]{8}\\.[0-9]{3}\\.[0-9]{1}\\.[0-9]{1}=[0-9]{10}");
            var num = badgeReg.Match(str).Value.Split('=')[1];
            BadgeNumber = Convert.ToInt64(num);
        }

        public void CaptureEmail(string str)
        {
            Regex badgeReg = new Regex("([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})");
            Email = badgeReg.Match(str).ToString();
        }

        public void CreateFullName()
        {
            
            var fullName = Email.Split('@')[0].Split('.');
            FirstName = fullName[0];
            LastName = fullName[fullName.Length - 1];
        }

        internal void CaptureAssetInfo(List<string> output)
        {
            ManufacturerName = output[2];
            Model = output[0];
            ComputerName = output[1];
            SerialNumber = output[3];
            var startIndex = ComputerName.Length - 8;
            Barcode = Convert.ToInt32(ComputerName.Substring(startIndex, 7));
        }
    }
}
