// 1. Primero: todas las directivas 'using'
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ScanMusic.Core;

// 2. Después: el 'namespace'
namespace ScanMusic.UI
{
    public partial class MainWindow : Window
    {
        private P2PManager p2p;
        private string SharedFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScanMusicShared");

        public MainWindow()
        {
            InitializeComponent();
            p2p = new P2PManager();
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            SetupSharedFolder();
            LoadLocalFiles();
        }

        private void SetupSharedFolder()
        {
            var testFolder = @"G:\ScanMusic\SharedMusic";
            if (Directory.Exists(testFolder))
            {
                // Usar carpeta de prueba
            }
            else if (!Directory.Exists(SharedFolder))
            {
                Directory.CreateDirectory(SharedFolder);
                File.WriteAllText(Path.Combine(SharedFolder, "Ejemplo - Haz doble clic.mp3"), "");
            }
        }

        private void LoadLocalFiles()
        {
            try
            {
                var files = new List<string>();
                var folders = new[] { @"G:\ScanMusic\SharedMusic", SharedFolder };
                var extensions = new[] { ".mp3", ".wav" };

                foreach (var folder in folders)
                {
                    if (Directory.Exists(folder))
                    {
                        foreach (var ext in extensions)
                        {
                            files.AddRange(Directory.GetFiles(folder, $"*{ext}", SearchOption.TopDirectoryOnly)
                                .Select(Path.GetFileName));
                        }
                    }
                }

                if (files.Count == 0)
                {
                    ResultsList.Items.Add("(Tu carpeta de música está vacía)");
                }
                else
                {
                    ResultsList.Items.Add($"🎵 Archivos locales ({files.Count}):");
                    foreach (var file in files)
                    {
                        ResultsList.Items.Add($" • {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                ResultsList.Items.Add($"Error al leer archivos: {ex.Message}");
            }
        }

       private void OnSelectFolderClick(object sender, RoutedEventArgs e)
{
    var dialog = new Microsoft.Win32.OpenFolderDialog();
    bool? result = dialog.ShowDialog();

    if (result == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
    {
        var selectedFolder = dialog.FolderName;

        ResultsList.Items.Clear();
        ResultsList.Items.Add($"📁 Carpeta seleccionada: {selectedFolder}");
        LoadFilesFromFolder(selectedFolder);
    }
}

        private void LoadFilesFromFolder(string folderPath)
        {
            try
            {
                var files = new List<string>();
                var extensions = new[] { ".mp3", ".wav" };

                if (Directory.Exists(folderPath))
                {
                    foreach (var ext in extensions)
                    {
                        files.AddRange(Directory.GetFiles(folderPath, $"*{ext}", SearchOption.TopDirectoryOnly)
                            .Select(Path.GetFileName));
                    }
                }

                if (files.Count == 0)
                {
                    ResultsList.Items.Add("No se encontraron archivos .mp3 o .wav en esta carpeta.");
                }
                else
                {
                    ResultsList.Items.Add($"🎵 Archivos encontrados ({files.Count}):");
                    foreach (var file in files)
                    {
                        ResultsList.Items.Add($" • {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                ResultsList.Items.Add($"Error al leer la carpeta: {ex.Message}");
            }
        }

        private async void OnSearchClick(object sender, RoutedEventArgs e)
        {
            var query = SearchBox.Text?.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Escribe un término de búsqueda.");
                return;
            }

            ResultsList.Items.Clear();
            ResultsList.Items.Add("Buscando en la red...");

            try
            {
                var results = await p2p.SearchAsync(query);
                ResultsList.Items.Clear();

                if (results.Count == 0)
                {
                    ResultsList.Items.Add("No se encontraron archivos.");
                }
                else
                {
                    ResultsList.Items.Add($"🔍 Resultados ({results.Count}):");
                    foreach (var result in results)
                    {
                        ResultsList.Items.Add($" → {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                ResultsList.Items.Clear();
                ResultsList.Items.Add("Error: " + ex.Message);
            }
        }

        private void OnSearchBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }
    }
}