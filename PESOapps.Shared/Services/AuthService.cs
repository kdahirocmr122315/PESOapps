using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PESOapps.Shared.Services
{
    public class AuthService
    {
        private readonly ProtectedSessionStorage SessionStorage;
        private readonly NavigationManager Navigation;
        private readonly IJSRuntime JS;

        public AuthService(ProtectedSessionStorage sessionStorage, NavigationManager navigation, IJSRuntime js)
        {
            SessionStorage = sessionStorage;
            Navigation = navigation;
            JS = js;
        }

        public async Task Logout()
        {
            // Clear LocalStorage and SessionStorage
            await JS.InvokeVoidAsync("localStorage.clear");
            await SessionStorage.DeleteAsync("IsLoggedIn");
            await SessionStorage.DeleteAsync("UserId");
            await SessionStorage.DeleteAsync("UserType");

            // Redirect to login
            Navigation.NavigateTo("/pesobeta/Login", forceLoad: true);
        }
    }
}
