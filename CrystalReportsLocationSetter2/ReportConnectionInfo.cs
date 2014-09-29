using CrystalDecisions.Shared;
using CrystalReportsLocationSetter2;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalReportLocationSetter2
{
    public class ReportConnectionInfo : ObservableObject
    {
        public ReportConnectionInfo(string server, string database, string username, string password, bool integratedSecurity)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
            IntegratedSecurity = integratedSecurity;
        }

        public string Server
        {
            get { return Settings.Default.Server; }
            set 
            {
                Settings.Default.Server = value;
                RaisePropertyChanged(() => Server);
                Settings.Default.Save();
            }
        }

        public string Database
        {
            get { return Settings.Default.Database; }
            set 
            {
                Settings.Default.Database = value;
                RaisePropertyChanged(() => Database);
                Settings.Default.Save();
            }
        }

        public string Username
        {
            get { return Settings.Default.Username; }
            set 
            {
                Settings.Default.Username = value;
                RaisePropertyChanged(() => Username);
                Settings.Default.Save();
            }
        }

        public string Password
        {
            get { return Settings.Default.Password; }
            set 
            {
                Settings.Default.Password = value;
                RaisePropertyChanged(() => Password);
                Settings.Default.Save();
            }
        }

        public bool IntegratedSecurity
        {
            get { return Settings.Default.IntegratedSecurity; }
            set 
            {
                Settings.Default.IntegratedSecurity = value;
                RaisePropertyChanged(() => IntegratedSecurity);
                Settings.Default.Save();
            }
        }

        public ConnectionInfo ToConnectionInfo()
        {
            ConnectionInfo info = new ConnectionInfo()
            {
                ServerName = this.Server,
                DatabaseName = this.Database,
                UserID = this.Username,
                Password = this.Password,
                IntegratedSecurity = this.IntegratedSecurity
            };

            return info;
        }

        public List<string> Dsns
        {
            get { return GetDsns(); }
        }

        private static List<string> GetDsns()
        {
            var list = new List<string>();
            list.AddRange(EnumDsn(Registry.CurrentUser));
            list.AddRange(EnumDsn(Registry.LocalMachine));
            return list;
        }

        private static IEnumerable<string> EnumDsn(RegistryKey rootKey)
        {
            var regKey = rootKey.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
            if (regKey != null)
            {
                foreach (string name in regKey.GetValueNames())
                {
                    string value = regKey.GetValue(name, "").ToString();
                    yield return name;
                }
            }
        }
    }
}
