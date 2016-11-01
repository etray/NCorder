using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder
{
    public static class StatusManager
    {
        private static bool EnableLogging = false;
        private static object lockObject = new object();

        public static string LogFile 
        { 
            get
            {
                return Path.Combine(Path.GetTempPath(), "ncorder.log");
            }
        }

        public static Action<string> SetText { get; set; }

        public static Action<bool> SetIndicator { get; set; }

        public static void Status(string status)
        {
            if (SetText != null)
            {
                SetText(status);
            }

            if (EnableLogging)
            {
                lock (lockObject)
                {
                    using (FileStream file = new FileStream(LogFile, FileMode.Append, FileAccess.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(file))
                        {
                            streamWriter.WriteLine(DateTime.Now.ToShortTimeString() + " " + status);
                        }

                        file.Close();
                    }
                }
            }
        }

        public static void Indicator(bool onOrOff)
        {
            if (SetIndicator != null)
            {
                SetIndicator(onOrOff);
            }
        }

        public static void Idle()
        {
            Status("Idle.");
        }

        public static void Clear()
        {
            Status(string.Empty);
        }
    }
}
