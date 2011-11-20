using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Silverlight.Testing;

namespace Twitter_WP7.Testing
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.IsVisible = false;
            var testPage = UnitTestSystem.CreateTestPage();
            IMobileTestPage imobileTPage = testPage as IMobileTestPage;
            BackKeyPress += (s, arg) =>
                {
                    bool navigateBackSuccessfull = imobileTPage.NavigateBack();
                    arg.Cancel = navigateBackSuccessfull;
                };

            (Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;
        }

    }
}