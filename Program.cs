using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Services;
using TripVista.Services.Interfaces;

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
builder.Services.AddScoped<IBudgetEstimationService, RuleBasedBudgetEstimationService>();




builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

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
