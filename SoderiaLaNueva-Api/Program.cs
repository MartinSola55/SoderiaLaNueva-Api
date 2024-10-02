using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.DAL.Seeding;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Services;
using System.Text;
using SoderiaLaNueva_Api.Security;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.Constants;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APIContextConnection") ?? throw new InvalidOperationException("Connection string 'APIContextConnection' not found.");

// Add services to the container.
ServiceContainer.AddServices(builder.Services);
builder.Services.AddScoped<ISeeder, Seeder>();

// Add Identity and Entity Framework services
builder.Services.AddIdentity<ApiUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<APIContext>();
builder.Services.AddDbContextFactory<APIContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Roles authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, RolesHandler>();

// Configure JWT authentication
var key = builder.Configuration["JWT:Key"];
if (string.IsNullOrEmpty(key))
    throw new InvalidOperationException("JWT key not found.");

builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Admin, policy => policy.Requirements.Add(new AuthorizeRolesAttribute(Roles.Admin)));
    options.AddPolicy(Policies.Dealer, policy => policy.Requirements.Add(new AuthorizeRolesAttribute(Roles.Dealer)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

Seed();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

void Seed()
{
    using var scope = app.Services.CreateScope();
    var dbSeeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
    dbSeeder.Seed();
}