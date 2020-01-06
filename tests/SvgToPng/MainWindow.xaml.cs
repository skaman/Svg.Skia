﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SvgToPng.ViewModels;

namespace SvgToPng
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel VM { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            VM = new MainWindowViewModel()
            {
                Items = new ObservableCollection<Item>(),
                ReferencePaths = new ObservableCollection<string>(),
                ItemsFilter = (o) =>
                {
                    var name = TextItemsFilter?.Text;
                    var isEmpty = string.IsNullOrWhiteSpace(name);
                    if (o is Item item && !isEmpty)
                    {
                        var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                        return compareInfo.IndexOf(item.Name, name, CompareOptions.IgnoreCase) >= 0;
                    }
                    return true;
                }
            };
            VM.LoadItems("Items.json");
            VM.CreateItemsView();
#if DEBUG
            VM.ReferencePaths = new ObservableCollection<string>(new string[]
            {
                @"c:\DOWNLOADS\GitHub\Svg.Skia\externals\SVG\Tests\W3CTestSuite\png\",
                @"c:\DOWNLOADS\GitHub-Forks\resvg-test-suite\png\",
                @"e:\Dropbox\Draw2D\SVG\vs2017-png\",
                @"e:\Dropbox\Draw2D\SVG\W3CTestSuite-png\"
            });
            TextOutputPath.Text = Path.Combine(Directory.GetCurrentDirectory(), "png");
#endif

            this.Closing += MainWindow_Closing;
            this.TextItemsFilter.TextChanged += TextItemsFilter_TextChanged;

            this.items.SelectionChanged += Items_SelectionChanged;
            this.items.MouseDoubleClick += Items_MouseDoubleClick;

#if true
            this.skElementSvg.PaintSurface += OnPaintCanvasSvg;
            this.skElementPng.PaintSurface += OnPaintCanvasPng;
            this.skElementDiff.PaintSurface += OnPaintCanvasDiff;
#endif

#if false
            this.glHostSvg.Initialized += OnGLControlHostSvg;
            this.glHostPng.Initialized += OnGLControlHostPng;
            this.glHostDiff.Initialized += OnGLControlHostDiff;
#endif

#if true
            skElementSvg.Visibility = Visibility.Visible;
            skElementPng.Visibility = Visibility.Visible;
            skElementDiff.Visibility = Visibility.Visible;
            glHostSvg.Visibility = Visibility.Collapsed;
            glHostPng.Visibility = Visibility.Collapsed;
            glHostDiff.Visibility = Visibility.Collapsed;
#else
            skElementSvg.Visibility = Visibility.Collapsed;
            skElementPng.Visibility = Visibility.Collapsed;
            skElementDiff.Visibility = Visibility.Collapsed;
            glHostSvg.Visibility = Visibility.Visible;
            glHostPng.Visibility = Visibility.Visible;
            glHostDiff.Visibility = Visibility.Visible;
#endif

            DataContext = this.VM;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            VM.SaveItems("Items.json");
            VM.ClearItems();
        }

        private void TextItemsFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            VM.ItemsView.Refresh();
        }

        private void Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (items.SelectedItem is Item item)
            {
                TextOpenTime.Text = "";
                TextToPictureTime.Text = "";
                TextDrawTime.Text = "";
                VM.UpdateItem(item, (text) => TextOpenTime.Text = text, (text) => TextToPictureTime.Text = text);
            }

            skElementSvg.InvalidateVisual();
#if true
            skElementPng.InvalidateVisual();
            skElementDiff.InvalidateVisual();
#endif

#if true
            glHostSvg.Child?.Invalidate();
            glHostPng.Child?.Invalidate();
            glHostDiff.Child?.Invalidate();
#endif
        }

        private void Items_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (items.SelectedItem is Item item)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Process.Start("notepad", item.SvgPath);
                }
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    Process.Start("explorer", item.SvgPath);
                }
            }
        }

        private void HandleDrop(string[] paths, string referencePath, string outputPath)
        {
            var inputFiles = MainWindowViewModel.GetFilesDrop(paths).ToList();
            if (inputFiles.Count > 0)
            {
                VM.AddItems(inputFiles, VM.Items, referencePath, outputPath);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths != null && paths.Length > 0)
                {
                    HandleDrop(paths, TextReferencePath.Text, TextOutputPath.Text);
                }
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            VM.ClearItems();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Supported Files (*.svg;*.svgz)|*.svg;*.svgz|Svg Files (*.svg)|*.svg;|Svgz Files (*.svgz)|*.svgz|All Files (*.*)|*.*",
                Multiselect = true,
                FilterIndex = 0
            };
            if (dlg.ShowDialog() == true)
            {
                var paths = dlg.FileNames;
                if (paths != null && paths.Length > 0)
                {
                    HandleDrop(paths, TextReferencePath.Text, TextOutputPath.Text);
                }
            }
        }

        private async void ButtonSavePng_Click(object sender, RoutedEventArgs e)
        {
            string outputPath = TextOutputPath.Text;
            await Task.Factory.StartNew(() =>
            {
                VM.SaveItemsAsPng(VM.Items);
            });
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Items Files (*.json)|*.json;|All Files (*.*)|*.*",
                DefaultExt = "json",
                FilterIndex = 0
            };
            if (dlg.ShowDialog() == true)
            {
                var path = dlg.FileName;
                if (path != null)
                {
                    VM.ClearItems();
                    VM.LoadItems(path);
                    VM.CreateItemsView();
                    DataContext = null;
                    DataContext = VM;
                }
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Items Files (*.json)|*.json;|All Files (*.*)|*.*",
                FileName = "Items",
                DefaultExt = "json",
                FilterIndex = 0
            };
            if (dlg.ShowDialog() == true)
            {
                var path = dlg.FileName;
                if (path != null)
                {
                    VM.SaveItems(path);
                }
            }
        }

        private void OnGLControlHostSvg(object sender, EventArgs e)
        {
            var glControl = new SKGLControl();
            glControl.PaintSurface += OnPaintGLSvg;
            glControl.Dock = System.Windows.Forms.DockStyle.None;
            var host = (WindowsFormsHost)sender;
            host.Child = glControl;
        }

        private void OnPaintGLSvg(object sender, SKPaintGLSurfaceEventArgs e)
        {
            OnPaintSurfaceSvg(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void OnPaintCanvasSvg(object sender, SKPaintSurfaceEventArgs e)
        {
            OnPaintSurfaceSvg(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void OnPaintSurfaceSvg(SKCanvas canvas, int width, int height)
        {
            if (items.SelectedItem is Item item && item.Picture != null)
            {
                var stopwatch = Stopwatch.StartNew();

                canvas.Clear(SKColors.White);

                float pwidth = item.Picture.CullRect.Width;
                float pheight = item.Picture.CullRect.Height;
                if (pwidth > 0f && pheight > 0f)
                {
                    skElementSvg.Width = pwidth;
                    skElementSvg.Height = pheight;
                    glHostSvg.Width = pwidth;
                    glHostSvg.Height = pheight;
                    canvas.DrawPicture(item.Picture);
                }

                stopwatch.Stop();
                TextDrawTime.Text = $"{Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3)}ms";
                Debug.WriteLine($"Draw: {Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3)}ms");
            }
            else
            {
                canvas.Clear(SKColors.White);
            }
        }

        private void OnGLControlHostPng(object sender, EventArgs e)
        {
            var glControl = new SKGLControl();
            glControl.PaintSurface += OnPaintGLPng;
            glControl.Dock = System.Windows.Forms.DockStyle.None;
            var host = (WindowsFormsHost)sender;
            host.Child = glControl;
        }

        private void OnPaintGLPng(object sender, SKPaintGLSurfaceEventArgs e)
        {
            OnPaintSurfacePng(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void OnPaintCanvasPng(object sender, SKPaintSurfaceEventArgs e)
        {
            OnPaintSurfacePng(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void OnPaintSurfacePng(SKCanvas canvas, int width, int height)
        {
            canvas.Clear(SKColors.White);

            if (items.SelectedItem is Item item && item.ReferencePng != null)
            {
                float pwidth = item.ReferencePng.Width;
                float pheight = item.ReferencePng.Height;
                if (pwidth > 0f && pheight > 0f)
                {
                    skElementPng.Width = pwidth;
                    skElementPng.Height = pheight;
                    glHostPng.Width = pwidth;
                    glHostPng.Height = pheight;
                    canvas.DrawBitmap(item.ReferencePng, 0f, 0f);
                }
            }
        }

        private void OnGLControlHostDiff(object sender, EventArgs e)
        {
            var glControl = new SKGLControl();
            glControl.PaintSurface += OnPaintGLDiff;
            glControl.Dock = System.Windows.Forms.DockStyle.None;
            var host = (WindowsFormsHost)sender;
            host.Child = glControl;
        }

        private void OnPaintGLDiff(object sender, SKPaintGLSurfaceEventArgs e)
        {
            OnPaintSurfaceDiff(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void OnPaintCanvasDiff(object sender, SKPaintSurfaceEventArgs e)
        {
            OnPaintSurfaceDiff(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void OnPaintSurfaceDiff(SKCanvas canvas, int width, int height)
        {
            canvas.Clear(SKColors.White);

            if (items.SelectedItem is Item item && item.PixelDiff != null)
            {
                float pwidth = item.PixelDiff.Width;
                float pheight = item.PixelDiff.Height;
                if (pwidth > 0f && pheight > 0f)
                {
                    skElementDiff.Width = pwidth;
                    skElementDiff.Height = pheight;
                    glHostDiff.Width = pwidth;
                    glHostDiff.Height = pheight;
                    canvas.DrawBitmap(item.PixelDiff, 0f, 0f);
                }
            }
        }
    }
}
