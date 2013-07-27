using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PWTranslator.Controllers;
using PWTranslator.Helpers;
using PWTranslator.Models;

namespace PWTranslator.Windows {
    public partial class MainWindow {
        private ResourceController ResourceController { get; set; }
        private List<FileEntry> Files { get; set; }

        public MainWindow() {
            InitializeComponent();
            Files = new List<FileEntry>();
            #region Регистрация обработчиков событий
            Loaded += OnLoaded;
            Drop += OnDrop;
            LbFiles.SelectionChanged += LbFilesOnSelectionChanged;
            #endregion
        }

        private void LbFilesOnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 0)
                return;
            var file = (FileEntry) e.AddedItems[0];
            SelectFile(file);
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            var langs = YandexTranslator.GetLangs();
            CbTranslateDirection.ItemsSource = null;
            CbTranslateDirection.ItemsSource = langs;
            if (langs != null && langs.Count > 0) {
                var defaultDirection = CbTranslateDirection.Items.IndexOf("en-ru");
                CbTranslateDirection.SelectedIndex = defaultDirection;
            }
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (ResourceController != null && ResourceController.XmlDoc != null && MessageBox.Show("The current translation is saved?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            RefreshFiles((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void RefreshFiles(IEnumerable<string> files) {
            Files.Clear();
            foreach (var file in files)
                Files.Add(new FileEntry { FullName = file, ShortName = Path.GetFileName(file) });
            LbFiles.ItemsSource = null;
            LbFiles.ItemsSource = Files;
        }

        private void SelectFile(FileEntry file) {
            ResourceController = ResourceController.Create(file.FullName);
            TeXml.Text = File.ReadAllText(file.FullName);
            BtnSave.IsEnabled = true;
            BtnTranslate.IsEnabled = true;
            if ((bool)ChkAutoTranslate.IsChecked)
                ResourceController.Translate(CbTranslateDirection.Text);
            IcResources.ItemsSource = null;
            IcResources.ItemsSource = ResourceController.Resources;
            BtnFileClose.IsEnabled = true;
        }

        private void BtnSaveOnClick(object sender, RoutedEventArgs e) {
            TeXml.Text = ResourceController.GetXml();
            ResourceController.SaveFile();
            LbFiles.IsEnabled = true;
        }

        private void BtnTranslateOnClick(object sender, RoutedEventArgs e) {
            if (CbTranslateDirection.SelectedIndex == -1) {
                MessageBox.Show("Select the direction of translation!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResourceController.Translate(CbTranslateDirection.Text);
            IcResources.ItemsSource = null;
            IcResources.ItemsSource = ResourceController.Resources;
        }

        private void NewValueOnTextChanged(object sender, TextChangedEventArgs e) {
            LbFiles.IsEnabled = false;
        }

        private void BtnFileCloseOnClick(object sender, RoutedEventArgs e) {
            LbFiles.IsEnabled = true;
            IcResources.ItemsSource = null;
            TeXml.Text = "";
            BtnFileClose.IsEnabled = false;
        }
    }
}
