using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalReportLocationSetter2
{
    public class ReportConnectionInfo
    {
        public ReportConnectionInfo(string server, string database, string username, string password, bool integratedSecurity)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
            IntegratedSecurity = integratedSecurity;
        }

        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IntegratedSecurity { get; set; }
    }
}
