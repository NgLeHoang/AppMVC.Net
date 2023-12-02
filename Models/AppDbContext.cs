using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using APPMVC.NET.Models.Contacts;
using APPMVC.NET.Models.Blog;
using APPMVC.NET.Models.Product;


namespace APPMVC.NET.Models {
        public class AppDbContext : IdentityDbContext<AppUser> {

        public AppDbContext (DbContextOptions<AppDbContext> options) : base (options) 
        { 

        }

        protected override void OnModelCreating(ModelBuilder builder) 
        {

            base.OnModelCreating(builder);
            
            foreach (var entityType in builder.Model.GetEntityTypes()) 
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet")) 
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            builder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug).IsUnique();
            });

            builder.Entity<PostCategory>(entity => {
                entity.HasKey(c => new {c.PostID, c.CategoryID});
            });

            builder.Entity<Post>(entity => {
                entity.HasIndex(p => p.Slug).IsUnique();
            });


            builder.Entity<CategoryProduct>(entity => {
                entity.HasIndex(c => c.Slug).IsUnique();
            });

            builder.Entity<ProductCategoryProduct>(entity => {
                entity.HasKey(c => new {c.ProductID, c.CategoryID});
            });

            builder.Entity<ProductModel>(entity => {
                entity.HasIndex(p => p.Slug).IsUnique();
            });
        }

        public DbSet<ContactModel> Contacts { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }

        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<ProductCategoryProduct> ProductCategoryProducts { get; set; }

        public DbSet<ProductPhoto> ProductPhotos { get; set; }
    }

}