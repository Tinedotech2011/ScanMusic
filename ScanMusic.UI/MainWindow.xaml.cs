using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ScanMusic.Core;

namespace ScanMusic.UI
{
    public partial class MainWindow : Window
    {
        private readonly MediaPlayer mediaPlayer = new MediaPlayer();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly DispatcherTimer bannerTimer = new DispatcherTimer();

        private List<string> playlist = new List<string>();
        private int currentTrackIndex = -1;
        private string currentFolder = "";
        private P2PManager p2p;
        private string SharedFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScanMusicShared");

        private double bannerPosition = 0;
        private double bannerTextWidth = 0;

        public MainWindow()
        {
            InitializeComponent();
            p2p = new P2PManager();
            Loaded += OnWindowLoaded;

            // Configurar MediaPlayer
            mediaPlayer.MediaOpened += OnMediaOpened;
            mediaPlayer.MediaEnded += OnMediaEnded;

            // Temporizador de progreso
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += OnTimerTick;

            // Temporizador del banner
            bannerTimer.Interval = TimeSpan.FromMilliseconds(30); // Velocidad moderada
            bannerTimer.Tick += OnBannerTick;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            SetupSharedFolder();
            LoadLocalFiles();
            LoadPlaylist();

            // Iniciar banner con mensaje inicial
            UpdateBannerArea();
        }

        private void SetupSharedFolder()
        {
            var testFolder = @"G:\ScanMusic\SharedMusic";
            if (Directory.Exists(testFolder))
            {
                currentFolder = testFolder;
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
                var folders = new[] { currentFolder, SharedFolder };
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

                playlist = files.Select(f => Path.Combine(currentFolder, f)).ToList();
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
                currentFolder = dialog.FolderName;
                ResultsList.Items.Clear();
                ResultsList.Items.Add($"📁 Carpeta seleccionada: {currentFolder}");
                LoadFilesFromFolder(currentFolder);
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

                playlist = files.Select(f => Path.Combine(folderPath, f)).ToList();
            }
            catch (Exception ex)
            {
                ResultsList.Items.Add($"Error al leer la carpeta: {ex.Message}");
            }
        }

        private void OnFileDoubleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsList.SelectedItem is string selectedText)
            {
                string fileName = selectedText.Trim();

                if (fileName.StartsWith("• ") || fileName.StartsWith("→ "))
                {
                    fileName = fileName.Substring(2).Trim();
                }

                int parenIndex = fileName.IndexOf('(');
                if (parenIndex > 0)
                {
                    fileName = fileName.Substring(0, parenIndex).Trim();
                }

                string[] searchFolders = { currentFolder, @"G:\ScanMusic\SharedMusic", SharedFolder };

                foreach (var folder in searchFolders)
                {
                    if (string.IsNullOrEmpty(folder)) continue;
                    var fullPath = Path.Combine(folder, fileName);
                    if (File.Exists(fullPath))
                    {
                        PlayFile(fullPath);
                        return;
                    }
                }

                MessageBox.Show($"No se encontró el archivo: {fileName}", "Archivo no encontrado");
            }
        }

        private void PlayFile(string filePath)
        {
            StopPlayback();

            if (playlist.Contains(filePath))
            {
                currentTrackIndex = playlist.IndexOf(filePath);
            }
            else
            {
                playlist.Add(filePath);
                currentTrackIndex = playlist.Count - 1;
            }

            mediaPlayer.Open(new Uri(filePath));
            StatusText.Text = $"Reproduciendo: {Path.GetFileName(filePath)}";
            BannerText.Text = $"🎵 Reproduciendo: {Path.GetFileName(filePath)}";
            UpdateBannerArea();
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            var duration = mediaPlayer.NaturalDuration.HasTimeSpan ? mediaPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero;
            TotalTime.Text = FormatTime(duration);
            ProgressSlider.Maximum = duration.TotalSeconds;
            timer.Start();
            mediaPlayer.Play();
            StatusText.Text = $"Reproduciendo: {Path.GetFileName(mediaPlayer.Source.LocalPath)}";
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var position = mediaPlayer.Position;
                CurrentTime.Text = FormatTime(position);
                ProgressSlider.Value = position.TotalSeconds;
            }
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            OnNextClick(this, new RoutedEventArgs());
        }

        private void OnPlayPauseClick(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source == null)
            {
                if (playlist.Count > 0)
                {
                    PlayFile(playlist[0]);
                }
                return;
            }

            if (mediaPlayer.Position.TotalSeconds > 0 && mediaPlayer.Position < mediaPlayer.NaturalDuration.TimeSpan)
            {
                if (mediaPlayer.Pause != null)
                {
                    mediaPlayer.Pause();
                    StatusText.Text = "Estado: Pausado";
                }
                else
                {
                    mediaPlayer.Play();
                    StatusText.Text = $"Reproduciendo: {Path.GetFileName(mediaPlayer.Source.LocalPath)}";
                }
            }
            else
            {
                mediaPlayer.Play();
                StatusText.Text = $"Reproduciendo: {Path.GetFileName(mediaPlayer.Source.LocalPath)}";
            }
        }

        private void OnStopClick(object sender, RoutedEventArgs e)
        {
            StopPlayback();
            StatusText.Text = "Estado: Detenido";
            BannerText.Text = "🎵 Escucha música con ScanMusic 1.0";
            UpdateBannerArea();
        }

        private void StopPlayback()
        {
            timer.Stop();
            mediaPlayer.Stop();
            CurrentTime.Text = "00:00";
            ProgressSlider.Value = 0;
        }

        private void OnPreviousClick(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;

            if (mediaPlayer.Position > TimeSpan.FromSeconds(3))
            {
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Play();
                StatusText.Text = $"Reproduciendo: {Path.GetFileName(mediaPlayer.Source.LocalPath)}";
            }
            else
            {
                currentTrackIndex = (currentTrackIndex - 1 + playlist.Count) % playlist.Count;
                PlayFile(playlist[currentTrackIndex]);
            }
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
            PlayFile(playlist[currentTrackIndex]);
        }

        private void OnVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = VolumeSlider.Value;
        }

        private string FormatTime(TimeSpan ts)
        {
            return $"{ts:mm\\:ss}";
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
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
                var results = p2p.SearchAsync(query).Result;
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
                ResultsList.Items.Add("Error: " + ex.InnerException?.Message ?? ex.Message);
            }
        }

        private void OnSearchBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        private void UpdateBannerArea()
        {
            var formattedText = new System.Windows.Media.FormattedText(
                BannerText.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface(BannerText.FontFamily, BannerText.FontStyle, BannerText.FontWeight, BannerText.FontStretch),
                BannerText.FontSize,
                System.Windows.Media.Brushes.White);

            bannerTextWidth = formattedText.Width;

            bannerPosition = BannerScroll.ActualWidth;
            bannerTimer.Start();
        }

        private void OnBannerTick(object sender, EventArgs e)
        {
            bannerPosition -= 2.5; // Velocidad suave

            if (bannerPosition + bannerTextWidth < 0)
            {
                bannerPosition = BannerScroll.ActualWidth;
            }

            BannerScroll.ScrollToHorizontalOffset(-bannerPosition);
        }

        private void SavePlaylist()
        {
            try
            {
                File.WriteAllLines("playlist.txt", playlist);
            }
            catch { }
        }

        private void LoadPlaylist()
        {
            try
            {
                if (File.Exists("playlist.txt"))
                {
                    playlist = File.ReadAllLines("playlist.txt").Where(File.Exists).ToList();
                }
            }
            catch { }
        }
    }
}