using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESOapps.Shared.Services
{
    public class UiStateService
    {
        public bool IsSidebarOpen { get; private set; } = true;

        public string SidebarClass => IsSidebarOpen ? "sbopen" : "sbclose";

        public event Action? OnChange;

        public void ToggleSidebar()
        {
            IsSidebarOpen = !IsSidebarOpen;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
