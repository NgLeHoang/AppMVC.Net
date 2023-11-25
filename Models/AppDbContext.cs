using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using APPMVC.NET.Models.Contacts;
using APPMVC.NET.Models.Blog;


namespace APPMVC.NET.Models {
        public class AppDbContext : IdentityDbContext<AppUser> {

        public AppDbContext (DbContextOptions<AppDbContext> options) : base (options) 
        { 

        }

        protected override void OnModelCreating (ModelBuilder builder) {

            base.OnModelCreating (builder);
            
            foreach (var entityType in builder.Model.GetEntityTypes ()) {
                var tableName = entityType.GetTableName ();
                if (tableName.StartsWith ("AspNet")) 
                {
                    entityType.SetTableName (tableName.Substring (6));
                }
            }

            builder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug);
            });
        }

        public DbSet<ContactModel> Contacts { get; set; }

        public DbSet<Category> Categories { get; set; }
    }

}