using PawnShop.Models;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CsvHelper;
using Windows.Storage;
using Syncfusion.XlsIO;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Runtime.Serialization;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PawnShop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScansPage : Windows.UI.Xaml.Controls.Page
    {
        private List<Scan> Scans;
        private List<Scan> Export;
   
        public ScansPage()
        {
            this.InitializeComponent();
            Scans = App.Scans;
            Export = new List<Scan>();
        }

        private async void btnExport_Click(object sender, RoutedEventArgs e)
        {
            /*
            StorageFile sampleFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("sample.csv", CreationCollisionOption.ReplaceExisting);
            var sw = new StreamWriter(sampleFile.Path);
            var csv = new CsvWriter(sw);
            foreach(Scan Scan in Scans)
            {
                csv.WriteRecords(Scan.Pledges);
            }
            sw.Close();
            */
            if(App.Config.Exports >= Export.Count)
            {
                await ExportExcelAsync();
                App.Config.Exports -= Export.Count;
                App.Config.Save();
            }
            else
            {
                MessageDialog msgDialog = new MessageDialog("Do you want to buy more?", "You don't have enough Scans.");

                UICommand yesCmd = new UICommand("Yes");
                msgDialog.Commands.Add(yesCmd);
                UICommand noCmd = new UICommand("No");
                msgDialog.Commands.Add(noCmd);
                IUICommand cmd = await msgDialog.ShowAsync();
                if (cmd == yesCmd)
                {
                    Frame.Navigate(typeof(StorePage));
                }
            }
            //await FileIO.WriteTextAsync(sampleFile, "Swift as a shadow");
        }

        private void lvScans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var item in e.AddedItems)
            {
                Export.Add((Scan)item);
            }
            foreach(var item in e.RemovedItems)
            {
                Export.Remove((Scan)item);
            }
        }

        private async Task ExportExcelAsync()
        {
            using (ExcelEngine excelEngine = new ExcelEngine())
            {
                //Set the default application version as Excel 2016.
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2016;

                //Create a workbook with a worksheet.
                IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);

                //Access first worksheet from the workbook instance.
                IWorksheet worksheet = workbook.Worksheets[0];

                //Insert sample text into cell “A1”.
                //worksheet.Range["A1"].Text = "Hello World";

                worksheet.Range[1, 1].Text = "Name";
                worksheet.Range[1, 2].Text = "Class";
                worksheet.Range[1, 3].Text = "Link";
                worksheet.Range[1, 4].Text = "Account";

                int i = 1;
                foreach (Scan Scan in Export)
                {
                    foreach (Pledge Pledge in Scan.Pledges)
                    {
                        worksheet.Range[i + 1, 1].Text = Pledge.Name;
                        worksheet.Range[i + 1, 2].Text = Pledge.Type;
                        worksheet.Range[i + 1, 3].Text = "=HYPERLINK(\"https://robertsspaceindustries.com/pledge/buyback/" + Pledge.ID + "\"; \"Click Me!\")";
                        worksheet.Range[i + 1, 4].Text = Pledge.Account;
                        i++;
                    }
                }

                worksheet.Columns[0].AutofitColumns();
                worksheet.Columns[1].AutofitColumns();
                worksheet.Columns[2].AutofitColumns();
                worksheet.Columns[3].AutofitColumns();

                IListObject table = worksheet.ListObjects.Create("Table", worksheet[$"A1:D{i}"]);

                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium9;


                StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync($"{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx", CreationCollisionOption.ReplaceExisting);
                /*
                if (!(ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
                    savePicker.SuggestedFileName = DateTime.Now.ToString();
                    savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                    storageFile = await savePicker.PickSaveFileAsync();
                }
                else
                {
                    StorageFolder local = ApplicationData.Current.LocalFolder;
                    storageFile = await local.CreateFileAsync($"{DateTime.Now.ToString()}.xlsx", CreationCollisionOption.ReplaceExisting);
                }
                */

                if (storageFile != null)
                {
                    //Stream sw = new Stream(storageFile.Path);
                    //Saving the workbook
                    FileStream fs = new FileStream(storageFile.Path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    workbook.SaveAs(fs);
                    workbook.Close();

                    MessageDialog msgDialog = new MessageDialog("Do you want to view the Document?", "File has been created successfully.");

                    UICommand yesCmd = new UICommand("Yes");
                    msgDialog.Commands.Add(yesCmd);
                    UICommand noCmd = new UICommand("No");
                    msgDialog.Commands.Add(noCmd);
                    IUICommand cmd = await msgDialog.ShowAsync();
                    if (cmd == yesCmd)
                    {
                        // Launch the saved file
                        bool success = await Windows.System.Launcher.LaunchFileAsync(storageFile);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            tbCredits.Text = App.Config.Exports.ToString();
        }
    }
}
