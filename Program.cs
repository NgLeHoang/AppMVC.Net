using APPMVC.NET.Models;
using APPMVC.NET.ExtendMethods;
using APPMVC.NET.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using APPMVC.NET.Data;
using System.Net.NetworkInformation;
using Microsoft.Extensions.FileProviders;
using Org.BouncyCastle.Asn1.Cms;
using APPMVC.NET.Menu;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add services to work with session
builder.Services.AddDistributedMemoryCache(); // Register services save cache to the container
builder.Services.AddSession(configure => {
    configure.Cookie.Name = "appmvc"; // Name of session - use at Browser (Cookie)
    configure.IdleTimeout = new TimeSpan(0, 30, 0); // Time alive of Session
});

builder.Services.Configure<RazorViewEngineOptions>(options => {
    options.ViewLocationFormats.Add("/MyView/{1}/{0}.cshtml" + RazorViewEngine.ViewExtension);
});

// Add services to display planet
builder.Services.AddSingleton<PlanetServices>();

// Add services to work with Mail
builder.Services.AddOptions();
            var mailSetting = builder.Configuration.GetSection("MailSettings");
            builder.Services.Configure<MailSettings>(mailSetting);
            builder.Services.AddSingleton<IEmailSender, SendMailService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppMvcConnectionString"));
});

// Add services identity
builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
            {
                // Set Password
                options.Password.RequireDigit = false; // Not must have number
                options.Password.RequireLowercase = false; // Not must lower case
                options.Password.RequireNonAlphanumeric = false; // No special characters required
                options.Password.RequireUppercase = false; // Capital letters are not required
                options.Password.RequiredLength = 3; // Minimum number of characters for password
                options.Password.RequiredUniqueChars = 1; // Number of distinct characters

                // Config Lockout - lock user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lock 5 minuates
                options.Lockout.MaxFailedAccessAttempts = 5; // Fail 5 times is lock
                options.Lockout.AllowedForNewUsers = true;

                // Config User.
                options.User.AllowedUserNameCharacters = // User name characters
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email is the only one

                // Config sign in.
                options.SignIn.RequireConfirmedEmail = true;            // Configure email address validation (email must exist)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Verify phone number
                options.SignIn.RequireConfirmedAccount = true;

            });

builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login/";
                options.LogoutPath = "/logout/";
                options.AccessDeniedPath = "/khongduoctruycap.html";
            });

builder.Services.AddAuthentication()
            .AddGoogle(googleOptions =>
            {
                // Read information of Authentication:Google from appsettings.json
                var googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

                // Setting ClientID and ClientSecret for access API google
                googleOptions.ClientId = googleAuthNSection["ClientId"];
                googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                // Configure Url callback from Google 
                googleOptions.CallbackPath = "/dang-nhap-tu-google";
            });

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddAuthorization(options => {
    options.AddPolicy("ViewManageMenu", builder => {
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});

// Add services work with cart
builder.Services.AddTransient<CartService>();

// Add services work with admin sidebar
builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<AdminSidebarService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions(){
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
    ),
    RequestPath = "/contents"
});

app.UseSession();

app.AddStatusCodePage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
