using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models.Database
{
    public partial class ApplicationContext : DbContext
    {
        public ApplicationContext() 
        {
          
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        public virtual DbSet<UploadDocument> UploadDocuments{ get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
            optionsBuilder.UseNpgsql("Server=localhost;Database=postgres;Username=postgres;Password=akdnehrc;Port=5432;");

        }
    }
}
