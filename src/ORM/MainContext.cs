using Microsoft.EntityFrameworkCore;
using ORM.DbModels;

namespace ORM
{
    public sealed class MainContext : DbContext
    {
        public DbSet<UserInfo> UserInfos { get; set; }
        
        public DbSet<User> Users { get; set; }
        
        public DbSet<Stend> Stends { get; set; }
        
        public DbSet<Plan> Plans { get; set; }

        public MainContext(DbContextOptions options) : base(options)
        {
        }
    }
}