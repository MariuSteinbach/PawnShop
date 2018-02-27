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
            if (await isLoggedIn() &&
                wvScanner.Source.ToString() == "https://robertsspaceindustries.com/account/buy-back-pledges?page=1&pagesize=100")
            {
                App.Scans.Add(new Scan());
            }
            if(await isLoggedIn() &&
                wvScanner.Source.ToString().Split('=')[0] == "https://robertsspaceindustries.com/account/buy-back-pledges?page")
            {
                await ScanDocument();
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

            if (Document.DocumentNode.OuterHtml.Contains("/account/buy-back-pledges?page=" + (Convert.ToInt32(wvScanner.Source.ToString().Split('=')[1].Split('&')[0].ToString()) + 1) + "&amp;pagesize=100"))
            {
                wvScanner.Source = new Uri("https://robertsspaceindustries.com/account/buy-back-pledges?page=" + (Convert.ToInt32(wvScanner.Source.ToString().Split('=')[1].Split('&')[0].ToString()) + 1) + "&pagesize=100");
            }
            else
            {
                await SaveDocument();
                var dialog = new MessageDialog($"Found {App.Scans.Last().Pledges.Count} Pledges on Account {App.Scans.Last().Account}");

                await dialog.ShowAsync();
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

    public class Crypter
    {
        public static byte[] Key = new byte[]{0x76, 0x36, 0x6f, 0x3d, 0x5a, 0x43, 0x17,
                                      0x94, 0x26, 0x4f, 0x44, 0x84, 0x41, 0x9a,
                                      0x34, 0x1f, 0x4a, 0x2c, 0x45, 0x7a, 0x5f,
                                      0x84, 0xff, 0x9a};
        public static byte[] IV = new byte[]{0x65, 0x8f, 0x4f, 0x3d, 0x52, 0x4a, 0x87,
                                      0x94, 0xf6, 0x4f, 0xe4, 0x84, 0x46, 0x92,
                                      0xf2, 0xaf};
        public static byte[] Decrypt(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        public static byte[] Encrypt(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }


    }
}
