using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace UnbloatVPN.Views;

public partial class HomePageView : UserControl
{
    private readonly BoxShadows headerBoxShadowsGrey = BoxShadows.Parse("0 0 12 2 LightGray");
    private readonly BoxShadows headerBoxShadowsNone = new BoxShadows();
    private const int HeaderExpandedHeight = 460;
    private const int HeaderCollapsedHeight = 48;
    private double scrollSnapTarget;
    private TopLevel? topLevel;
    private bool invalidateScrollSnap;
    private bool animatingScrollSnap;

    public HomePageView()
    {
        InitializeComponent();
    }
    
    private static double Lerp(double fromValue, double toValue, double weight)
    {
        return fromValue * (1 - weight) + toValue * weight;
    }
    
    private void RecalculateHeaderPositions()
    {
        var toCollapsedScroll = Math.Min(1, ForegroundViewer.Offset.Y / (HeaderExpandedHeight - HeaderCollapsedHeight)); // 0 - 1, scroll left until full collapse
        var headerCollapsed = 1.0d - toCollapsedScroll < 0.01d;
        
        HeaderBorder.BoxShadow = headerCollapsed ? headerBoxShadowsGrey : headerBoxShadowsNone;
        
        Canvas.SetTop(HeaderTitle, Lerp(HeaderCollapsedHeight / 2d - HeaderTitle.Bounds.Height / 2, 0, toCollapsedScroll));
        Canvas.SetLeft(HeaderTitle, Bounds.Width / 2 - HeaderTitle.Bounds.Width / 2);
        
        HeaderDescriptionInfo.Opacity = Lerp(1d, 0d, toCollapsedScroll);
        Canvas.SetTop(HeaderDescriptionInfo, HeaderCollapsedHeight);
        Canvas.SetLeft(HeaderDescriptionInfo, Bounds.Width / 2 - HeaderDescriptionInfo.Bounds.Width / 2);

        HeaderVpnButton.Width = Lerp(256, 48, toCollapsedScroll);
        HeaderVpnButton.Height = Lerp(256, 48, toCollapsedScroll);
        Canvas.SetTop(HeaderVpnButton, Lerp(64, 0, toCollapsedScroll));
        Canvas.SetLeft(HeaderVpnButton, Lerp(Bounds.Width / 2 - HeaderVpnButton.Bounds.Width / 2, Bounds.Width - HeaderVpnButton.Bounds.Width, toCollapsedScroll));
        
        Canvas.SetTop(HeaderVpnStatus, Lerp(320d, 24d, toCollapsedScroll));
        Canvas.SetLeft(HeaderVpnStatus, Bounds.Width / 2 - HeaderVpnStatus.Bounds.Width / 2);

        HeaderVpnStatusPanel.Opacity = Lerp(1d, 0d, toCollapsedScroll);

        HeaderIcon.Opacity = Lerp(0.6d, 1d, toCollapsedScroll);
        HeaderCanvas.Height = Lerp(360, 0d, toCollapsedScroll);
        ((LinearGradientBrush)HeaderBackground.Background!).GradientStops[0].Color =
            new Color((byte) (255d * (1.0d - toCollapsedScroll)), 240, 255, 255);
    }

    protected override void OnLoaded(RoutedEventArgs args)
    {
        RecalculateHeaderPositions();
        topLevel = TopLevel.GetTopLevel(this);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        RecalculateHeaderPositions();
    }

    private void OnForegroundScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        RecalculateHeaderPositions();
    }
    
    private void AnimateHeaderScrollSnap(TimeSpan deltaTime)
    {
        animatingScrollSnap = true;
        ForegroundViewer.Offset = new Vector(ForegroundViewer.Offset.X,
            Lerp(ForegroundViewer.Offset.Y, scrollSnapTarget, 0.1d));

        if (Math.Abs(ForegroundViewer.Offset.Y - scrollSnapTarget) < 1d || invalidateScrollSnap)
        {
            ForegroundViewer.Offset = new Vector(ForegroundViewer.Offset.X, scrollSnapTarget);
            invalidateScrollSnap = false;
            animatingScrollSnap = false;
            return;
        }
        
        topLevel?.RequestAnimationFrame(AnimateHeaderScrollSnap);
    }
    
    private void TestHeaderScrollSnap()
    {
        var toCollapsedScroll = ForegroundViewer.Offset.Y / (HeaderExpandedHeight - HeaderCollapsedHeight);
        if (toCollapsedScroll < 1.0d)
        {
            invalidateScrollSnap = false;
            scrollSnapTarget = Math.Round(toCollapsedScroll) * (HeaderExpandedHeight - HeaderCollapsedHeight);
            topLevel?.RequestAnimationFrame(AnimateHeaderScrollSnap);
        }
    }

    private void OnForegroundTouchReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OperatingSystem.IsAndroid())
        {
            TestHeaderScrollSnap();
        }
    }

    private void OnForegroundWheelScroll(object? sender, PointerWheelEventArgs e)
    {
        if (animatingScrollSnap)
        {
            invalidateScrollSnap = true;
        }
        else
        {
            TestHeaderScrollSnap();
        }
    }
}