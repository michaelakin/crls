using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Threading.Tasks;
using CrystalDecisions.ReportAppServer.DataDefModel;

namespace CrystalReportLocationSetter2
{
    public class CrlsReportDocument : ReportDocument
    {
        protected override bool CheckLicenseStatus()
        {
            return true;
        }

        public override string FileName
        {
            get
            {
                return base.FileName.Replace("rassdk://", String.Empty);
            }
            set
            {
                base.FileName = value;
            }
        }

        public void UpdateConnectionInfo(ReportConnectionInfo rptConnectionInfo)
        {
            SetDataSourceLocation(rptConnectionInfo, this);
            SetDataSourceLocationForSubreports(rptConnectionInfo, this);
        }

        private void SetDataSourceLocation(ReportConnectionInfo info, ReportDocument crystalReportDocument)
        {
            foreach (IConnectionInfo connectInfo in crystalReportDocument.DataSourceConnections)
            {
                connectInfo.SetConnection(info.Server, info.Database, info.Username, info.Password);
            }

            //Create a new Database Table to replace the reports current table.
            var boTable = new CrystalDecisions.ReportAppServer.DataDefModel.Table();

            //boMainPropertyBag: These hold the attributes of the tables ConnectionInfo object
            PropertyBag boMainPropertyBag = new PropertyBag();
            //boInnerPropertyBag: These hold the attributes for the QE_LogonProperties
            //In the main property bag (boMainPropertyBag)
            PropertyBag boInnerPropertyBag = new PropertyBag();

            //Set the attributes for the boInnerPropertyBag
            boInnerPropertyBag.Add("Database", info.Database);
            boInnerPropertyBag.Add("DSN", info.Server);
            boInnerPropertyBag.Add("SSOKey", "");
            boInnerPropertyBag.Add("Trusted_Connection", (info.IntegratedSecurity ? "True" : "False"));
            boInnerPropertyBag.Add("UseDSNProperties", "False");

            //Set the attributes for the boMainPropertyBag
            boMainPropertyBag.Add("Database DLL", "crdb_odbc.dll");
            boMainPropertyBag.Add("QE_DatabaseName", info.Database);
            boMainPropertyBag.Add("QE_DatabaseType", "ODBC (RDO)");
            //Add the QE_LogonProperties we set in the boInnerPropertyBag Object
            boMainPropertyBag.Add("QE_LogonProperties", boInnerPropertyBag);
            boMainPropertyBag.Add("QE_ServerDescription", info.Server);
            boMainPropertyBag.Add("QE_SQLDB", "True");
            boMainPropertyBag.Add("SSO Enabled", (info.IntegratedSecurity ? "True" : "False"));

            //Create a new ConnectionInfo object
            var boConnectionInfo = new CrystalDecisions.ReportAppServer.DataDefModel.ConnectionInfo();
            //Pass the database properties to a connection info object
            boConnectionInfo.Attributes = boMainPropertyBag;
            //Set the connection kind
            boConnectionInfo.Kind = CrConnectionInfoKindEnum.crConnectionInfoKindCRQE;
            boConnectionInfo.UserName = info.Username;
            boConnectionInfo.Password = info.Password;
            boTable.ConnectionInfo = boConnectionInfo;

            CrystalDecisions.ReportAppServer.DataDefModel.Tables boTables = ReportClientDocument.DatabaseController.Database.Tables;

            foreach (CrystalDecisions.ReportAppServer.DataDefModel.Table table in boTables)
            {
                boTable.Name = table.Name;
                boTable.QualifiedName = table.QualifiedName;
                boTable.Alias = table.Alias;
                ReportClientDocument.DatabaseController.SetTableLocation(table, boTable);
            }
            
            //Verify the database after adding substituting the new table.
            //To ensure that the table updates properly when adding Command tables or Stored Procedures.
            VerifyDatabase();
        }

        private string getOwner(string qualifiedName)
        {
            if (String.IsNullOrWhiteSpace(qualifiedName))
            {
                throw new ArgumentException("qualifiedName");
            }

            string owner = null;
            string[] nameParts = qualifiedName.Split('.');

            if (nameParts.Length == 2) // owner.table
            {
                owner = nameParts[0];
            }
            else if (nameParts.Length == 3) // database.owner.table
            {
                owner = nameParts[1];
            }
            else if (nameParts.Length == 4) // server.database.owner.table
            {
                owner = nameParts[2];
            }

            return owner;
        }

        private void SetDataSourceLocationForSubreports(ReportConnectionInfo connectionInfo, CrlsReportDocument reportDocument)
        {
            if (reportDocument.Subreports == null) return;

            foreach (ReportDocument subreport in reportDocument.Subreports)
            {
                SetDataSourceLocation(connectionInfo, subreport);
            }
        }
    }
}
