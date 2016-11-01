using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NCorder
{
    // Launches Chrome and sends it commands asynchronously.
    public class ChromeDriver
    {
        private int _minUrlAgeForChangeDetection = 2000;
        public int MinUrlAgeForChangeDetection 
        {
            get
            {
                return this._minUrlAgeForChangeDetection;
            }

            set
            {
                this._minUrlAgeForChangeDetection = value;
            }
        }

        private int _minTitleAgeForChangeDetection = 2000;
        public int MinTitleAgeForChangeDetection
        {
            get
            {
                return this._minTitleAgeForChangeDetection;
            }

            set
            {
                this._minTitleAgeForChangeDetection = value;
            }
        }

        private int _minIntervalForSilenceDetection = 2000;
        public int MinIntervalForSilenceDetection
        {
            get
            {
                return this._minIntervalForSilenceDetection;
            }

            set
            {
                this._minIntervalForSilenceDetection = value;
            }
        }

        private int Port { get; set; }
        private Process ChromeProcess { get; set; }
        private ChromeComm ChromeJson { get; set; }
        private bool BrowserRunning { get; set; }
        private Thread BrowserThread { get; set; }
        private Thread UrlChangeThread { get; set; }
        private Thread TitleChangeThread { get; set; }
        private Thread SilenceDetectionThread { get; set; }

        public Recorder Recorder { get; set; }


        public string Url { get; set; }
        public string Title { get; set; }

        public EventHandler ShutdownEvent { get; set; }
        public EventHandler UrlChangeEvent { get; set; }
        public EventHandler TitleChangeEvent { get; set; }
        public EventHandler SilenceDetectedEvent { get; set; }
        public EventHandler SoundDetectedEvent { get; set; }
        public Func<string> ProvideTitle { get; set; }

        public ChromeDriver(int port)
        {
            this.Port = port;
            this.Recorder = new Recorder();
            this.SilenceDetectedEvent += SaveTrack;
            this.ChromeJson = new ChromeComm(port);
        }

        public void Start()
        {
            if (this.BrowserRunning)
            {
                this.Stop();
            }

            this.BrowserRunning = true;
            this.BrowserThread = new Thread(new ThreadStart(this.Run));
            this.BrowserThread.Start();

            this.UrlChangeThread = new Thread(new ThreadStart(this.UrlChangeDetector));
            this.UrlChangeThread.Start();

            this.TitleChangeThread = new Thread(new ThreadStart(this.TitleChangeDetector));
            this.TitleChangeThread.Start();

            this.SilenceDetectionThread = new Thread(new ThreadStart(this.SilenceDetector));
            this.SilenceDetectionThread.Start();
        }

        private void UrlChangeDetector()
        {
            this.Url = string.Empty;
            DateTime startTime = DateTime.Now;
            string pendingValue = string.Empty;

            while (this.BrowserRunning)
            {
                string currentUrl = this.ChromeJson.GetValue("url");
                if (this.Url != currentUrl)
                {
                    if (pendingValue == currentUrl)
                    {
                        if (DateTime.Now.Subtract(startTime).Ticks / TimeSpan.TicksPerMillisecond >= this.MinUrlAgeForChangeDetection)
                        {
                            this.Url = pendingValue;
                            StatusManager.Status("Url Changed: " + this.Url);

                            // trigger event            
                            if (UrlChangeEvent != null)
                            {
                                this.UrlChangeEvent(this, null);
                            }
                        }
                    }
                    else
                    {
                        startTime = DateTime.Now;
                        pendingValue = currentUrl;
                    }

                }

                Thread.Sleep(120);
            }
        }

        private void TitleChangeDetector()
        {
            this.Title = string.Empty;
            DateTime startTime = DateTime.Now;
            string pendingValue = string.Empty;

            while (this.BrowserRunning)
            {
                string currentTitle = this.ChromeJson.GetValue("title");
                if (this.Title != currentTitle)
                {
                    if (pendingValue == currentTitle)
                    {
                        if (DateTime.Now.Subtract(startTime).Ticks / TimeSpan.TicksPerMillisecond >= this.MinTitleAgeForChangeDetection)
                        {
                            this.Title = pendingValue;
                            StatusManager.Status("Title Changed: " + this.Title);

                            // trigger event            
                            if (TitleChangeEvent != null)
                            {
                                this.TitleChangeEvent(this, null);
                            }
                        }
                    }
                    else
                    {
                        startTime = DateTime.Now;
                        pendingValue = currentTitle;
                    }

                }

                Thread.Sleep(120);
            }
        }

        private void SilenceDetector()
        {
            DateTime silenceStart = DateTime.Now;
            bool previouslySilent = true;

            while (this.BrowserRunning)
            {
                if (!this.Recorder.SilenceDetected)
                {
                    if (previouslySilent)
                    {
                        StatusManager.Status("Sound detected.");
                        previouslySilent = false;
                        if (this.SoundDetectedEvent != null)
                        {
                            this.SoundDetectedEvent(this, null);
                        }
                    }

                    silenceStart = DateTime.Now;
                }
                else
                {
                    if (!previouslySilent)
                    {
                        if (DateTime.Now.Subtract(silenceStart).Ticks / TimeSpan.TicksPerMillisecond >= this.MinIntervalForSilenceDetection)
                        {
                            StatusManager.Status("Silence Detected");
                            previouslySilent = true;
                            // trigger event            
                            if (this.SilenceDetectedEvent != null)
                            {
                                this.SilenceDetectedEvent(this, null);
                            }
                        }
                    }
                }

                Thread.Sleep(20);
            }
        }

        public void SaveTrack(object sender, EventArgs e)
        {
            string title = this.Title;

            // If in list mode, use the title from the list, otherwise go with the one scraped from Chrome.
            if (this.ProvideTitle != null)
            {
                title = this.ProvideTitle();
            }

            long finishingTrack = this.Recorder.CurrentTrack;
            this.Recorder.CurrentTrack = Recorder.Sequence;
            this.Recorder.SaveTrack(finishingTrack, this.CleanUpTitle(title));
        }

        private void Run()
        {
            this.ChromeProcess = LaunchChrome(this.Port);
            this.ChromeProcess.WaitForInputIdle();

            // Wait for Chrome so we can get its geometry to reposition ourselves...
            for (int i = 0; i < 500; i++)
            {
                if (!this.ChromeProcess.HasExited && this.ChromeProcess.MainWindowHandle == (IntPtr)0)
                {
                    Thread.Sleep(20);
                }
            }

            MoveSelfToForeground();

            // Begin processing loop
            while (this.BrowserRunning)
            {
                Thread.Sleep(120);

                if (this.ChromeProcess.HasExited)
                {
                    break;
                }
            }

            try
            {
                this.ChromeProcess.CloseMainWindow();
            }
            catch
            {
                // user could have closed window.
            }
            
            if (this.ShutdownEvent != null)
            {
                this.ShutdownEvent(this, null);
            }
        }

        // PInvoke platform libraries to set window position
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern long GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        public struct RECT {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        };

        private void MoveSelfToForeground()
        {
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            RECT chromeRect = new RECT();
            GetWindowRect(this.ChromeProcess.MainWindowHandle, ref chromeRect);
            RECT nCorderRect = new RECT();
            GetWindowRect(handle, ref nCorderRect);

            int newX = 20;
            int newY = chromeRect.bottom - (nCorderRect.bottom - nCorderRect.top) - 20;
            SetWindowPos(handle, new IntPtr(-1), newX, newY, 0, 0, (0x0001));
        }

        public void Stop()
        {
            this.BrowserRunning = false;

            // wait for threads to die.
            if (this.UrlChangeThread != null && this.UrlChangeThread.IsAlive)
            {
                try
                {
                    this.UrlChangeThread.Join();
                }
                catch
                {
                }
            }

            if (this.TitleChangeThread != null && this.TitleChangeThread.IsAlive)
            {
                try
                {
                    this.TitleChangeThread.Join();
                }
                catch
                {
                }
            }

            if (this.SilenceDetectionThread != null && this.SilenceDetectionThread.IsAlive)
            {
                try
                {
                    this.SilenceDetectionThread.Join();
                }
                catch
                {
                }
            }

            if (this.BrowserThread != null && this.BrowserThread.IsAlive)
            {
                try
                {
                    this.BrowserThread.Join();
                }
                catch
                {
                }
            }
        }

        public void NavigateToUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                ChromeJson.NavigateToUrl(url);
            }
        }

        public string ScrapeTitle()
        {
            string result = string.Empty;
            result = this.ChromeJson.GetValue("title");
            return this.CleanUpTitle(result);
        }

        private string CleanUpTitle(string title)
        {
            string result = title.Replace(" - YouTube", string.Empty);

            Regex pattern = new Regex("&[^\\s]*;");
            result = pattern.Replace(result, "");
            pattern = new Regex("[^a-zA-Z0-9\\s]");
            result = pattern.Replace(result, "");
            return result;
        }

        private string CleanUpURL(string url)
        {
            string result = url;
            // Remove playlist index, these prevent silence between tracks.
            Regex pattern = new Regex("&index=[0-9]*");
            result = pattern.Replace(result, "");
            pattern = new Regex("&list=[A-Za-z\\-]*");
            result = pattern.Replace(result, "");
            return result;
        }

        public string ScrapeUrl()
        {
            string result = string.Empty;
            result = this.ChromeJson.GetValue("url");
            return this.CleanUpURL(result);
        }

        // Launch chrome in remote debug mode so we can send commands.
        // Specify user directory so we have installed Add-Ons accessible.
        private Process LaunchChrome(int port)
        {
            string chromePath = this.GetChromePath();
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string path = localAppDataPath + @"\Google\Chrome\User Data";
            
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            var remoteDebuggingArg = "--remote-debugging-port=" + port;
            var userDirectoryArg = "--user-data-dir=\"" + directoryInfo.FullName + "\"";
            var chromeProcessArgs = remoteDebuggingArg + " " + userDirectoryArg + " --bwsi --no-first-run --incognito";
            var processStartInfo = new ProcessStartInfo(chromePath, chromeProcessArgs);
            processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;

            // close any existing chrome windows so we don't get confused.
            this.CloseAllChromes();

            var chromeProcess = Process.Start(processStartInfo);

            return chromeProcess;
        }

        private void CloseAllChromes()
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.ProcessName.ToLower().Contains("chrome"))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // might have been closed by user
                    }
                }
            }
        }

        // Attempt to get path from registry. Fall-back on well-known path. Fail if Chrome not found.
        public string GetChromePath()
        {
            string result = "";

            result = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe", "Default", @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"); 
            
            if(!File.Exists(result))
            {
                throw new FileNotFoundException("Chrome is required.");
            }

            return result;
        }
        
        public bool WaitForUrl(string url)
        {
            bool result = false;
            string currentUrl = string.Empty;

            for (int i = 0; i < 500; i++ )
            {
                Thread.Sleep(20);
                currentUrl = Regex.Replace(this.ChromeJson.GetValue("url"), @"/$", "");
                if (currentUrl == url)
                {
                    break;
                }
            }

            if (currentUrl == url)
            {
                result = true;
            }

            return result;
        }

        public bool WaitForTitle(string title)
        {
            bool result = false;
            string currentTitle = string.Empty;

            for (int i = 0; i < 500; i++)
            {
                Thread.Sleep(20);
                currentTitle = this.ChromeJson.GetValue("title");
                if (currentTitle == title)
                {
                    break;
                }
            }

            if (currentTitle == title)
            {
                result = true;
            }

            return result;
        }

        public bool WaitForValueToChange(string name, string currentValue, int timeout)
        {
            bool result = false;
            string newValue = string.Empty;
            int msIncrement = 100;
            int msTimeOut = 2000;
            
            if (timeout > msTimeOut)
            {
                msTimeOut = timeout;
            }

            for (int i = 0; i < (msTimeOut / msIncrement); i++ )
            {
                Thread.Sleep(20);
                newValue = this.ChromeJson.GetValue(name);
                if (newValue != currentValue)
                {
                    break;
                }
            }

            if (newValue != currentValue)
            {
                result = true;
            }

            return result;
        }
    }
}
