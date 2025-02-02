using Jwt.Auth.API.Data;
using Jwt.Auth.API.Services;
using Jwt.Auth.API.Shared.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyWorldDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyWorldDbConnection"));
});

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddCors(option => 
{
    option.AddDefaultPolicy(
        Policy =>
        {
            Policy.AllowAnyOrigin();
        }
    );
});

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection(nameof(TokenSettings)));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenSettings = builder.Configuration.GetSection(nameof(TokenSettings)).Get<TokenSettings>();
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = tokenSettings.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithMethods("GET, PATCH, DELETE, PUT, POST, OPTIONS"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();


