using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Menu
{
    public enum SidebarItemType
    {
        Divider,
        Heading,
        NavItem
    }
    public class SidebarItem
    {
        public string? Title { get; set; }
        public bool IsActive { get; set; }
        public SidebarItemType Type { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Area { get; set; }
        public string? AwesomeIcon { get; set; }

        public List<SidebarItem> Items { get; set; }
        public string? CollapseID { get; set; }
        public string GetLink(IUrlHelper urlHelper)
        {
            return urlHelper.Action(Action, Controller, new { Area });
        }

        public string RenderHtml(IUrlHelper urlHelper)
        {
            var html = new StringBuilder();
            if (Type == SidebarItemType.Divider)
            {
                html.Append("<hr class=\"sidebar-divider my-2\">");
            }
            else if (Type == SidebarItemType.Heading)
            {
                html.Append(@$"<div class=""sidebar-heading"">
                            {Title}
                            </div>");
            }
            else if (Type == SidebarItemType.NavItem)
            {
                var url = GetLink(urlHelper);
                var icon = (AwesomeIcon != null) ? $"<i class=\"{AwesomeIcon}\"></i>"
                                                     : "";
                var cssClass = "nav-item";
                if (IsActive) cssClass += " active";

                var collapseCss = "collapse";
                if (IsActive) collapseCss += " show";

                if (Items == null)
                {
                    html.Append($@"
                                <li class=""{cssClass}"">
                                    <a class=""nav-link"" href=""{url}"">
                                        {icon}
                                        <span>{Title}</span>
                                    </a>
                                </li>");
                }
                else
                {
                    var itemMenu = "";
                    foreach (var item in Items)
                    {
                        var urlItem = item.GetLink(urlHelper);
                        var cssItem = "collapse-item";
                        if (item.IsActive) cssItem += " active";
                        itemMenu += $"<a class=\"{cssItem}\" href=\"{urlItem}\">{item.Title}</a>";
                    }
                    html.Append($@"
                    <li class=""{cssClass}"">
                        <a class=""nav-link collapsed"" href=""#"" data-bs-toggle=""collapse"" data-bs-target=""#{CollapseID}""
                            aria-expanded=""true"" aria-controls=""{CollapseID}"">
                            {icon}
                            <span>{Title}</span>
                        </a>
                        <div id = ""{CollapseID}"" class=""{collapseCss}"" aria-labelledby=""headingTwo"" data-bs-parent=""#accordionSidebar"">
                            <div class=""bg-white py-2 collapse-inner rounded"">
                                {itemMenu}
                            </div>
                        </div>
                    </li>");
                }
            }

            return html.ToString();
        }
    }
}