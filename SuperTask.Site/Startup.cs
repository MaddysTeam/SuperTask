using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(XzNursery.Admin.Startup))]
namespace XzNursery.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
