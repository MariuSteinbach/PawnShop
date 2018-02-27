using PawnShop.Models;
using PawnShop.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PawnShop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void nv_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if(args.IsSettingsInvoked)
            {
                frmContent.Navigate(typeof(SettingsPage));
            }
            else
            {
                // load invoked item
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(i => (string)i.Content == (string)args.InvokedItem);
                nv_Navigate(item as NavigationViewItem);
            }
        }

        private void nv_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                frmContent.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationViewItem item = args.SelectedItem as NavigationViewItem;
                nv_Navigate(item);
            }
        }

        private void nv_Loaded(object sender, RoutedEventArgs e)
        {
            // Load HomePage initial
            foreach(NavigationViewItemBase item in nv.MenuItems)
            {
                if(item is NavigationViewItem && item.Tag.ToString() == "home")
                {
                    nv.SelectedItem = item;
                    break;
                }
            }
        }

        private void nv_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    frmContent.Navigate(typeof(HomePage));
                    break;
                case "scan":
                    frmContent.Navigate(typeof(ScanPage));
                    break;
                case "scans":
                    frmContent.Navigate(typeof(ScansPage));
                    break;
                case "store":
                    frmContent.Navigate(typeof(StorePage));
                    break;
            }
        }

        private void hbtnMoreInfo_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
