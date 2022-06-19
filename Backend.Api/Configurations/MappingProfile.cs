using AutoMapper;
using Backend.Api.Entities;
using Backend.Api.ViewModels.Bus.Subscribe;
using Backend.Api.ViewModels.Rbac.DncIcon;
using Backend.Api.ViewModels.Rbac.DncMenu;
using Backend.Api.ViewModels.Rbac.DncPermission;
using Backend.Api.ViewModels.Rbac.DncRole;
using Backend.Api.ViewModels.Rbac.DncUser;

namespace Backend.Api.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public MappingProfile()
        {
            #region DncUser
            CreateMap<DncUser, UserJsonModel>();
            CreateMap<UserCreateViewModel, DncUser>();
            CreateMap<UserEditViewModel, DncUser>();
            CreateMap<DncUser, UserEditViewModel>();
            #endregion

            #region DncRole 
            CreateMap<DncRole, RoleJsonModel>();
            CreateMap<RoleCreateViewModel, DncRole>();
            CreateMap<DncRole, RoleCreateViewModel>();
            #endregion

            #region DncMenu
            CreateMap<DncMenu, MenuJsonModel>();
            CreateMap<MenuCreateViewModel, DncMenu>();
            CreateMap<MenuEditViewModel, DncMenu>();
            CreateMap<DncMenu, MenuEditViewModel>();
            #endregion

            #region DncIcon
            CreateMap<DncIcon, IconCreateViewModel>();
            CreateMap<DncIcon, IconJsonModel>();
            CreateMap<IconCreateViewModel, DncIcon>();
            #endregion

            #region DncPermission
            CreateMap<DncPermission, PermissionJsonModel>()
                .ForMember(d => d.MenuName, s => s.MapFrom(x => x.Menu.Name))
                .ForMember(d => d.PermissionTypeText, s => s.MapFrom(x => x.Type.ToString()));
            CreateMap<PermissionCreateViewModel, DncPermission>();
            CreateMap<PermissionEditViewModel, DncPermission>();
            CreateMap<DncPermission, PermissionEditViewModel>();
            #endregion
              
        }
    }
}
