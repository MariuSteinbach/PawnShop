using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PawnShop.Models;
using HtmlAgilityPack;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.Serialization;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PawnShop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScanPage : Page
    {
        public ScanPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            if (tswOnline.IsOn && await isLoggedIn())
            {
                wvScanner.Source = new Uri("https://robertsspaceindustries.com/account/buy-back-pledges?page=1&pagesize=100");
            }
            else if(!tswOnline.IsOn)
            {
                HtmlDocument Document = new HtmlDocument();
                Document.LoadHtml(tbScanner.Text);
                Scan newScan = new Scan();

                var dialog = new MessageDialog($"Found {newScan.AddPledges(Document).ToString()} Pledges on Account {newScan.Account}");

                try
                {
                    App.Scans.Find(a => a.Account == newScan.Account).Pledges.AddRange(newScan.Pledges);
                }
                catch
                {
                    App.Scans.Add(newScan);
                }

                await dialog.ShowAsync();
            }
        }

        private async void wvScanner_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if(await isLoggedIn() &&
                wvScanner.Source.ToString().Split('=')[0] == "https://robertsspaceindustries.com/account/buy-back-pledges?page")
            {
                await ScanDocument();
            }
            else if (await isLoggedIn())
            {
                App.Scans.Add(new Scan());
                wvScanner.Visibility = Visibility.Collapsed;
                tbScanner.Visibility = Visibility.Collapsed;
                prLoader.IsActive = true;
                wvScanner.Source = new Uri("https://robertsspaceindustries.com/account/buy-back-pledges?page=1&pagesize=100");
            }
        }

        private async Task<bool> isLoggedIn()
        {
            HtmlDocument Document = new HtmlDocument();
            string Html = await wvScanner.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            Document.LoadHtml(Html);
            Pledge temp = new Pledge();
            if(!string.IsNullOrEmpty(temp.getPledgeAccount(Document)))
            {
                return true;
            }
            return false;
        }

        private async Task ScanDocument()
        {
            HtmlDocument Document = new HtmlDocument();
            string Html = await wvScanner.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            Document.LoadHtml(Html);

            App.Scans.Last().AddPledges(Document);

            tbCounter.Text = $"Found {App.Scans.Last().Pledges.Count} Pledges on {App.Scans.Last().Account}";

            if (Document.DocumentNode.OuterHtml.Contains("/account/buy-back-pledges?page=" + (Convert.ToInt32(wvScanner.Source.ToString().Split('=')[1].Split('&')[0].ToString()) + 1) + "&amp;pagesize=100"))
            {
                wvScanner.Source = new Uri("https://robertsspaceindustries.com/account/buy-back-pledges?page=" + (Convert.ToInt32(wvScanner.Source.ToString().Split('=')[1].Split('&')[0].ToString()) + 1) + "&pagesize=100");
            }
            else
            {
                await SaveDocument();
                await wvScanner.InvokeScriptAsync("eval", new string[] { "document.getElementsByClassName('c-account-sidebar__profile-info-signout js-sign-out')[0].click();" });
                MessageDialog msgDialog = new MessageDialog("Scan another Account?", $"{App.Scans.Last().Pledges.Count} Pledges found on {App.Scans.Last().Account}");
                wvScanner.Visibility = Visibility.Visible;
                tbScanner.Visibility = Visibility.Visible;
                prLoader.IsActive = false;
                UICommand yesCmd = new UICommand("Yes");
                msgDialog.Commands.Add(yesCmd);
                UICommand noCmd = new UICommand("No");
                msgDialog.Commands.Add(noCmd);
                IUICommand cmd = await msgDialog.ShowAsync();
                if (cmd == noCmd)
                {
                    Frame.Navigate(typeof(ScansPage));
                }
            }
        }

        private async Task SaveDocument()
        {
            StorageFile storageFile = null;
            try
            {
                //storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{App.Scans.Last().Account.ToString()}.{App.Scans.Last().Date.ToString()}.scan", CreationCollisionOption.ReplaceExisting);
                storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{App.Scans.Last().Account}.{App.Scans.Last().Date.ToString("yyyyMMddHHmmss")}.scan", CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog("Failed to save scan. Error: " + e.Message);

                await dialog.ShowAsync();
                throw;
            }
            if (storageFile != null)
            {
                //Stream sw = new Stream(storageFile.Path);
                //Saving the workbook
                FileStream fs = new FileStream(storageFile.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    bf.Serialize(ms, App.Scans.Last());
                    ms.Seek(0, SeekOrigin.Begin);
                    Byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)ms.Length);
                    Byte[] encrypted = Crypter.Encrypt(bytes);
                    await fs.WriteAsync(encrypted, 0, encrypted.Count());
                }
                catch (SerializationException e)
                {
                    var dialog = new MessageDialog("Failed to save scan. Error: " + e.Message);

                    await dialog.ShowAsync();
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
    }
}
