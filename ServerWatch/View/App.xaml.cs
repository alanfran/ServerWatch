﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ServerWatch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new MainWindow();
            ServerViewModel vm = new ServerViewModel();
            window.DataContext = vm;

            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler(vm.TextBoxGotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.LostFocusEvent, new RoutedEventHandler(vm.TextBoxLostFocus));

            window.Show();
        }
    }
}
