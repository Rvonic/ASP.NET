using Microsoft.EntityFrameworkCore;
using PrviLabos.DAL;
using PrviLabos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PrviLabosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PrviLabosDbContext")));
builder.Services.AddScoped<DropoffSupportService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PrviLabosDbContext>();

    await context.Database.MigrateAsync();
    await SupportDataSeeder.SeedAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
