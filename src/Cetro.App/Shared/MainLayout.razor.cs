using Microsoft.AspNetCore.Components;

namespace Cetro.App.Shared
{
    public partial class MainLayout : LayoutComponentBase
    {
        [Inject]
        private NavigationManager? NavigationManager { get; set; }
    }
}
