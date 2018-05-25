using FinancialAccountTool.ExcelSerialzation;
using FinancialAccountTool.Properties;
using FinanicalAccountModernClient.DO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FinancialAccountTool
{
    public partial class FinancialAccountTool : Form
    {
        #region members
        private string _inFile = string.Empty;
        private string _outFile = string.Empty;

        private static readonly Settings Settings = Settings.Default;
        private readonly List<AccountsReceivable> _sourceAccountsReceivables = new List<AccountsReceivable>();
        private readonly Dictionary<string, Payment> _payments = new Dictionary<string, Payment>();
        #endregion

        #region FormFunctions
        public FinancialAccountTool()
        {
            InitializeComponent();
        }
        private void FinancialAccountTool_Load(object sender, EventArgs e)
        {
            SetInitButtonStatus();
        }
        private void listBox1_Click(object sender, EventArgs e)
        {
            int index = listBox1.IndexFromPoint(((MouseEventArgs)e).Location);
            if (index == ListBox.NoMatches)
            {
                listBox1.SelectedItem = null;
            }
        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            int index = listBox1.IndexFromPoint(((MouseEventArgs)e).Location);
            if (index != ListBox.NoMatches)
            {
                string file = listBox1.SelectedItem.ToString().Trim();
                if (File.Exists(file))
                {
                    Process.Start(file);
                }
                return;
            }
            Add_ToolStripMenuItem_Click(sender, e);
        }
        private void Add_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = @"Excel File|*.xlsx;*.xls",
                Title = @"Please choose source spreadsheet",
                RestoreDirectory = true
            };

            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                toolStripProgressBar1.Value = 0;
                string file = ofd.FileName;
                if (Path.GetExtension(file).Equals(".xlsx") ||
                    Path.GetExtension(file).Equals(".xls"))
                {
                    _inFile = file;
                    listBox1.Items.Clear();
                    AddListBoxMessage("File to process:");
                    AddListBoxMessage("    " + file);
                    SetButtonStatus(ItemOrButton.Clear, true);

                    if (LoadInitData(_inFile))
                    {
                        SetButtonStatus(ItemOrButton.Process, true);
                        UpdateStatusMessage("Click the Process button.");
                    }
                }
                else
                {
                    AddListBoxMessage("*Please select a valid file.*");
                    SetButtonStatus(ItemOrButton.Clear, true);
                }
            }
        }
        private void Exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void Process_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = @"Excel File|*.xlsx",
                Title = @"Export to Excel, name the output file",
                FileName = "NewReport"+DateTime.Now.ToString("yyyyMMddHHmmss"),
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            _outFile = saveFileDialog.FileName;
            SetButtonStatus(ItemOrButton.Process, false);

            var worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.ProgressChanged += ReportProgress;
            worker.RunWorkerCompleted += RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }
        private void Help_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            //Form3 form = new Form3();
            form.ShowDialog();
        }
        private void Clear_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetInitButtonStatus();
            listBox1.Items.Clear();
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = "Add a file to process.";
            listBox1.HorizontalScrollbar = false;
        }
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index >= 0)
            {
                if (listBox1.Items[e.Index].ToString().Substring(0, 1) == "*")//extra style for lines start with *
                {
                    Font font = new Font(e.Font, FontStyle.Bold);
                    e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(),font, new SolidBrush(Color.DimGray), e.Bounds);
                }
                else
                {
                    e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
                }
            }
            e.DrawFocusRectangle();
        }
        private void SetInitButtonStatus()
        {
            SetButtonStatus(ItemOrButton.Clear, false);
            SetButtonStatus(ItemOrButton.Process, false);
        }
        #endregion

        #region Worker Functions
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as Dictionary<string, object>;

            var outputResult = OutputResult(result?["transactionList"] as IEnumerable<Transaction>,
                result["paymentDict"] as Dictionary<string, Payment>);
            if (outputResult)
            {
                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                listBox1.Items.Add("Job done! Output file:");
                listBox1.Items.Add("    "+_outFile);
                //SetButtonStatus(ItemOrButton.Process, false);
                UpdateStatusMessage("Add a file to process.");
            }
        }
        private bool OutputResult(IEnumerable<Transaction> transactions, Dictionary<string, Payment> payments)
        {
            using (var serializer = new ExcelSerializer(_outFile))
            {
                serializer.Serialize(transactions);
                var paymentList = payments
                    .OrderBy(payment => payment.Value.Account)
                    .ThenBy(payment => payment.Value.Date).Select(payment => payment.Value);
                serializer.Serialize(paymentList);
            }

            return true;
        }
        private void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            _sourceAccountsReceivables.Sort((x, y) =>
                x.Account.CompareTo(y.Account) * 2 + x.DocumentDate.CompareTo(y.DocumentDate));

            var transactionList = new List<Transaction>();
            var paymentDict = new Dictionary<string, Payment>(_payments);

            var indicator = 1;
            foreach (var sourceAccountsReceivable in _sourceAccountsReceivables)
            {
                var transaction = new Transaction(sourceAccountsReceivable);

                var allFitPayments = paymentDict
                    .Where(payment =>
                        (payment.Value.Account.Equals(transaction.Account) &&
                         payment.Value.Date >= transaction.DocumentDate && payment.Value.Balance > 0))
                    .OrderBy(payment => payment.Value.Date);

                foreach (var fitPayment in allFitPayments)
                {
                    paymentDict[fitPayment.Key] = transaction.Exchange(fitPayment.Value);
                    transactionList.Add(transaction);
                    transaction = transaction.GenerateNexTransaction();
                    if (transaction == null) break;
                }

                if (transaction != null)
                {
                    transactionList.Add(transaction);
                }

                ((BackgroundWorker)sender).ReportProgress(indicator++);
            }

            e.Result = new Dictionary<string, object>
            {
                {"transactionList", transactionList},
                {"paymentDict", paymentDict}
            };
        }
        private bool LoadInitData(string filePath)
        {
            _sourceAccountsReceivables.Clear();
            _payments.Clear();
            try
            {
                //using (var fileStream = File.OpenRead(filePath))
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var filExtension = Path.GetExtension(filePath);
                    var workbook = filExtension.Equals(".xls")
                        ? (IWorkbook)new HSSFWorkbook(fileStream)
                        : new XSSFWorkbook(fileStream);
                    var arWorksheet = workbook.GetSheet(Settings.arTab);
                    if (arWorksheet == null)
                    {
                        AddListBoxMessage("*Please select a valid report.xlsx file*");
                        return false;
                    }

                    for (var i = 1; i <= arWorksheet.LastRowNum; i++)
                    {
                        var row = arWorksheet.GetRow(i);
                        _sourceAccountsReceivables.Add(new AccountsReceivable(row));
                    }

                    var paymentWorksheet = workbook.GetSheet(Settings.paymentTab);
                    for (var i = 1; i <= paymentWorksheet.LastRowNum; i++)
                    {
                        var row = paymentWorksheet.GetRow(i);

                        var payment = new Payment(row);
                        _payments.Add(payment.GetKey(), payment);
                    }
                }
            }
            catch(Exception ex) {
                AddListBoxMessage("*"+ex.Message+"*");
                return false;
            }
            toolStripProgressBar1.Maximum = _sourceAccountsReceivables.Count;
            AddListBoxMessage("    AR data amount:" + _sourceAccountsReceivables.Count);
            AddListBoxMessage("    Payment data amount:" + _payments.Count);
            return true;
        }
        #endregion

        #region Helper functions
        private void SetButtonStatus(ItemOrButton ib, bool isEnabled)
        {
            switch (ib)
            {
                case ItemOrButton.Add:
                    cmsAdd.Enabled = tsbAdd.Enabled = tsmiAdd.Enabled = isEnabled;
                    break;
                case ItemOrButton.Clear:
                    cmsClear.Enabled = tsbClear.Enabled = tsmiClear.Enabled = isEnabled;
                    break;
                case ItemOrButton.Process:
                    cmsProcess.Enabled = tsbProcess.Enabled = tsmiProcess.Enabled = isEnabled;
                    break;
                case ItemOrButton.Exit:
                    cmsExit.Enabled = tsbExit.Enabled = tsmiProcess.Enabled = isEnabled;
                    break;
                default:
                    break;
            }
        }
        private void AddListBoxMessage(string str)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action<string>((s) =>
                {
                    this.listBox1.Items.Add(s);
                }), str);
            }
            else
            {
                this.listBox1.Items.Add(str);
            }

            Graphics g = listBox1.CreateGraphics();

            listBox1.HorizontalScrollbar = true;
           // Determine the size for HorizontalExtent using the MeasureString method using the last item in the list.
           int hzSize = (int) (g.MeasureString(listBox1.Items[listBox1.Items.Count -1].ToString(),listBox1.Font).Width * 1.05);
           // Set the HorizontalExtent property.
           listBox1.HorizontalExtent = hzSize;
        }

        private void UpdateStatusMessage(string msg)
        {
            if (toolStrip1.InvokeRequired)
            {
                toolStrip1.Invoke(new Action<string>((s) =>
                {
                    toolStripStatusLabel1.Text = s;
                }), msg);
            }
            else
            {
                toolStripStatusLabel1.Text = msg;
            }
        }
        #endregion
    }
    enum ItemOrButton
    {
        Add,
        Clear,
        Process,
        Exit
    }
}