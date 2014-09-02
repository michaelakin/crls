using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalReportLocationSetter2;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CrystalReportsLocationSetter2.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly object _reportsLock = new object();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            AddReportsCommand = new RelayCommand(ExecuteAddReportsCommand);
            RemoveCommand = new RelayCommand<IList<CrlsReportDocument>>(ExecuteRemoveCommand);
            RemoveAllCommand = new RelayCommand(ExecuteRemoveAllCommand);
            SetLocationCommand = new RelayCommand(ExecuteSetLocationCommand);
            OpenReportCommand = new RelayCommand(ExecuteOpenReportCommand);
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            SelectItemsCommand = new RelayCommand<CrlsReportDocument>(ExecuteSelectItemsCommand);

            Reports = new ObservableCollection<CrlsReportDocument>();
            BindingOperations.EnableCollectionSynchronization(Reports, _reportsLock);
        }

        public ObservableCollection<CrlsReportDocument> Reports { get; private set; }

        private CrlsReportDocument _selectedReport;
        public CrlsReportDocument SelectedReport
        {
            get { return _selectedReport; }
            set
            {
                _selectedReport = value;
                RaisePropertyChanged(() => SelectedReport);
            }
        }

        public RelayCommand AddReportsCommand { get; set; }

        public RelayCommand<IList<CrlsReportDocument>> RemoveCommand { get; set; }

        public RelayCommand RemoveAllCommand { get; set; }

        public RelayCommand SetLocationCommand { get; set; }

        public RelayCommand OpenReportCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand<CrlsReportDocument> SelectItemsCommand { get; set; }

        public void ExecuteSelectItemsCommand(CrlsReportDocument report)
        {
            SelectedReport = report;
        }

        public async void ExecuteAddReportsCommand()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Report files (*.rpt)|*.rpt";

            if (ofd.ShowDialog() == true)
            {
                string[] filePath = ofd.FileNames;
                await AddReportsAsync(filePath);
            }
        }

        public void ExecuteRemoveCommand(IList<CrlsReportDocument> reports)
        {
            foreach (var report in reports)
            {
                Reports.Remove(report);
                report.Dispose();
            }
        }

        public void ExecuteRemoveAllCommand()
        {
            foreach (var report in Reports)
            {
                Reports.Remove(report);
                report.Dispose();
            }
        }

        public void ExecuteSetLocationCommand()
        {

        }

        public void ExecuteOpenReportCommand()
        {

        }

        public void ExecuteSaveCommand()
        {

        }



        private bool _shouldVerify;
        public bool ShouldVerify
        {
            get { return _shouldVerify; }
            set
            {
                SetValue<bool>(ref _shouldVerify, value);
            }
        }

        /// <summary>
        /// The server/DSN name.
        /// </summary>
        public string Server
        {
            get { return Settings.Default.Server; }
            set
            {
                Settings.Default.Server = value;
                RaisePropertyChanged("Server");
            }
        }

        /// <summary>
        /// The database to which the reports will connect.
        /// </summary>
        public string Database
        {
            get { return Settings.Default.Database; }
            set
            {
                Settings.Default.Database = value;
                RaisePropertyChanged("Database");
            }
        }

        /// <summary>
        /// The username for the new connection (required if not using Integrated Security).
        /// </summary>
        public string Username
        {
            get { return Settings.Default.Username; }
            set
            {
                Settings.Default.Username = value;
                RaisePropertyChanged("Username");
            }
        }

        /// <summary>
        /// The password for the new connection (required if not using Integrated Security).
        /// </summary>
        public string Password
        {
            get { return Settings.Default.Password; }
            set
            {
                Settings.Default.Password = value;
                RaisePropertyChanged("Password");
            }
        }

        /// <summary>
        /// Value indicating whether or not Integrated Security will be used.
        /// </summary>
        public bool IntegratedSecurity
        {
            get { return Settings.Default.IntegratedSecurity; }
            set
            {
                Settings.Default.IntegratedSecurity = value;
                RaisePropertyChanged("IntegratedSecurity");
            }
        }


        private void SetValue<T>(ref T field, T value, [CallerMemberName] string fieldSetter = "")
        {
            field = value;
            RaisePropertyChanged(fieldSetter);
        }
        

        /// <summary>
        /// Gets or sets the <see cref="ReportConnectionInfo"/> used by the service.
        /// </summary>
        public ReportConnectionInfo ReportConnectionInfo { get; set; }


        /// <summary>
        /// Asynchronously performs the "Set Datasource Location" process on the collection of reports.
        /// </summary>
        /// <param name="reports">The collection of reports to process.</param>
        /// <param name="isVerify">If true, "Verify Database" will also be performed on the reports.</param>
        public async Task SetLocationForReportsAsync(ReportCollection reports, bool isVerify)
        {
            for (int i = 0; i < reports.Count; i++)
            {
                var report = reports[i];

                try
                {
                    await SetLocationForReportAsync(report, isVerify);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    //_logger.LogException(LogLevel.Error, "Set location failed.", ex);
                }
            }
        }

        /// <summary>
        /// Performs the "Set Datasource Location" process on a single report.
        /// </summary>
        /// <param name="reports">The report to process.</param>
        /// <param name="isVerify">If true, "Verify Database" will also be performed on the report.</param>
        public async Task SetLocationForReportAsync(CrlsReportDocument doc, bool isVerify)
        {
            await UdpateConnectionInfoAsync(doc);

            if (isVerify)
            {
                await Task.Run(() => doc.VerifyDatabase());
            }
        }

        #region Private Methods

        private async Task UdpateConnectionInfoAsync(CrlsReportDocument reportDocument)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.ServerName = ReportConnectionInfo.Server;
            connectionInfo.DatabaseName = ReportConnectionInfo.Database;
            connectionInfo.UserID = ReportConnectionInfo.Username;
            connectionInfo.Password = ReportConnectionInfo.Password;

            await SetDataSourceLocationAsync(connectionInfo, reportDocument);
            await SetDataSourceLocationForSubreportsAsync(connectionInfo, reportDocument);
        }

        private Task SetDataSourceLocationAsync(ConnectionInfo connectionInfo, ReportDocument crystalReportDocument)
        {
            Task task = Task.Run(() =>
            {
                foreach (IConnectionInfo connectInfo in crystalReportDocument.DataSourceConnections)
                {
                    connectInfo.SetConnection(connectionInfo.ServerName, connectionInfo.DatabaseName,
                        connectionInfo.UserID, connectionInfo.Password);
                }

                foreach (Table table in crystalReportDocument.Database.Tables)
                {
                    TableLogOnInfo tableLogonInfo = table.LogOnInfo;
                    tableLogonInfo.ConnectionInfo = connectionInfo;
                    table.ApplyLogOnInfo(tableLogonInfo);
                    table.Location = table.LogOnInfo.TableName;
                }
            });

            return task;
        }

        private string _status;
        public string Status 
        {
            get { return _status; }
            set { SetValue<string>(ref _status, value); }
        }

        private Task SetDataSourceLocationForSubreportsAsync(ConnectionInfo connectionInfo, CrlsReportDocument reportDocument)
        {
            Task task = Task.Run(async () =>
            {
                if (reportDocument.Subreports == null) return;

                foreach (ReportDocument subreport in reportDocument.Subreports)
                {
                    await SetDataSourceLocationAsync(connectionInfo, subreport);
                }
            });

            return task;
        }

        private Task AddReportsAsync(string[] files)
        {
            if (null == files) return Task.FromResult(true);

            Task t = new Task(() =>
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];

                    try
                    {
                        Status = String.Format("Loading {0}...", file);
                        var doc = new CrlsReportDocument();
                        doc.Load(file);
                        this.Reports.Add(doc);
                    }
                    catch (Exception error)
                    {
                        Status = error.ToString();
                    }
                }

                Status = "Finished adding reports!";
            });

            t.Start();
            return t;     
        }

        #endregion


    }
}