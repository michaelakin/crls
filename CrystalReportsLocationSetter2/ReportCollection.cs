using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CrystalDecisions.CrystalReports.Engine;
using System.Collections.Specialized;

namespace CrystalReportLocationSetter2
{
    public class ReportCollection : ObservableCollection<CrlsReportDocument>
    {
        public void NotifyChanged()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
