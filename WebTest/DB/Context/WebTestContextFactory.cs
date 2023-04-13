using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using WebTest;

namespace API.Test
{
    public class ContextFactory : IDesignTimeDbContextFactory<WebTestContext>
    {   
        public ContextFactory()
        {
            
        }

        public WebTestContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "."))
            .AddJsonFile("appsettings.json")
            .Build();

            var optionsBuilder = new DbContextOptionsBuilder<WebTestContext>();
            return new WebTestContext(optionsBuilder.Options, configuration);
        }
    }
}
