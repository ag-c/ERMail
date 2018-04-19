﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using MailKit.Security;
using Walterlv.AssembleMailing.Mailing;
using Walterlv.AssembleMailing.Models;

namespace Walterlv.AssembleMailing.Views
{
    public sealed partial class ConfigMailBoxDialog : ContentDialog
    {
        public ConfigMailBoxDialog(MailBoxConnectionInfo connectionInfo)
        {
            InitializeComponent();
            ConnectionInfo = connectionInfo;
        }

        public MailBoxConnectionInfo ConnectionInfo
        {
            get => (MailBoxConnectionInfo) DataContext;
            set => DataContext = value;
        }

        private string ErrorTip { get; set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
        {
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs e)
        {
            var info = ConnectionInfo;
            if (!info.Validate()) return;
            var deferral = e.GetDeferral();

            var client = new IncomingMailClient(info);

            try
            {
                await client.TestConnectionAsync();
                deferral.Complete();
            }
            catch (AuthenticationException ex)
            {
                e.Cancel = true;
                ErrorTip = ex.Message;
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                ErrorTip = ex.Message;
            }
        }

        private void AddressTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var parts = ConnectionInfo.Address.Split(new[] {'@'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return;

            var mailHost = $"@{parts[1]}";

            if (OAuthDictionary.TryGetValue(mailHost, out var url))
            {
                WebView.Visibility = Visibility.Visible;
                WebView.Navigate(new Uri(url));
            }
        }

        private static readonly Dictionary<string, string> OAuthDictionary = new Dictionary<string, string>
        {
            {"@outlook.com", "https://login.microsoftonline.com/common/oauth2/v2.0/authorize" }
        };
    }

    internal class VisibilityReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility.Visible)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
