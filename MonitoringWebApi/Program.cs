using MonitoringWebApi.Services.BankConnection;
using MonitoringWebApi.Services.KeyProvider;

namespace MonitoringWebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IKeyProviderService, KeyProviderService>(sp =>
        {
            var privateKey = builder.Configuration.GetSection("Encryption").GetValue<string?>("PrivateKey") ?? "";
            return new KeyProviderService(privateKey);
        });

        builder.Services.AddScoped<IBankConnectionService, BankConnectionService>(sp =>
        {
            var host = builder.Configuration.GetSection("Server").GetValue<string>("Host") ?? throw new InvalidOperationException("Server host configuration is missing");
            var port = builder.Configuration.GetSection("Server").GetValue<int?>("Port") ?? throw new InvalidOperationException("Server port configuration is missing");
            var privateKey = builder.Configuration.GetSection("Encryption").GetValue<string?>("PrivateKey") ?? "";
            return new BankConnectionService(host, port, privateKey);
        });

        var app = builder.Build();

        app.UseWebSockets();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
