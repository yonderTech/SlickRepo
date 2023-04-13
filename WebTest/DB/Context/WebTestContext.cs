using Microsoft.EntityFrameworkCore;
using System;

namespace WebTest
{
    public class WebTestContext : DbContext
    {
        private IConfiguration Configuration { get; set; }

        public WebTestContext(DbContextOptions<WebTestContext> options, IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cs = Configuration.GetSection("ConnectionStrings:WebTestDB");
            optionsBuilder.UseSqlServer(cs.Value);
            base.OnConfiguring(optionsBuilder);
        }


        public DbSet<DB.Models.User> Users { get; set; }
        public DbSet<DB.Models.Post> Posts { get; set; }
    }
}
