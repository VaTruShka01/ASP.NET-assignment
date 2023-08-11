using Microsoft.AspNetCore.Mvc;
using Assignment.Models;

namespace Assignment.Components.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke ()
        {

            var menuItems = new List<MenuItem>
            {
                new MenuItem {Controller = "Home", Action = "Index", Label = "Home" },
                new MenuItem {Controller = "Shop", Action = "Index", Label = "Shop" },
                new MenuItem {Controller= "Categories", Action = "Index", Label = "Category", Authorized = true, AllowedRoles = new List<string> {"Administrator" }, DropdownItems = new List<MenuItem> {
                    new MenuItem{Controller = "Categories", Action = "Index", Label = "List"},
                    new MenuItem{Controller = "Categories", Action = "Create", Label = "Create"}
                } },
                new MenuItem {Controller= "Bags", Action = "Index", Label = "Bags", Authorized = true, AllowedRoles = new List<string> {"Administrator" }, DropdownItems = new List<MenuItem> {
                    new MenuItem{Controller = "Bags", Action = "Index", Label = "List"},
                    new MenuItem{Controller = "Bags", Action = "Create", Label = "Create"}
                } },
                new MenuItem {Controller= "Home", Action = "Privacy" , Label = "Privacy"}

                };

            return View(menuItems);
        }
    }
}
