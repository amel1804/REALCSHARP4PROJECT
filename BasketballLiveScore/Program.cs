using BasketballLiveScore.Data;
using BasketballLiveScore.Repositories.Implementations;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données
builder.Services.AddDbContext<BasketballDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration du Unit of Work et des Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configuration des Services
builder.Services.AddScoped<IGameActionService, GameActionService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<IDeleteService, DeleteService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IUserService, UserService>();

// Configuration de l'authentification JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
            };
        });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** DÉPLACÉ ICI - AVANT builder.Build() ***
// Configuration du HttpClient pour communiquer avec l'API
builder.Services.AddHttpClient("BasketballAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configuration des sessions pour stocker le JWT
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Ajout de Razor Pages
builder.Services.AddRazorPages();
// *** FIN DES SERVICES DÉPLACÉS ***

var app = builder.Build(); // APRÈS CETTE LIGNE, PLUS DE builder.Services.AddXXX() !

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession(); // Session avant Authentication
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();