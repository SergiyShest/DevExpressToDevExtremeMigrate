using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using Configuration;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("autofac.json")
    .Build();

// Настройка Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()) // Использование Autofac в качестве DI контейнера
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        // Загрузка модуля конфигурации Autofac
        var module = new ConfigurationModule(config);
        containerBuilder.RegisterModule(module);
    });
AuthenticationConfigurer.ConfigureAuthentication(builder);//Выбор типа авторизации в зависимости от настроек


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyAllowSpecificOrigins",
                      policy =>
                      {
                          policy.WithOrigins("*");
                      });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.WebHost.UseKestrel(options =>
 {
     options.AllowSynchronousIO = true;
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
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "Common",
      pattern: "{area:exists}/{controller=*Journal}/{action=Index}/{id?}"
    );
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
