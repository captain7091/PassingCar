using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PassingCarApis.Hubs;
using PassingCarApis.Services;
using PassingCarApis.SQL;
using Rotativa.AspNetCore;
using System.Data;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddSignalR(o =>
{
    //o.EnableDetailedErrors = true;
});

// Add response compression for better performance
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add response caching for better performance
builder.Services.AddResponseCaching();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHubService, HubService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddHostedService<RemoveOldAdsService>();
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc($"v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = $"PassingCarApis", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = $"JWR Authorization",
        Name = $"Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = $"Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = $"Bearer"
                }
            },
                Array.Empty<string>()
        }
    });
});
//builder.Services.AddSignalRCore();
//builder.Services.AddScoped<AdsContextHub>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    })
    ;
WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();

    // Only enable Swagger in development to improve production startup time
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"PassingCar v1"));
}

// Enable response compression
app.UseResponseCompression();

// Enable response caching
app.UseResponseCaching();

SqlMapper.AddTypeMap(typeof(bool), DbType.String);
SqlMapper.AddTypeMap(typeof(Enum), DbType.String);

// Configure Dapper for better performance
SqlMapper.Settings.CommandTimeout = 15;

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

RotativaConfiguration.Setup(app.Environment.WebRootPath);

//app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<AdsHub>("/AdsHub");
app.MapHub<ChatHub>("/ChatHub");

//StartMigration.Run();

app.Run();
