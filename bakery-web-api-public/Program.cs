using bakery_web_api;
using bakery_web_api.Controllers.User;
using bakery_web_api.Interfaces.Admin;
using bakery_web_api.Interfaces.Common;
using bakery_web_api.Interfaces.User;
using bakery_web_api.Services.Admin;
using bakery_web_api.Services.Common;
using bakery_web_api.Services.User;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Configuration.AddJsonFile("appsettings.json");
Config.BindValuesFromConfigFile(builder);

var configuration = builder.Configuration.GetConnectionString("bakeryDbCon");

builder.Services.AddDbContext<BakeryDbContext>(options =>
    options.UseMySql(configuration, ServerVersion.AutoDetect(configuration)));

// builder add scoped services
//Admin
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminOrderingPageService, AdminOrderingPageService>();
builder.Services.AddScoped<IAdminOrdersService, AdminOrdersService>();
builder.Services.AddScoped<IAdminMainPageService, AdminMainPageService>();
builder.Services.AddScoped<IAdminProductionService, AdminProductionService>();
builder.Services.AddScoped<IInstagramApi, InstagramApiService>();
builder.Services.AddScoped<IContactForm, ContactFormService>();
builder.Services.AddScoped<IAdminProductsService>(serviceProvider =>
{
    // Blob configuration
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("blobContainerCon");
    var dbContext = serviceProvider.GetRequiredService<BakeryDbContext>();

    return new AdminProductsService(connectionString, dbContext, configuration);
});
//Common
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserVerifyService, UserVerifyService>();
builder.Services.AddScoped<IOrderingService, OrderingService>();
//User
builder.Services.AddScoped<IUserPanelService, UserPanelService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IForgotPassowrdService, ForgotPasswordService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
