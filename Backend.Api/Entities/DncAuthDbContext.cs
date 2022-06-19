
using Backend.Api.Entities.QueryModels.DncPermission;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class DncZeusDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public DncZeusDbContext(DbContextOptions<DncZeusDbContext> options) : base(options)
        {

        }
        /// <summary>
        /// 用户
        /// </summary>
        public DbSet<DncUser> DncUser { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public DbSet<DncRole> DncRole { get; set; }
        /// <summary>
        /// 菜单
        /// </summary>
        public DbSet<DncMenu> DncMenu { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public DbSet<DncIcon> DncIcon { get; set; }

        /// <summary>
        /// 用户-角色多对多映射
        /// </summary>
        public DbSet<DncUserRoleMapping> DncUserRoleMapping { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public DbSet<DncPermission> DncPermission { get; set; }
        /// <summary>
        /// 角色-权限多对多映射
        /// </summary>
        public DbSet<DncRolePermissionMapping> DncRolePermissionMapping { get; set; }
        /// <summary>
        /// 核心配置信息
        /// </summary>
        public DbSet<ZYSoftConfig> ZYSoftConfig { get; set; }
        /// <summary>
        /// 一般接口AccessToken
        /// </summary>
        public DbSet<ZYSoftAccessToken> ZYSoftAccessToken { get; set; }
        /// <summary>
        /// 账户与微信openID绑定
        /// </summary>
        public DbSet<ZYSoftUserWcMapping> ZYSoftUserWcMapping { get; set; }

        public DbSet<ZYSoftUserPersonalMapping> ZYSoftUserPersonalMapping { get; set; }
        public DbSet<ZYSoftUserPartnerMapping> ZYSoftUserPartnerMapping { get; set; }

        #region DbQuery
        /// <summary>
        /// 
        /// </summary>
        public DbQuery<DncPermissionWithAssignProperty> DncPermissionWithAssignProperty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DbQuery<DncPermissionWithMenu> DncPermissionWithMenu { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<DncUser>()
            //    .Property(x => x.Status);
            //modelBuilder.Entity<DncUser>()
            //    .Property(x => x.IsDeleted);


            modelBuilder.Entity<DncRole>(entity =>
            {
                entity.HasIndex(x => x.Code).IsUnique();
            });

            modelBuilder.Entity<DncMenu>(entity => { });


            modelBuilder.Entity<DncUserRoleMapping>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.UserGuid,
                    x.RoleCode
                });

                entity.HasOne(x => x.DncUser)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.UserGuid)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DncRole)
                    .WithMany(x => x.UserRoles)
                    .HasForeignKey(x => x.RoleCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DncPermission>(entity =>
            {
                entity.HasIndex(x => x.Code)
                    .IsUnique();

                entity.HasOne(x => x.Menu)
                    .WithMany(x => x.Permissions)
                    .HasForeignKey(x => x.MenuGuid);
            });

            modelBuilder.Entity<DncRolePermissionMapping>(entity =>
            {
                entity.HasKey(x => new
                {
                    x.RoleCode,
                    x.PermissionCode
                });

                entity.HasOne(x => x.DncRole)
                    .WithMany(x => x.Permissions)
                    .HasForeignKey(x => x.RoleCode)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DncPermission)
                    .WithMany(x => x.Roles)
                    .HasForeignKey(x => x.PermissionCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
