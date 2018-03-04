using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        private HashSet<Guid> consumedTransactionIds = new HashSet<Guid>();
        private StoreContext context = null;
        public StorePage()
        {
            this.InitializeComponent();
            GetAssociatedProducts();
        }

        private async void GetAssociatedProducts()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }
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
            ProductsListView.ItemsSource = await Utils.CreateProductListFromQueryResult(addOns, "Add-Ons");
        }
      
        private async void ProductsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemDetails Item = (ItemDetails)e.ClickedItem;
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }
            
            StorePurchaseResult result = await context.RequestPurchaseAsync(Item.StoreId);

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            MessageDialog dialog = new MessageDialog("");

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    dialog.Content = "The user has already purchased the product.";
                    break;

                case StorePurchaseStatus.Succeeded:
                    dialog.Content = "The purchase was successful.";
                    break;

                case StorePurchaseStatus.NotPurchased:
                    dialog.Content = "The purchase did not complete. " +
                        "The user may have cancelled the purchase. ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.NetworkError:
                    dialog.Content = "The purchase was unsuccessful due to a network error. " +
                        "ExtendedError: " + extendedError;
                    break;

                case StorePurchaseStatus.ServerError:
                    dialog.Content = "The purchase was unsuccessful due to a server error. " +
                        "ExtendedError: " + extendedError;
                    break;

                default:
                    dialog.Content = "The purchase was unsuccessful due to an unknown error. " +
                        "ExtendedError: " + extendedError;
                    break;
            }
            await dialog.ShowAsync();
        }

        private bool IsLocallyFulfilled(Guid transactionId)
        {
            return consumedTransactionIds.Contains(transactionId);
        }
    }


    public static class Utils
    {
        public async static Task<ObservableCollection<ItemDetails>>
            CreateProductListFromQueryResult(StoreProductQueryResult addOns, string description)
        {
            var productList = new ObservableCollection<ItemDetails>();

            if (addOns.ExtendedError != null)
            {
                ReportExtendedError(addOns.ExtendedError);
            }
            else if (addOns.Products.Count == 0)
            {
                MessageDialog dialog = new MessageDialog("No Add-Ons found for this App...");
                await dialog.ShowAsync();
            }
            else
            {
                foreach (StoreProduct product in addOns.Products.Values)
                {
                    if (product.StoreId == "9PPLX2HDLV2L")
                    {
                        productList.Add(new ItemDetails(product));
                    }
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
        public string StoreId { get; set; }
        public string FormattedTitle => $"{Title} {Price}";

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
