using SnusProject.Models;
using System.Data.Entity;

namespace SnusProject.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base("name=PostgresConnection")
        {
        }

        public DbSet<OperationLog> OperationLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperationLog>().ToTable("OperationLogs");
        }
    }
}
