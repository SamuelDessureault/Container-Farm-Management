using ContainerFarmManagement.Models;
using ContainerFarmManagement.Utilities;
using Firebase.Auth.Providers;
using Firebase.Auth;
using ContainerFarmManagement.Services;
using System.Collections.ObjectModel;

namespace ContainerFarmManagement;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        signUpAccountTypePicker.ItemsSource = Enum.GetNames(typeof(Account.AccountType));
        signUpAccountTypePicker.SelectedIndex = 0;
        BindingContext = this;
    }

    private async void btn_signIn_Clicked(object sender, EventArgs e)
    {
        try
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            if (accessType == NetworkAccess.Internet)
            {
                var client = AuthService.Client;
                FetchUserProvidersResult resultEmail = await client.FetchSignInMethodsForEmailAsync(signInEmailEntry.Text);
                if (resultEmail.UserExists)
                {
                    UserCredential userCreds = await client.SignInWithEmailAndPasswordAsync(signInEmailEntry.Text, signInPasswordEntry.Text);
                    AuthService.UserCreds = userCreds;

                    Account account = App.AccountRepo.Accounts.Where(a => a.Email == signInEmailEntry.Text).FirstOrDefault();
                    if (account == null)
                    {
                        account = new Account("", signInEmailEntry.Text, signInPasswordEntry.Text, Account.AccountType.TECHNICIAN);
                        await App.AccountRepo.AddAccount(account);

                    }
                    AuthService.Account = account;

                    await DisplayAlert("Login", "Logged in successfully", "OK");

                    login.IsVisible = false;
                    logout.IsVisible = true;
                    BindingContext = userCreds.User;

                    App.Account = await App.AccountRepo.GetAccountByEmail(signInEmailEntry.Text);

                    //SET PROPER ROUTE HERE
                    await Shell.Current.GoToAsync($"//ContainerList");
                }
                else
                {
                    lbl_err.Text = "This user does not exist";
                    lbl_err.IsVisible = true;
                    await DisplayAlert("Alert", "This user does not exist", "OK");
                }
            }
            else
            {
                lbl_err.Text = "No internet connection";
                lbl_err.IsVisible = true;
                await DisplayAlert("Alert", "No internet connection", "OK");
            }

        }
        catch (FirebaseAuthException ex)
        {
            lbl_err.Text = ex.Reason.ToString();
            lbl_err.IsVisible = true;
            await DisplayAlert("Alert", ex.Reason.ToString(), "OK");
        }
        catch (Exception ex)
        {
            lbl_err.Text = ex.Message;
            await DisplayAlert("Alert", ex.Message, "OK");
        }
    }

    private async void btn_signOut_Clicked(object sender, EventArgs e)
    {
        try
        {
            AuthService.Client.SignOut();

            login.IsVisible = true;
            logout.IsVisible = false;

            App.Account = null;

            //SET PROPER ROUTE HERE
            //await Shell.Current.GoToAsync($"//Login");
        }
        catch (FirebaseAuthException ex)
        {
            await DisplayAlert("Alert", ex.Reason.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Alert", ex.Message, "OK");
        }
    }

    private void btn_cancel_Clicked(object sender, EventArgs e)
    {
        signup.IsVisible = false;
        login.IsVisible = true;
    }

    private async void btn_submit_Clicked(object sender, EventArgs e)
    {
        try
        {
            UserCredential userCreds = await AuthService.Client.CreateUserWithEmailAndPasswordAsync(signUpEmailEntry.Text, signUpPasswordEntry.Text);
            AuthService.UserCreds = userCreds;
            await DisplayAlert("Sign Up", "Account created successfully", "OK");


            Account.AccountType type;

            if (signUpAccountTypePicker.SelectedIndex == 0)
                type = Account.AccountType.OWNER;
            else
                type = Account.AccountType.TECHNICIAN;


            Account newAccount = new Account("", signUpUsernameEntry.Text, signUpEmailEntry.Text, type);
            await App.AccountRepo.AddAccount(newAccount);

            login.IsVisible = true;
            logout.IsVisible = false;
            signup.IsVisible = false;

            BindingContext = userCreds.User.Uid;

            //SET PROPER ROUTE HERE
            //await Shell.Current.GoToAsync($"//Login");
        }
        catch (FirebaseAuthException ex)
        {
            await DisplayAlert("Alert", ex.Reason.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Alert", ex.Message, "OK");
        }
    }

    private void btn_wantSignUp_Clicked(object sender, EventArgs e)
    {
        signup.IsVisible = true;
        login.IsVisible = false;
    }

    private async void btn_back_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//ContainerList");
    }
}