using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ContainerFarmManagement.Views;

public partial class ContainerListPage : ContentPage
{

    /// <summary>
    /// The list of containers linked to the account.
    /// </summary>
    public ObservableCollection<Container> ContainerList { get; set; }

    public ContainerListPage()
    {
        InitializeComponent();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var list = await App.ContainerRepo.GetContainers(App.Account.Key);
        ContainerList = new ObservableCollection<Container>(list);
        await RequestLocationPermissionAsync();

        BindingContext = this;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Container container = e.Parameter as Container;

        if (App.Account.Type == Account.AccountType.TECHNICIAN)
        {
            await Navigation.PushAsync(new Views.FarmTechViews.ContainerViewPage(container));
        }
        else
        {
            await Navigation.PushAsync(new Views.FarmOwnerViews.GeoLocationView(container));
        }
    }

    private async void deleteContainer_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayActionSheet("Delete this container?", "NO", "YES");
        if (result.Equals("YES"))
        {
            Container container = (sender as MenuItem).CommandParameter as Container;

            if (App.Account.Type == Account.AccountType.OWNER)
                await App.ContainerRepo.RemoveContainer(container);
            else
            {
                container.RegisteredUsers.Remove(App.Account.Key);
                await App.ContainerRepo.EditContainer(container);
            }
        }
    }

    private async void editContainer_Clicked(object sender, EventArgs e)
    {

        Container container = (sender as MenuItem).CommandParameter as Container;
        if (App.Account.Type == Account.AccountType.TECHNICIAN)
        {
            await Navigation.PushAsync(new Views.FarmTechViews.AddEditContainerPage(container));
        }
        else
        {
            await Navigation.PushAsync(new Views.FarmOwnerViews.AddEditOwner(container))
;
        }

    }

    private async void toolAccount_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//Login");
    }

    private async void addContainer_Clicked(object sender, EventArgs e)
    {
        if (AuthService.Account.Type == Account.AccountType.OWNER)
            await Navigation.PushAsync(new Views.FarmOwnerViews.AddEditOwner());
        else
            await Navigation.PushAsync(new Views.FarmTechViews.AddEditContainerPage());
    }

    private async void toolAddContainer_Clicked(object sender, EventArgs e)
    {
        if (App.Account.Type == Account.AccountType.TECHNICIAN)
        {
            await Navigation.PushAsync(new Views.FarmTechViews.AddEditContainerPage());
        }
        else
        {
            await Navigation.PushAsync(new Views.FarmOwnerViews.AddEditOwner())
;
        }
    }

    private async Task<bool> RequestLocationPermissionAsync()
    {

        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Location access is required for this feature.", "OK");
            return false;
        }


        return true;
    }
}