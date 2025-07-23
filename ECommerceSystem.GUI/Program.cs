using ECommerceSystem.GUI.Apis;
using ECommerceSystem.GUI.Controllers;

// Nếu AuthRetryHandler nằm trong namespace này
using ECommerceSystem.GUI.Services;
using ECommerceSystem.GUI.Services.ECommerceSystem.GUI.Handlers;
using ECommerceSystem.Shared.Constants;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Refit;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddTransient<AuthRetryHandler>();
ConfigureRefit(builder.Services);

// Cấu hình xác thực
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "MyCookieAuth"; // Sử dụng scheme tùy chỉnh
    options.DefaultChallengeScheme = "MyCookieAuth";
})
.AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Thay bằng issuer từ API của bạn
        ValidAudience = builder.Configuration["Jwt:Audience"], // Thay bằng audience từ API
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Thay bằng key từ API
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Lấy token từ cookie thay vì header Authorization
            context.Token = context.Request.Cookies["AuthToken"];
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Thêm middleware xác thực
app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 403)
    {
        response.Redirect("/Home/AccessDenied"); // ✅ Trang bạn tự thiết kế
    }
    else if (response.StatusCode == 401)
    {
        response.Redirect("/Auth/Login?returnUrl=" + context.HttpContext.Request.Path);
    }

    return Task.CompletedTask;
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ConfigureRefit
static void ConfigureRefit(IServiceCollection services)
{
    services.AddRefitClient<IAuthApi>()
        .AddHttpMessageHandler<AuthRetryHandler>()
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IOrderApi>()
       .AddHttpMessageHandler<AuthRetryHandler>()
       .ConfigureHttpClient(SetHttpClient); 

    services.AddRefitClient<ICategoryApi>()
        .AddHttpMessageHandler<AuthRetryHandler>()
        .ConfigureHttpClient(SetHttpClient);

    services.AddRefitClient<IProductApi>()
       .AddHttpMessageHandler<AuthRetryHandler>()
       .ConfigureHttpClient(SetHttpClient);
    services.AddRefitClient<ICartApi>()
       .AddHttpMessageHandler<AuthRetryHandler>()
       .ConfigureHttpClient(SetHttpClient);
    services.AddRefitClient<IUserApi>()
       .AddHttpMessageHandler<AuthRetryHandler>()
       .ConfigureHttpClient(SetHttpClient);
    services.AddRefitClient<IAdminApi>()
      .AddHttpMessageHandler<AuthRetryHandler>()
      .ConfigureHttpClient(SetHttpClient);

    //services.AddRefitClient<IAuthApi>()
    //   .AddHttpMessageHandler<AuthRetryHandler>()
    //   .ConfigureHttpClient(SetHttpClient);
    void SetHttpClient(HttpClient httpClient)
    {
        httpClient.BaseAddress = new Uri(AppConstants.ApiBaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(20);
        Console.WriteLine($"[Refit] BaseAddress set to: {httpClient.BaseAddress}");
    }
  
    
}