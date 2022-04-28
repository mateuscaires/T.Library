using Microsoft.Reporting.WinForms;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace T.Windows
{
    public class RDLC
    {
        private const string CT_PDF = "PDF";
        private const string CT_BACK_SLACH = "\\";
        private ReportViewer _rpt;
        private string _deviceInfo;
        private string _reportDirectory;
        private DataTable _table;
        private string _reportName;
        private static RDLC _instance;

        public static RDLC Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RDLC();
                return _instance;
            }
        }

        public RDLC()
        {
            ResetReport();
        }

        public void SetReportPath(string reportPath)
        {
            _reportDirectory = Path.GetDirectoryName(Application.ExecutablePath) + reportPath;
            if (reportPath.EndsWith("\\"))
                return;
            _reportDirectory += "\\";
        }

        public byte[] GetFileBytes(DataTable table, string reportName)
        {
            _table = table;
            _reportName = reportName;
            return Bind();
        }

        private byte[] Bind()
        {
            string[] streams = null;
            Warning[] warnings = null;
            _rpt.LocalReport.DataSources.Clear();
            _rpt.LocalReport.DataSources.Add(new ReportDataSource(_table.TableName, _table));
            _rpt.LocalReport.ReportPath = string.Concat(_reportDirectory, _reportName);
            string mimeType;
            string encoding;
            string fileNameExtension;
            return _rpt.LocalReport.Render("PDF", _deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        }

        private void ResetReport()
        {
            _deviceInfo = @"<DeviceInfo>
                                <OutputFormat>PDF</OutputFormat>
                                <PageWidth>8.3in</PageWidth>
                                <PageHeight>11.7in</PageHeight>
                                <MarginTop>0.0in</MarginTop>
                                <MarginLeft>0.0in</MarginLeft>
                                <MarginRight>0.0in</MarginRight>
                                <MarginBottom>0.0in</MarginBottom>
                            </DeviceInfo>";

            _rpt = new ReportViewer();
            _rpt.ProcessingMode = ProcessingMode.Local;
            _rpt.Dock = DockStyle.Fill;
            _rpt.LocalReport.EnableHyperlinks = true;
            _rpt.TabIndex = 0;
        }
    }
}
