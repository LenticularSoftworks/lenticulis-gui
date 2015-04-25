﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for LayerSelectWindow.xaml
    /// </summary>
    public partial class LayerSelectWindow : MetroWindow
    {
        public int selectedLayer = -1;

        public LayerSelectWindow(List<String> layers)
        {
            InitializeComponent();

            ListBoxItem lbi;

            LayerListBox.Items.Clear();

            lbi = new ListBoxItem();
            lbi.Content = "<všechny sloučené>";
            LayerListBox.Items.Add(lbi);

            foreach (String layer in layers)
            {
                lbi = new ListBoxItem();
                lbi.Content = layer;
                LayerListBox.Items.Add(lbi);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            selectedLayer = LayerListBox.SelectedIndex;
            Close();
        }
    }
}
