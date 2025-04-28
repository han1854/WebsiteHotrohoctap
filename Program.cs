using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Hubs;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;
using WebsiteHotrohoctap.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<JDoodleService>();

builder.Services.AddSignalR();

builder.Services.AddScoped<ICourseRepository, EFCourseRepository>();
builder.Services.AddScoped<ILessonRepository, EFLessonRepository>();
builder.Services.AddScoped<ILessonContentRepository, EFLessonContentRepository>();
builder.Services.AddScoped<IExamRepository, EFExamRepository>();
builder.Services.AddScoped<IExamContentRepository, EFExamContentRepository>();
builder.Services.AddScoped<IExamResultRepository, EFExamResultRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();


// Add services to the container.
builder.Services.AddControllersWithViews();
//
builder.Services.AddHttpClient("IdeoneClient", client =>
{
    client.BaseAddress = new Uri("https://ideone.com/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddIdentity<User, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    string[] roles = { SD.Role_Admin, SD.Role_Customer };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Tạo tài khoản Admin mặc định
    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin123.";

    var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (existingAdmin == null)
    {
        var adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            UserType = SD.Role_Admin
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
