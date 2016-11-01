using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder
{
    public class ListExecutor
    {
        ChromeDriver Driver { get; set; }
        
        public Func<KeyValuePair<string,string>> NextItem { get; set; }
        public Action RemoveItem { get; set; }

        public ListExecutor(ChromeDriver Driver, Action RemoveItem, Func<KeyValuePair<string, string>> NextItem)
        {
            this.NextItem = NextItem;
            this.Driver = Driver;
            this.Driver.ProvideTitle = this.ProvideTitle;
            this.RemoveItem = RemoveItem;
        }

        public void NavigateToNextUrl()
        {
            KeyValuePair<string,string>? item = this.NextItem();
            string url = item.GetValueOrDefault().Value;
            if (item != null && !string.IsNullOrWhiteSpace(url))
            {
                StatusManager.Status("Navigating to: " + url);
                this.Driver.NavigateToUrl(url);
            }
            else
            {
                StatusManager.Status("Navigating to: about:blank");
                this.Driver.NavigateToUrl("about:blank");
                this.Driver.Recorder.Stop();
            }
        }

        public string ProvideTitle()
        {
            KeyValuePair<string, string>? item = this.NextItem();
            return item.GetValueOrDefault().Key;
        }

        public void SilenceDetectedCallback(object sender, EventArgs e)
        {
            this.RemoveItem();
            this.NavigateToNextUrl();
        }
    }
}
