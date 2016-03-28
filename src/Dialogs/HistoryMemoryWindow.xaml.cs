using lenticulis_gui.src.App;
using MahApps.Metro.Controls;
using System;
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

namespace lenticulis_gui.src.Dialogs
{
    /// <summary>
    /// Interaction logic for HistoryMemoryWindow.xaml
    /// </summary>
    public partial class HistoryMemoryWindow : MetroWindow
    {
        public HistoryMemoryWindow()
        {
            InitializeComponent();

            MemoryTextBox.Value = HistoryList.HistoryListSize;
        }

        /// <summary>
        /// Parse and store input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //numeric updown control - parsing isnt necessary
            HistoryList.HistoryListSize = (int)MemoryTextBox.Value;

            this.Close();
        }
    }
}
