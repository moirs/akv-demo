using System.Security.Cryptography.X509Certificates;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace WebApiAzureKeyVault
{
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Demo only - this line would not be here in the IsDevelopment block it would only be in the else block below
                ConfigureAzureKeyVault(builder);
            }
            else
            {
                // Only required when not working locally
                //ConfigureAzureKeyVault(builder);
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        private static void ConfigureAzureKeyVault(WebApplicationBuilder builder)
        {
            var x509Certificate = GetCertificateFromStoreToAuthenticateWithAzureKeyVault(builder);

            // NOTE: the reload interval allows for AKV values to be refreshed at runtime (typically you would not have this and load secrets once at startup)
            builder.Configuration.AddAzureKeyVault(
                new Uri(builder.Configuration["KeyVaultUrl"]),
                new ClientCertificateCredential(
                    builder.Configuration["AzureADApplicationId"],
                    builder.Configuration["AzureADDirectoryId"],
                    x509Certificate), new AzureKeyVaultConfigurationOptions{ ReloadInterval = TimeSpan.FromSeconds(10)});

            // Managed Identity - this only works when hosted in Azure Service - hence it does not work locally
            //var identity = new ManagedIdentityCredential(clientId: "c941f324-68da-4f7c-97da-7f0c6489bb35");
            //var identity = new DefaultAzureCredential();
            //builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["KeyVaultUrl"]), identity);
        }

        private static X509Certificate2 GetCertificateFromStoreToAuthenticateWithAzureKeyVault(WebApplicationBuilder builder)
        {
            using var x509Store = new X509Store(StoreLocation.CurrentUser);

            x509Store.Open(OpenFlags.ReadOnly);

            return x509Store.Certificates
                .Find(
                    X509FindType.FindByThumbprint,
                    builder.Configuration["AzureADCertThumbprint"],
                    validOnly: false)
                .OfType<X509Certificate2>()
                .Single();
        }
    }
}