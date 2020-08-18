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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.GenericDialogs
{

    public enum FileClosedConfirmResult
    {
        Save,
        Discard,
        Cancel
    }

    public sealed partial class FileClosedConfirm : ContentDialog
    {

        public FileClosedConfirmResult Result { get; set; }

        public FileClosedConfirm()
        {
            this.InitializeComponent();
            this.Result = FileClosedConfirmResult.Cancel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = FileClosedConfirmResult.Save;
            this.Hide();
        }

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = FileClosedConfirmResult.Discard;
            this.Hide();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = FileClosedConfirmResult.Cancel;
            this.Hide();
        }
    }
}
