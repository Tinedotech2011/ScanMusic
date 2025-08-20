using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using ScanMusic.Core;

namespace ScanMusic.UI
{
    public partial class MainWindow : Window
    {
        private P2PManager p2p;

        public MainWindow()
        {
            InitializeComponent();
            p2p = new P2PManager();
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            var query = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Escribe un t‚rmino de b£squeda.");
                return;
            }

            ResultsList.Items.Clear();
            ResultsList.Items.Add("Buscando...");

            try
            {
                var results = await p2p.SearchAsync(query);
                ResultsList.Items.Clear();

                if (results.Count == 0)
                    ResultsList.Items.Add("No se encontraron archivos.");
                else
                    foreach (var result in results)
                        ResultsList.Items.Add(result);
            }
            catch (Exception ex)
            {
                ResultsList.Items.Clear();
                ResultsList.Items.Add("Error: " + ex.Message);
            }
        }
    }
}
