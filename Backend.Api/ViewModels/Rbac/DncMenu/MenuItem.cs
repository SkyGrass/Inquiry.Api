using System.Collections.Generic;
using static Backend.Api.Entities.Enums.CommonEnum;

namespace Backend.Api.ViewModels.Rbac.DncMenu
{
    public class MenuItem
    {
        public MenuItem()
        {
            Meta = new MenuMeta();
            Children = new List<MenuItem>();
        }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Component { get; set; }
        public string ParentId { get; set; }
        public YesOrNo AllowPC { get; set; }
        public YesOrNo AllowMobile { get; set; }
        public string MobileName { get; set; }
        public string MobileUrl { get; set; }
        public string MobileIcon { get; set; }
        public MenuMeta Meta { get; set; }
        public List<MenuItem> Children { get; set; }
    }

    public class MenuMeta
    {
        public MenuMeta()
        {
            Permission = new List<string>();
            BeforeCloseFun = "";
        }
        public bool HideInMenu { get; set; }
        public string Icon { get; set; }
        public bool NotCache { get; set; }
        public string Title { get; set; }
        public List<string> Permission { get; set; }
        public string BeforeCloseFun { get; set; }
    }
}
