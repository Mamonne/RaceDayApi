using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayApi.Data;

namespace RaceDayApi.Tests
{
    // Starts the API for testing, using a temporary in-memory database
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(this._databaseName));
            });
        }
    }
}