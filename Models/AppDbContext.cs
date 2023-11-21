using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace APPMVC.NET.Models {
        public class AppDbContext : DbContext {

        public AppDbContext (DbContextOptions<AppDbContext> options) : base (options) 
        { 

        }

        protected override void OnModelCreating (ModelBuilder builder) {

            base.OnModelCreating (builder);
            // Bỏ tiền tố AspNet của các bảng: mặc định
            // foreach (var entityType in builder.Model.GetEntityTypes ()) {
            //     var tableName = entityType.GetTableName ();
            //     if (tableName.StartsWith ("AspNet")) {
            //         entityType.SetTableName (tableName.Substring (6));
            //     }
            // }
        }

    }

}