﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HolyLogger.OptionsUserControls
{
    /// <summary>
    /// Interaction logic for QRZServiceControl.xaml
    /// </summary>
    public partial class SatelliteControl : UserControl
    {
        public bool HasChanged{ get; set; }

        public SatelliteControl()
        {
            InitializeComponent();
            HasChanged = false;
        }
        
        private void HasChanged_Click(object sender, RoutedEventArgs e)
        {
            HasChanged = true;
        }

    }
}
