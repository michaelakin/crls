using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalReportLocationSetter2;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;

namespace CrystalReportsLocationSetter2.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private CrlsReportDocument _selectedReport;
        private bool _shouldVerify;
        private string _status;
        private readonly object _reportsLock = new object();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            AddReportsCommand = new RelayCommand(ExecuteAddReportsCommand);
            RemoveCommand = new RelayCommand(ExecuteRemoveCommand);
            RemoveAllCommand = new RelayCommand(ExecuteRemoveAllCommand);
            SetLocationCommand = new RelayCommand(ExecuteSetLocationCommand);
            OpenReportCommand = new RelayCommand(ExecuteOpenReportCommand);
            SaveCommand = new RelayCommand(ExecuteSaveCommand);
            SelectItemsCommand = new RelayCommand<CrlsReportDocument>(ExecuteSelectItemsCommand);

            Reports = new ReportCollection();
            BindingOperations.EnableCollectionSynchronization(Reports, _reportsLock);
            ReportConnectionInfo = new ReportConnectionInfo(
                Settings.Default.Server,
                Settings.Default.Database,
                Settings.Default.Username,
                Settings.Default.Password,
                false);
        }

        public ReportCollection Reports { get; private set; }

        public CrlsReportDocument SelectedReport
        {
            get { return _selectedReport; }
            set { Set<CrlsReportDocument>(() => SelectedReport, ref _selectedReport, value); }
        }

        public RelayCommand AddReportsCommand { get; set; }

        public RelayCommand RemoveCommand { get; set; }

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

        public void ExecuteRemoveCommand()
        {
            Reports.Remove(SelectedReport);
            SelectedReport = null;
        }

        public void ExecuteRemoveAllCommand()
        {
            foreach (var report in Reports)
            {
                Reports.Remove(report);
                report.Dispose();
            }
        }

        public async void ExecuteSetLocationCommand()
        {
            foreach (var report in Reports)
            {
                await SetLocationForReportAsync(report, ShouldVerify);
            }
        }

        public void ExecuteOpenReportCommand()
        {
            
        }
        
        public void ExecuteSaveCommand()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            
            if (dialog.ShowDialog() != DialogResult.OK) return;
            if (String.IsNullOrWhiteSpace(dialog.SelectedPath)) return;            
            
            string path = dialog.SelectedPath;

            for (int i = 0; i < Reports.Count; i++)
            {
                Status = String.Format("Saving report {0}/{1}", i, Reports.Count);
                var report = Reports[i];

                try
                {
                    string newName = Path.Combine(path, Path.GetFileName(report.FileName));
                    Status = String.Format("Saving {0}...", newName);
                    report.SaveAs(newName, false);
                }
                catch (Exception ex)
                {
                    Status = ex.ToString();
                }
            }
            Status = "Finished saving reports.";
        }

        public bool ShouldVerify
        {
            get { return _shouldVerify; }
            set { Set<bool>(() => ShouldVerify, ref _shouldVerify, value); }
        }

        /// <summary>
        /// Gets or sets the status text displayed.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set { Set<string>(() => Status, ref _status, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ReportConnectionInfo"/> for the form.
        /// </summary>
        public ReportConnectionInfo ReportConnectionInfo { get; set; }

        /// <summary>
        /// Performs the "Set Datasource Location" process on a single report.
        /// </summary>
        /// <param name="reports">The report to process.</param>
        /// <param name="isVerify">If true, "Verify Database" will also be performed on the report.</param>
        public async Task SetLocationForReportAsync(CrlsReportDocument doc, bool isVerify)
        {
            Status = String.Format("Setting location for {0}...", Path.GetFileName(doc.FileName));
            await Task.Run(() => doc.UpdateConnectionInfo(ReportConnectionInfo));

            Reports.NotifyChanged();

            if (isVerify)
            {
                await Task.Run(() => doc.VerifyDatabase());
            }

            Status = "Finished setting location for reports.";
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

        
    }
}