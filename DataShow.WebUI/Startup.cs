using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DataShow.WebUI.Startup))]
namespace DataShow.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
