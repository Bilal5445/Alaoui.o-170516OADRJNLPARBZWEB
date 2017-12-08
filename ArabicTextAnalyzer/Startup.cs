using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ArabicTextAnalyzer.Startup))]
namespace ArabicTextAnalyzer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
