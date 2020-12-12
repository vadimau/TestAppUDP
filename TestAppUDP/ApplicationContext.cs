using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestAppUDP
{
    class ApplicationContext : DbContext
    {
        public DbSet<udpData> udpData { get; set; }

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
    }
}
