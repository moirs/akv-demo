using System.Security.Cryptography.X509Certificates;
using Azure.Identity;

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
                
                // TODO: delete following line - demo purposes only
                //ConfigureAzureKeyVault(builder);
            }
            else
            {
                // Only required when not working locally
                //ConfigureAzureKeyVault(builder);
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            //app.MapGet("/", (IConfiguration config) =>
            //    string.Join(
            //        Environment.NewLine,
            //        "SecretName (Name in Key Vault: 'SecretName')",
            //        @"Obtained from configuration with config[""SecretName""]",
            //        $"Value: {config["MSS-SECRET-1"]}",
            //        "",
            //        "Section:SecretName (Name in Key Vault: 'Section--SecretName')",
            //        @"Obtained from configuration with config[""Section:SecretName""]",
            //        $"Value: {config["Section:SecretName"]}",
            //        "",
            //        "Section:SecretName (Name in Key Vault: 'Section--SecretName')",
            //        @"Obtained from configuration with config.GetSection(""Section"")[""SecretName""]",
            //        $"Value: {config.GetSection("Section")["SecretName"]}"));

            app.Run();
        }

        private static void ConfigureAzureKeyVault(WebApplicationBuilder builder)
        {
            using var x509Store = new X509Store(StoreLocation.CurrentUser);

            x509Store.Open(OpenFlags.ReadOnly);

            var x509Certificate = x509Store.Certificates
                .Find(
                    X509FindType.FindByThumbprint,
                    builder.Configuration["AzureADCertThumbprint"],
                    validOnly: false)
                .OfType<X509Certificate2>()
                .Single();

            builder.Configuration.AddAzureKeyVault(
                new Uri(builder.Configuration["KeyVaultUrl"]),
                new ClientCertificateCredential(
                    builder.Configuration["AzureADApplicationId"],
                    builder.Configuration["AzureADDirectoryId"],
                    x509Certificate));

            // Managed Identity - this only works when hosted in Azure Service - hence it does not work locally
            //var identity = new ManagedIdentityCredential(clientId: "c941f324-68da-4f7c-97da-7f0c6489bb35");
            //var identity = new DefaultAzureCredential();
            //builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["KeyVaultUrl"]), identity);
        }
    }
}