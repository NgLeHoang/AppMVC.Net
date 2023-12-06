using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace APPMVC.NET.Menu
{
    public class AdminSidebarService
    {
        private readonly IUrlHelper urlHelper;
        public List<SidebarItem> Items { get; set; } = new List<SidebarItem>();

        public AdminSidebarService(IUrlHelperFactory helperFactory, IActionContextAccessor actionContext)
        {
            urlHelper = helperFactory.GetUrlHelper(actionContext.ActionContext);

            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Heading, Title = "Quản lí chung" });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "DbManage",
                Action = "Index",
                Area = "Database",
                Title = "Quản lí Database",
                AwesomeIcon = "fas fa-database"
            });
            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Controller = "Contact",
                Action = "Index",
                Area = "Contact",
                Title = "Quản lí liên hệ",
                AwesomeIcon = "far fa-address-card"
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Phân quyền & thành viên",
                AwesomeIcon = "far fa-folder",
                CollapseID = "role",
                Items = new List<SidebarItem>()
                {
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Role",
                        Action = "Index",
                        Area = "Identity",
                        Title = "Các vai trò"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Role",
                        Action = "Create",
                        Area = "Identity",
                        Title = "Tạo vai trò mới"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "User",
                        Action = "Index",
                        Area = "Identity",
                        Title = "Danh sách thành viên"
                    },
                }
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lí bài viết",
                AwesomeIcon = "far fa-folder",
                CollapseID = "post",
                Items = new List<SidebarItem>()
                {
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Các chuyên mục"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Category",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo chuyên mục mới"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Index",
                        Area = "Blog",
                        Title = "Các bài viết"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "Post",
                        Action = "Create",
                        Area = "Blog",
                        Title = "Tạo bài viết mới"
                    },
                }
            });
            Items.Add(new SidebarItem() { Type = SidebarItemType.Divider });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lí sản phẩm",
                AwesomeIcon = "far fa-folder",
                CollapseID = "product",
                Items = new List<SidebarItem>()
                {
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Index",
                        Area = "Product",
                        Title = "Các chuyên mục"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "CategoryProduct",
                        Action = "Create",
                        Area = "Product",
                        Title = "Tạo chuyên mục mới"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManage",
                        Action = "Index",
                        Area = "Product",
                        Title = "Các sản phẩm"
                    },
                    new()
                    {
                        Type = SidebarItemType.NavItem,
                        Controller = "ProductManage",
                        Action = "Create",
                        Area = "Product",
                        Title = "Tạo sản phẩm mới"
                    },
                }
            });
        }

        public string RenderHtml()
        {
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(urlHelper));
            }

            return html.ToString();
        }

        public void SetActive(string Controller, string Action, string Area)
        {
            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Action == Action && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var childItem in item.Items)
                        {
                            if (childItem.Controller == Controller && childItem.Action == Action && childItem.Area == Area)
                            {
                                childItem.IsActive = true;
                                item.IsActive = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}