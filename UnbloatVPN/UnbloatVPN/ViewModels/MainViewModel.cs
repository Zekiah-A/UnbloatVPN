using CommunityToolkit.Mvvm.ComponentModel;

namespace UnbloatVPN.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ObservableObject currentPage = new HomePageViewModel();
}