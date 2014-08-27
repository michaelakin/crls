using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

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
    }
}
