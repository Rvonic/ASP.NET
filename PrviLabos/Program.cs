using PrviLabos.Data;
using PrviLabos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(_ => MockRepositorySet.CreateDefault());
builder.Services.AddSingleton(sp => sp.GetRequiredService<MockRepositorySet>().Context);
builder.Services.AddScoped<DropoffSupportService>();

var app = builder.Build();

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
