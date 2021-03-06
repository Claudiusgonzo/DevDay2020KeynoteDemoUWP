﻿using System;
using DevDay2020KeynoteDemoUWP.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace DevDay2020KeynoteDemoUWP.Pages
{
    public sealed partial class DetailPage : Page
    {
        public Place SelectedPlace { get; set; }
        private bool _firstTimeAnimation = true;

        public DetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                if (e.SourcePageType == typeof(DestinationsPage))
                {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backwardToMain", HeroImage);
                    //animation.Configuration = new DirectConnectedAnimationConfiguration();
                }
                else if (e.SourcePageType == typeof(ComparisonPage))
                {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("detailToComparison", HeroImage);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SelectedPlace = e.Parameter as Place;

            var aniamtion1 = ConnectedAnimationService.GetForCurrentView().GetAnimation("mainToDetail");
            aniamtion1?.TryStart(HeroImage, new UIElement[] { Header });

            var aniamtion2 = ConnectedAnimationService.GetForCurrentView().GetAnimation("comparisonToDetail");
            aniamtion2?.TryStart(HeroImage, new UIElement[] { Header });
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void OnPlanTripClick(object sender, RoutedEventArgs e)
        {
            PlanTrip.IsEnabled = false;

            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(HeroImage);
            HeroImageMirror.Source = bitmap;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("storePlace", HeroImageMirror);

            var mainPage = this.FindAscendant<MainPage>();
            if (!mainPage.PickedPlaces.Contains(SelectedPlace))
            {
                mainPage.PickedPlaces.Add(SelectedPlace);
            }

            var navView = mainPage.FindDescendant<WinUI.NavigationView>();
            if (navView.PaneCustomContent.FindDescendantByName("PlaceStore") is Button placeStoreButton)
            {
                var dot = placeStoreButton.FindDescendant<Ellipse>();

                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("storePlace");
                animation?.TryStart(dot);
                dot.Visibility = Visibility.Visible;

                // JL: Need to figutre out why the first time the animation doesn't run although animation returns true.
                if (_firstTimeAnimation)
                {
                    _firstTimeAnimation = false;
                    await Task.Delay(50);
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("storePlace", HeroImageMirror);
                    var animation1 = ConnectedAnimationService.GetForCurrentView().GetAnimation("storePlace");
                    animation1?.TryStart(dot);
                }
            }

            PlanTrip.IsEnabled = true;
        }



        /// <summary>
        /// STEP 2.2: Detail page code-behind 
        /// This fires when the TwoPaneViewMode (SinglePane, Wide & Tall) is changed.
        /// 
        /// Here we set the PlanTripTop button on the top right to be visible in Tall mode,
        /// so users don't need to scroll down to the content to make the same action.
        /// </summary>
        private void OnContentViewModeChanged(WinUI.TwoPaneView sender, object args)
        {
            switch (sender.Mode)
            {
                // Update layout when two Panes are stacked horizontally.
                case WinUI.TwoPaneViewMode.Wide:
                    PlanTripTop.Visibility = Visibility.Collapsed;
                    break;

                // Update layout when two Panes are stacked vertically.
                case WinUI.TwoPaneViewMode.Tall:
                    PlanTripTop.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
