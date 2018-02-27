using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PawnShop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StorePage : Page
    {
        private StoreContext context = StoreContext.GetDefault();
        public StorePage()
        {
            this.InitializeComponent();
            GetAppInfo();
            GetAssociatedProducts();
        }

        private async void GetAssociatedProducts()
        {
            // Create a filtered list of the product AddOns I care about
            string[] filterList = new string[] { "Consumable", "Durable", "UnmanagedConsumable" };
            StoreProductQueryResult addOns = null;
            try
            {
                // Get list of Add Ons this app can sell, filtering for the types we know about
                addOns = await context.GetAssociatedStoreProductsAsync(filterList);
            }
            catch
            {
                var dialog = new MessageDialog("Could not load Products...");
            }
            ProductsListView.ItemsSource = Utils.CreateProductListFromQueryResult(addOns, "Add-Ons");
        }

        public async void GetAppInfo()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }
            StoreProductResult queryResult = null;
            // Get app store product details. Because this might take several moments,   
            // display a ProgressRing during the operation.
            try
            {
                queryResult = await context.GetStoreProductForCurrentAppAsync();
            }
            catch (Exception ex)
            {
                var exdialog = new MessageDialog(ex.Message);
                await exdialog.ShowAsync();
            }
            var dialog = new MessageDialog("");
            if (queryResult != null)
            {
                if (queryResult.Product == null)
                {
                    // The Store catalog returned an unexpected result.
                    dialog.Content = "Something went wrong, and the product was not returned.";

                    // Show additional error info if it is available.
                    if (queryResult.ExtendedError != null)
                    {
                        dialog.Content += $"\nExtendedError: {queryResult.ExtendedError.Message}";
                    }

                    await dialog.ShowAsync();
                    return;
                }
            }

            // Display the price of the app.
            dialog.Content = $"The price of this app is: {queryResult.Product.Price.FormattedBasePrice}";

            await dialog.ShowAsync();
        }
    }

    public static class Utils
    {
        public static ObservableCollection<ItemDetails>
            CreateProductListFromQueryResult(StoreProductQueryResult addOns, string description)
        {
            var productList = new ObservableCollection<ItemDetails>();

            if (addOns.ExtendedError != null)
            {
                ReportExtendedError(addOns.ExtendedError);
            }
            else if (addOns.Products.Count == 0)
            {
                //MainPage.Current.NotifyUser($"No configured {description} found for this Store Product.", NotifyType.ErrorMessage);
            }
            else
            {
                foreach (StoreProduct product in addOns.Products.Values)
                {
                    productList.Add(new ItemDetails(product));
                }
            }
            return productList;
        }

        static int IAP_E_UNEXPECTED = unchecked((int)0x803f6107);

        public static void ReportExtendedError(Exception extendedError)
        {
            string message;
            if (extendedError.HResult == IAP_E_UNEXPECTED)
            {
                message = "This sample has not been properly configured. See the README for instructions.";
            }
            else
            {
                // The user may be offline or there might be some other server failure.
                message = $"ExtendedError: {extendedError.Message}";
            }
            // SHow Error Message
        }
    }
    public class ItemDetails
    {
        public string Title { get; private set; }
        public string Price { get; private set; }
        public bool InCollection { get; private set; }
        public string ProductKind { get; private set; }
        public string StoreId { get; private set; }
        public string FormattedTitle => $"{Title} ({ProductKind}) {Price}, InUserCollection:{InCollection}";

        public ItemDetails(StoreProduct product)
        {
            Title = product.Title;
            Price = product.Price.FormattedPrice;
            InCollection = product.IsInUserCollection;
            ProductKind = product.ProductKind;
            StoreId = product.StoreId;
        }
    }
}
