using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services;
using TripVista.Services.Interfaces;
using TripVista.Services.Ml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SmartTripContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartTripConn")));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IOtpDAO, OtpDAO>();

builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IDestinationDAO, DestinationDAO>();
builder.Services.AddScoped<IPackageDAO, PackageDAO>();
builder.Services.AddScoped<IBookingDAO, BookingsDAO>();
builder.Services.AddScoped<IPollDAO, PollDAO>();
builder.Services.AddScoped<IReportingDAO, ReportingDAO>();
builder.Services.AddScoped<IBudgetModelTrainer, BudgetModelTrainer>();
builder.Services.AddSingleton<IBudgetEstimationService, MlBudgetEstimationService>();




builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Seed the default admin account if it doesn't exist yet.
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["AdminSeed:Email"];
    var adminPassword = config["AdminSeed:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var userDAO = scope.ServiceProvider.GetRequiredService<IUserDAO>();
        var existingAdmin = await userDAO.GetUserByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            await userDAO.CreateUserAsync(new Users
            {
                UserId = Guid.NewGuid(),
                FullName = config["AdminSeed:FullName"] ?? "Admin",
                Email = adminEmail,
                Password = adminPassword, // hashed inside CreateUserAsync
                Role = "admin",
                IsEmailVerified = true
            });
        }
    }
}

// Train the budget ML models on first run (skipped once the model files exist on disk).
using (var scope = app.Services.CreateScope())
{
    var trainer = scope.ServiceProvider.GetRequiredService<IBudgetModelTrainer>();
    await trainer.EnsureModelsTrainedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
