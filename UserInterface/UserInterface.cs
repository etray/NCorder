using NCorder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UserInterface
{
    public partial class NCorder : Form
    {
        ChromeDriver Browser { get; set; }
        DataPersistor Persistor { get; set; }
        ListExecutor Executor { get; set; }

        public NCorder()
        {
            InitializeComponent();
            StatusManager.SetText = this.ThreadSafeSetStatus;
            StatusManager.SetIndicator = this.ThreadSafeSetIndicator;
            this.Browser = new ChromeDriver(9222);
            this.Browser.Start();
            this.Browser.ShutdownEvent += Shutdown;
            this.Persistor = new DataPersistor();
            this.Persistor.RestoreDataMethod = this.RestoreMethod;
            this.Persistor.SaveDataMethod = this.SaveMethod;
            this.ListCache = new Dictionary<string, string>();
        }

        private void ThreadSafeSetStatus(string status)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(
                    new MethodInvoker(
                    delegate() {
                        this.statusText.Text = status;
                    
                    }));
            }
            else
            {
                this.statusText.Text = status;
            }
        }

        private void ThreadSafeSetIndicator(bool indicator)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(
                    new MethodInvoker(
                    delegate()
                    {
                        this.recordingIndicator.Checked = indicator;
                    }));
            }
            else
            {
                this.recordingIndicator.Checked = indicator;
            }
        } 

        protected override void OnLoad(EventArgs e)
        {
            this.Persistor.Restore();
            this.ClearSelections();
        }

        protected override void OnShown(EventArgs e)
        {
            //this.ClearSelections();
            this.TopMost = true;
        }

        private void Shutdown(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void addToQueue_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = (DataGridViewRow)this.queueGrid.Rows[0].Clone();
            row.Cells[0].Value = "retrieving...";
            row.Cells[1].Value = "retrieving...";
            queueGrid.Rows.Add(row);
            RetrieveRowData(row);
        }

        private void RetrieveRowData(DataGridViewRow row)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                string title = this.Browser.ScrapeTitle();
                string url = this.Browser.ScrapeUrl();

                row.Cells[0].Value = title;
                row.Cells[1].Value = url;

                this.Persistor.Save();
            }).Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            Browser.Stop();
        }

        private IDictionary<string, string> SaveMethod()
        {
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["playListModeCheckBox"] = string.Empty + this.playListModeCheckBox.Checked;
            IDictionary<string, string> queue = new Dictionary<string, string>();

            for (int i = 0; i < this.queueGrid.Rows.Count; i++)
            {
                queue[i + " " + this.queueGrid.Rows[i].Cells[0].Value] = (string)this.queueGrid.Rows[i].Cells[1].Value;
            }

            data["queueData"] = JsonConvert.SerializeObject(queue);

            return data;
        }

        private void RestoreMethod(IDictionary<string, string> data)
        {
            queueGrid.Rows.Clear();

            if (!String.IsNullOrWhiteSpace(data["playListModeCheckBox"]))
            {
                this.playListModeCheckBox.Checked = Boolean.Parse(data["playListModeCheckBox"]);
            }

            string queuedData = data["queueData"];
            if(!string.IsNullOrWhiteSpace(queuedData))
            {
                IDictionary<string, string> queue = JsonConvert.DeserializeObject<IDictionary<string, string>>(queuedData);

                foreach (var key in queue.Keys)
                {
                    string title = string.Empty;
                    Regex pattern = new Regex("^[0-9]*\\s");
                    title = pattern.Replace(key, "");
                    string url = queue[key];

                    if (!String.IsNullOrWhiteSpace(title))
                    {
                        DataGridViewRow row = (DataGridViewRow)this.queueGrid.Rows[0].Clone();
                        row.Cells[0].Value = title;
                        row.Cells[1].Value = url;
                        queueGrid.Rows.Add(row);
                    }
                }
            }
        }

        private void playListModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Persistor != null)
            {
                this.Persistor.Save();
            }
        }

        private void queueGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (this.Persistor != null)
            {
                this.Persistor.Save();
            }
        }

        private void queueGrid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (this.Persistor != null)
            {
                this.Persistor.Save();
            }
        }

        private void queueGrid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (this.Persistor != null)
            {
                this.Persistor.Save();
            }
        }

        private void queueGrid_MouseLeave(object sender, EventArgs e)
        {
            this.ClearSelections();
        }

        private void ClearSelections()
        {
            queueGrid.ClearSelection();
            queueGrid.CurrentCell = null;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            Browser.ProvideTitle = null;
            Browser.Recorder.Start();
            if(!this.playListModeCheckBox.Checked)
            {
                LoadListCache();
                Executor = new ListExecutor(Browser, RemoveItem, NextItem);
                Browser.ProvideTitle = Executor.ProvideTitle;
                Browser.SilenceDetectedEvent -= Executor.SilenceDetectedCallback;
                Browser.SilenceDetectedEvent += Executor.SilenceDetectedCallback;
                Executor.NavigateToNextUrl();
            }
        }

        // The DataGridView is increadibly slow, to the point where youtube is on the next video by the time
        // we've removed the first item and retrieved the next, so here we do some caching in hopes that speeds things up...
        IDictionary<string, string> ListCache { get; set; }

        private void LoadListCache()
        {
            this.ListCache.Clear();

            foreach (DataGridViewRow row in this.queueGrid.Rows)
            {
                string title = (string)row.Cells[0].Value;
                string url = (string)row.Cells[1].Value;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    this.ListCache[title] = (string)row.Cells[1].Value;
                }
            }
        }

        public void RemoveItem()
        {
            // Remove from cache
            ListCache.Remove(ListCache.Keys.First());

            // REmove from UI
            if (this.InvokeRequired)
            {
                this.Invoke(
                    new MethodInvoker(
                    delegate()
                    {
                        this.queueGrid.Rows.RemoveAt(0);
                    }));
            }
            else
            {
                this.queueGrid.Rows.RemoveAt(0);
            }
        }
        
        public KeyValuePair<string, string> NextItem()
        {   
            KeyValuePair<string, string>? result = null;

            if(this.ListCache.Count > 0)
            {
                result = new KeyValuePair<string, string>(this.ListCache.Keys.First(), this.ListCache.Values.First());
            }

            return result.GetValueOrDefault();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            Browser.Recorder.Stop();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            queueGrid.Rows.Clear();
        }
    }
}
