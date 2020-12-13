using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestAppUDP
{
    class ApplicationContext : DbContext
    {
        public DbSet<UdpData> udpData { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;user=root;password=1qazse456;database=mydb;"
                
            );
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UdpData>().HasIndex(u => new { u.value1, u.value2, u.value3, u.value4, u.value5 });
        }
    }
}
