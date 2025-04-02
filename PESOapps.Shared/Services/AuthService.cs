using Microsoft.AspNetCore.Components.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESOapps.Shared.Services
{
    public class AuthService
    {
        private readonly ProtectedSessionStorage SessionStorage;
        private readonly NavigationManager Navigation;

        public AuthService(ProtectedSessionStorage sessionStorage, NavigationManager navigation)
        {
            SessionStorage = sessionStorage;
            Navigation = navigation;
        }

        public async Task Logout()
        {
        await SessionStorage.DeleteAsync("IsLoggedIn");
            Navigation.NavigateTo("/pesobeta/Login", forceLoad: true);
        }
    }
}
