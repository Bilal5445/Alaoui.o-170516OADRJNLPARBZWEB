How To Add API Doc
------------------

NuGet Package Manager
Default project : ArabiziWebAPI
Install-Package Microsoft.AspNet.WebApi.HelpPage
This command installs the necessary assemblies, creates new folder Areas/HelpPage and adds the MVC views for the help pages
Add a link to the Help page. The URI is /Help. ex : @Html.ActionLink("API", "Index", "Help", new { area = "" }, null)
<li>@Html.ActionLink("API", "Index", "Help", new { area = "" }, null)</li>