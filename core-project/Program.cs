using Library.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Services;
using Library.Hubs;
using Library.Middleware;
using Library.Services;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ... (שאר ההגדרות של ה-Services נשארות אותו דבר) ...
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// הוסף SignalR
builder.Services.AddSignalR();

// הוסף Log Queue ו-Background Service
var logChannel = Channel.CreateUnbounded<LogEntry>();
builder.Services.AddSingleton(logChannel);
builder.Services.AddHostedService<LogWriterBackgroundService>();

builder.Services.AddSingleton<ILibraryBookService, LibraryBookService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "אנא הכנס טוקן עם המילה Bearer לפניו",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = LibraryTokenService.GetTokenValidationParameters();
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    
    // הוסף scopes כדי לקבל גישה לפרטים נוספים של המשתמש
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    // Get user picture from Google
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            try
            {
                // Fetch user info from Google
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                request.Headers.Add("Authorization", $"Bearer {context.AccessToken}");
                
                var response = await context.Backchannel.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userInfo = System.Text.Json.JsonDocument.Parse(content);
                    
                    // Extract picture URL
                    if (userInfo.RootElement.TryGetProperty("picture", out var pictureElement))
                    {
                        var pictureUrl = pictureElement.GetString();
                        Console.WriteLine($"Google Picture URL: {pictureUrl}");
                        if (!string.IsNullOrEmpty(pictureUrl))
                        {
                            // Add picture as a claim
                            context.Identity?.AddClaim(new System.Security.Claims.Claim("picture", pictureUrl));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Google picture: {ex.Message}");
            }
        }
    };
});


var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "login.html", "index.html" }
});
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

// הוסף Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

// מטפל את LibraryHub עבור התראות בזמן אמת
app.MapHub<LibraryHub>("/libraryHub");

app.Run();