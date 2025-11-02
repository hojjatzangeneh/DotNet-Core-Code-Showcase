using Microsoft.EntityFrameworkCore;

namespace EfHiLoSqlServer
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();

        // این connection string برای LocalDB هست؛ در صورت نیاز آن را تغییر بده
        private const string ConnectionString =@"Server=host.docker.internal,1433;Database=EfHiLoDemo;User Id=sa;Password=****;TrustServerCertificate=True;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("ProductHiLoSequence")
                        .StartsAt(1)
                        .IncrementsBy(1);

            modelBuilder.Entity<Product>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.Property(p => p.Id)
                       .UseHiLo("ProductHiLoSequence");
                builder.Property(p => p.Name)
                       .IsRequired()
                       .HasMaxLength(200);
            });
        }
    }
}
