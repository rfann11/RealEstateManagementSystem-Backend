// REMS.Backend/Program.cs

// 1. Gerekli Using Direktifleri (Dosyan�n en ba��na ekleyin)
using REMS.Backend.Data; // ApplicationDbContext i�in
using Microsoft.EntityFrameworkCore; // DbContextOptionsBuilder ve UseNpgsql i�in
using Npgsql.EntityFrameworkCore.PostgreSQL; // PostgreSQL veritaban� sa�lay�c�s� i�in

using REMS.Backend.Interfaces; // IIlService, IAuthService i�in
using REMS.Backend.Services;   // IlService, AuthService i�in
using REMS.Backend.Helpers;   // JwtService, PasswordHasher i�in

// JWT Kimlik Do�rulama ve Yetkilendirme i�in gerekli using'ler
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims; // <<<< BU SATIR B�YLE OLMALI! (ClaimTypes i�in) >>>>
using Microsoft.OpenApi.Models; // <<<< BU SATIR AYRI OLMALI (Swagger i�in) >>>>


var builder = WebApplication.CreateBuilder(args);

// 2. Servislerin Ba��ml�l�k Enjeksiyonu Konteynerine Kayd� (builder.Services.Add...)

// 2.1. Veritaban� Ba�lam�n� Kaydet (En Temel Ba��ml�l�k)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2.2. Yard�mc� Servisleri Kaydet (Di�er servislerin ba��ml�l��� olabilecekler)
builder.Services.AddScoped<JwtService>(); // AuthService'in ba��ml�l��� oldu�u i�in �nce kaydedilmeli

// 2.3. �� Mant��� Servislerini Kaydet (Aray�zleri somut s�n�flara ba�la)
builder.Services.AddScoped<IIlService, IlService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // AuthController'�n ba��ml�l��� oldu�u i�in kaydedilmeli
builder.Services.AddScoped<IUserService, UserService>();

// 2.4. API Controller'lar� ve Swagger'� Kaydet
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger/OpenAPI ke�fi i�in gerekli
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "REMS API", Version = "v1" });

    // JWT (Bearer) kimlik do�rulamas�n� etkinle�tir
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// 2.5. JWT Kimlik Do�rulamas�n� Yap�land�r ve Kaydet
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKeyBytes = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]); // BURAYI EKLEY�N
        System.Diagnostics.Debug.WriteLine($"JWT Key Bytes Length (Validate): {jwtKeyBytes.Length}"); // BURAYI EKLEY�N
        Console.WriteLine($"JWT Key Bytes Length (Validate): {jwtKeyBytes.Length}"); // BURAYI EKLEY�N


        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Token'�n imzaland��� anahtar� do�rula
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])), // appsettings.json'dan secret key'i al
            ValidateIssuer = false, // Geli�tirme i�in false (canl�da true olmal�, token'� veren sunucuyu do�rular)
            ValidateAudience = false // Geli�tirme i�in false (canl�da true olmal�, token'�n kimin i�in oldu�unu do�rular)
        };


        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Token ba�ar�yla do�ruland���nda buras� �al���r
                System.Diagnostics.Debug.WriteLine("---------- TOKEN BA�ARIYLA DO�RULANDI! ----------");
                System.Diagnostics.Debug.WriteLine($"Kullan�c�: {context.Principal.Identity.Name}");
                foreach (var claim in context.Principal.Claims)
                {
                    System.Diagnostics.Debug.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Token do�rulama ba�ar�s�z oldu�unda buras� �al���r
                System.Diagnostics.Debug.WriteLine("---------- TOKEN DO�RULAMA BA�ARISIZ OLDU! ----------");
                System.Diagnostics.Debug.WriteLine($"Hata Mesaj�: {context.Exception.Message}");
                if (context.Exception.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"�� Hata: {context.Exception.InnerException.Message}");
                }
                return Task.CompletedTask;
            }
        };
    });

// 2.6. Yetkilendirme Politikalar�n� Kaydet (Rol bazl� yetkilendirme i�in)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminRole", policy => policy.RequireRole("Admin")); // Sadece "Admin" rol�ndekiler eri�ebilir
    options.AddPolicy("UserRole", policy => policy.RequireRole("User", "Admin")); // "User" veya "Admin" rol�ndekiler eri�ebilir
});


var app = builder.Build(); // Uygulama yap�s�n� olu�tur

// 3. Middleware S�ralamas� (app.Use... sat�rlar�) - BU SIRALAMA �OK KR�T�KT�R!

// Geli�tirme ortam�nda Swagger'� etkinle�tir
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geli�tirme s�ras�nda detayl� hata sayfalar�n� g�sterir
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinle�tirir
    app.UseSwaggerUI(); // Swagger UI aray�z�n� etkinle�tirir
}
else
{
    // �retim ortam� i�in hata y�netimi ve g�venlik ayarlar�
    // app.UseExceptionHandler("/Error"); // �retim hatalar� i�in �zel sayfa
    // app.UseHsts(); // HSTS (HTTP Strict Transport Security)
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'ye y�nlendirir

// Kimlik do�rulama ve yetkilendirme middleware'lerini do�ru s�rada ekle
// �NCE kimlik do�rulan�r, SONRA yetki kontrol edilir!
app.UseAuthentication(); // Gelen isteklerde kimlik bilgilerini (JWT gibi) do�rular
app.UseAuthorization();  // Do�rulanm�� kimli�e g�re kullan�c�n�n belirli kaynaklara eri�im yetkisini kontrol eder

app.MapControllers(); // API Controller'lar� i�in rota e�le�tirmesini etkinle�tirir

app.Run(); // Uygulamay� ba�lat