using BasketballLiveScore.Data;
using BasketballLiveScore.Hubs;
using BasketballLiveScore.Repositories.Implementations;
using BasketballLiveScore.Repositories.Interfaces;
using BasketballLiveScore.Services;
using BasketballLiveScore.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURATION DES SERVICES ====================

// Configuration de la base de données
builder.Services.AddDbContext<BasketballDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration du Unit of Work et des Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configuration du service de hashage des mots de passe
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();

// Configuration de tous les Services avec leurs dépendances
builder.Services.AddScoped<IGameActionService, GameActionService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<IDeleteService, DeleteService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<ILiveScoreService, LiveScoreService>();

// Configuration de l'authentification JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "VotreCleSecreteTresLonguePourLaSecuriteJWT12345";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BasketballLiveScore";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BasketballLiveScore.Users";

// S'assurer que la clé est assez longue
if (jwtKey.Length < 32)
{
    jwtKey = "CleDeSecuriteParDefautPourBasketballLiveScore2024!";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Configuration pour SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Si la requête est pour notre hub SignalR
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/livescorehub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configuration de l'autorisation
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EncoderOnly", policy =>
        policy.RequireRole("Administrator", "Encoder"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrator"));
});

// Configuration des contrôleurs avec support JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configuration de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Basketball LiveScore API",
        Version = "v1",
        Description = "API pour la gestion des matchs de basketball en temps réel"
    });

    // Configuration pour l'authentification JWT dans Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuration de SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configuration du HttpClient pour communiquer avec l'API
builder.Services.AddHttpClient("BasketballAPI", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configuration des sessions pour stocker le JWT
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".BasketballLiveScore.Session";
});

// Ajout de Razor Pages avec options
builder.Services.AddRazorPages(options =>
{
    // Pages qui nécessitent une authentification
    options.Conventions.AuthorizePage("/Dashboard");
    options.Conventions.AuthorizePage("/MatchSetup");
    options.Conventions.AuthorizePage("/LiveScore");
    options.Conventions.AuthorizePage("/Teams");
    options.Conventions.AuthorizePage("/Players");

    // Pages publiques
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Register");
});

// Configuration CORS pour SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "https://localhost:5001",
                "http://localhost:7000",
                "https://localhost:7001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configuration du logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
}

// ==================== CONSTRUCTION DE L'APPLICATION ====================

var app = builder.Build();

// ==================== PIPELINE DE L'APPLICATION ====================

// Gestion des erreurs
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basketball LiveScore API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Redirection HTTPS
app.UseHttpsRedirection();

// Fichiers statiques
app.UseStaticFiles();

// Routing
app.UseRouting();

// CORS (doit être avant Authentication et Authorization)
app.UseCors("SignalRPolicy");

// Session (doit être avant Authentication)
app.UseSession();

// Authentication et Authorization
app.UseAuthentication();
app.UseAuthorization();

// Mapping des endpoints
app.MapControllers();
app.MapRazorPages();
app.MapHub<LiveScoreHub>("/livescorehub");

// Page par défaut
app.MapGet("/", context =>
{
    context.Response.Redirect("/Index");
    return Task.CompletedTask;
});

// ==================== INITIALISATION DE LA BASE DE DONNÉES ====================

// Créer la base de données et initialiser les données
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BasketballDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasherService>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Appliquer les migrations ou créer la base
        if (app.Environment.IsDevelopment())
        {
            context.Database.EnsureCreated();
        }
        else
        {
            context.Database.Migrate();
        }

        // Seed des données initiales
        if (!context.Users.Any())
        {
            logger.LogInformation("Initialisation des utilisateurs par défaut");

            // Créer un administrateur par défaut
            var adminPassword = passwordHasher.HashPassword("Admin123!");
            context.Users.Add(new BasketballLiveScore.Models.User
            {
                Username = "admin",
                FirstName = "Admin",
                LastName = "System",
                Email = "admin@basketballlive.com",
                Password = adminPassword,
                Role = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Créer un encodeur par défaut
            var encoderPassword = passwordHasher.HashPassword("Encoder123!");
            context.Users.Add(new BasketballLiveScore.Models.User
            {
                Username = "encoder",
                FirstName = "Encoder",
                LastName = "Default",
                Email = "encoder@basketballlive.com",
                Password = encoderPassword,
                Role = "Encoder",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            // Créer un spectateur par défaut
            var viewerPassword = passwordHasher.HashPassword("Viewer123!");
            context.Users.Add(new BasketballLiveScore.Models.User
            {
                Username = "viewer",
                FirstName = "Viewer",
                LastName = "Default",
                Email = "viewer@basketballlive.com",
                Password = viewerPassword,
                Role = "Viewer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            context.SaveChanges();
            logger.LogInformation("Utilisateurs par défaut créés avec succès");
        }

        // Seed des équipes de test
        if (!context.Teams.Any())
        {
            logger.LogInformation("Initialisation des équipes de test");

            var team1 = new BasketballLiveScore.Models.Team
            {
                Name = "Lakers",
                City = "Los Angeles",
                Coach = "Darvin Ham",
                CreatedAt = DateTime.UtcNow
            };

            var team2 = new BasketballLiveScore.Models.Team
            {
                Name = "Warriors",
                City = "Golden State",
                Coach = "Steve Kerr",
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(team1, team2);
            context.SaveChanges();

            // Ajouter quelques joueurs de test
            var players = new[]
            {
                new BasketballLiveScore.Models.Player { FirstName = "LeBron", LastName = "James", JerseyNumber = 23, TeamId = team1.Id },
                new BasketballLiveScore.Models.Player { FirstName = "Anthony", LastName = "Davis", JerseyNumber = 3, TeamId = team1.Id },
                new BasketballLiveScore.Models.Player { FirstName = "Stephen", LastName = "Curry", JerseyNumber = 30, TeamId = team2.Id },
                new BasketballLiveScore.Models.Player { FirstName = "Klay", LastName = "Thompson", JerseyNumber = 11, TeamId = team2.Id }
            };

            context.Players.AddRange(players);
            context.SaveChanges();

            logger.LogInformation("Équipes et joueurs de test créés avec succès");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur est survenue lors de l'initialisation de la base de données");
    }
}

// ==================== DÉMARRAGE DE L'APPLICATION ====================

app.Run();