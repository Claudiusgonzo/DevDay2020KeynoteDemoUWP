﻿using System;
using WinUI = Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml.Input;
using System.Collections.ObjectModel;
using DevDay2020KeynoteDemoUWP.Model;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.UI;

namespace DevDay2020KeynoteDemoUWP.Pages
{
    public sealed partial class MainPage
    {
        public ObservableCollection<Place> PickedPlaces { get; } = new ObservableCollection<Place>();

        private HingeAngleSensor _sensor;

        public MainPage()
        {
            InitializeComponent();

            //ApplicationView.PreferredLaunchViewSize = new Size(1440, 936);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            CustomizeTitleBar();
            void CustomizeTitleBar()
            {
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                // Draw into the title bar.
                coreTitleBar.ExtendViewIntoTitleBar = true;
                DraggableAppTitleBarArea.Height = coreTitleBar.Height;

                // Set a draggable region.
                Window.Current.SetTitleBar(DraggableAppTitleBarArea);

                coreTitleBar.LayoutMetricsChanged += (s, args) =>
                {
                    DraggableAppTitleBarArea.Height = s.Height;
                };

                coreTitleBar.IsVisibleChanged += (s, args) =>
                {
                    DraggableAppTitleBarArea.Visibility = s.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                };

                // Remove the solid-colored backgrounds behind the caption controls and system back button.
                var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
                viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                viewTitleBar.ButtonForegroundColor = Color.FromArgb(255, 4, 119, 191);
            }

            ConnectedAnimationService.GetForCurrentView().DefaultDuration = TimeSpan.FromMilliseconds(400);

            if (MainNav.MenuItems[0] is WinUI.NavigationViewItemBase item)
            {
                MainNav.SelectedItem = item;
                NavigateToPage(item.Tag);
            }

            Window.Current.SizeChanged += async (s, e) =>
                        {
                            await Task.Delay(1200);

                            var isSpanned = ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Spanning;
                            if (isSpanned)
                            {
                                Logo.GoToDualScreenState();
                            }
                            else
                            {
                                Logo.GoToSingleScreenState();
                            }
                        };

            Loaded += async (s, e) =>
            {
                var titleBarHeight = CoreApplication.GetCurrentView().TitleBar.Height;
                MainNav.Padding = new Thickness(0, titleBarHeight, 0, 0);

                Logo.Start();

                _sensor = await HingeAngleSensor.GetDefaultAsync();

                if (_sensor != null)
                {
                    _sensor.ReportThresholdInDegrees = _sensor.MinReportThresholdInDegrees;

                    _sensor.ReadingChanged += OnSensorReadingChanged;
                    var current = (await _sensor.GetCurrentReadingAsync()).AngleInDegrees;
                }

                async void OnSensorReadingChanged(HingeAngleSensor sender, HingeAngleSensorReadingChangedEventArgs args)
                {
                    // Event is invoked from a different thread.
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        double angle = 0.0;

                        if (args.Reading.AngleInDegrees <= 180)
                        {
                            angle = args.Reading.AngleInDegrees / 2 - 90;
                        }
                        else
                        {
                            angle = (args.Reading.AngleInDegrees - 180) * 2;
                        }

                        Logo.SetAngle(angle);
                    });
                }
            };
        }

        private void OnMainNavItemInvoked(WinUI.NavigationView sender, WinUI.NavigationViewItemInvokedEventArgs args) =>
            NavigateToPage(args.InvokedItemContainer.Tag);

        private void NavigateToPage(object pageTag)
        {
            var pageName = $"DevDay2020KeynoteDemoUWP.Pages.{pageTag}";
            var pageType = Type.GetType(pageName);

            ContentFrame.Navigate(pageType);
        }

        private void OnPlaceStoreClick(object sender, RoutedEventArgs e)
        {
            PickedPlacesPane.Visibility = Visibility.Visible;
            ContentFrame
                .Fade(0.5f)
                .Scale(scaleX: 0.95f, scaleY: 0.95f, centerX: (float)ContentFrame.ActualWidth / 2, centerY: (float)ContentFrame.ActualHeight / 2)
                .Start();
        }

        private void OnDismissTouchAreaTapped(object sender, TappedRoutedEventArgs e)
        {
            PickedPlacesPane.Visibility = Visibility.Collapsed;
            ContentFrame
                .Fade(1.0f)
                .Scale(scaleX: 1.0f, scaleY: 1.0f, centerX: (float)ContentFrame.ActualWidth / 2, centerY: (float)ContentFrame.ActualHeight / 2)
                .Start();
        }

        private async void OnWonderbarToggleChecked(object sender, RoutedEventArgs e)
        {
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);

            if (PickedPlaces.Any() && modeSwitched)
            {
                VisualStateManager.GoToState(this, nameof(ApplicationViewMode.CompactOverlay), false);
            }
        }

        private async void OnWonderbarToggleUnchecked(object sender, RoutedEventArgs e)
        {
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            if (modeSwitched)
            {
                VisualStateManager.GoToState(this, nameof(ApplicationViewMode.Default), false);
            }
        }

        private async void OnPlacesListViewDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.FirstOrDefault() is Place place)
            {
                e.Data.RequestedOperation = DataPackageOperation.Copy;

                //e.Data.SetData(StandardDataFormats.Text, place.CityName);

                var imageUri = new Uri($"ms-appx://{place.ImageUri}", UriKind.RelativeOrAbsolute);
                var file = await StorageFile.GetFileFromApplicationUriAsync(imageUri);
                e.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(file));
            }
        }
    }
}
