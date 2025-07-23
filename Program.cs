// REMS.Backend/Program.cs

// 1. Gerekli Using Direktifleri (Dosyanýn en baþýna ekleyin)
using REMS.Backend.Data; // ApplicationDbContext için
using Microsoft.EntityFrameworkCore; // DbContextOptionsBuilder ve UseNpgsql için
using Npgsql.EntityFrameworkCore.PostgreSQL; // PostgreSQL veritabaný saðlayýcýsý için

using REMS.Backend.Interfaces; // IIlService, IAuthService için
using REMS.Backend.Services;   // IlService, AuthService için
using REMS.Backend.Helpers;   // JwtService, PasswordHasher için

// JWT Kimlik Doðrulama ve Yetkilendirme için gerekli using'ler
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims; // <<<< BU SATIR BÖYLE OLMALI! (ClaimTypes için) >>>>
using Microsoft.OpenApi.Models; // <<<< BU SATIR AYRI OLMALI (Swagger için) >>>>


var builder = WebApplication.CreateBuilder(args);

// 2. Servislerin Baðýmlýlýk Enjeksiyonu Konteynerine Kaydý (builder.Services.Add...)

// 2.1. Veritabaný Baðlamýný Kaydet (En Temel Baðýmlýlýk)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2.2. Yardýmcý Servisleri Kaydet (Diðer servislerin baðýmlýlýðý olabilecekler)
builder.Services.AddScoped<JwtService>(); // AuthService'in baðýmlýlýðý olduðu için önce kaydedilmeli

// 2.3. Ýþ Mantýðý Servislerini Kaydet (Arayüzleri somut sýnýflara baðla)
builder.Services.AddScoped<IIlService, IlService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // AuthController'ýn baðýmlýlýðý olduðu için kaydedilmeli
builder.Services.AddScoped<IUserService, UserService>();

// 2.4. API Controller'larý ve Swagger'ý Kaydet
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger/OpenAPI keþfi için gerekli
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "REMS API", Version = "v1" });

    // JWT (Bearer) kimlik doðrulamasýný etkinleþtir
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

// 2.5. JWT Kimlik Doðrulamasýný Yapýlandýr ve Kaydet
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKeyBytes = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]); // BURAYI EKLEYÝN
        System.Diagnostics.Debug.WriteLine($"JWT Key Bytes Length (Validate): {jwtKeyBytes.Length}"); // BURAYI EKLEYÝN
        Console.WriteLine($"JWT Key Bytes Length (Validate): {jwtKeyBytes.Length}"); // BURAYI EKLEYÝN


        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Token'ýn imzalandýðý anahtarý doðrula
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])), // appsettings.json'dan secret key'i al
            ValidateIssuer = false, // Geliþtirme için false (canlýda true olmalý, token'ý veren sunucuyu doðrular)
            ValidateAudience = false // Geliþtirme için false (canlýda true olmalý, token'ýn kimin için olduðunu doðrular)
        };


        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Token baþarýyla doðrulandýðýnda burasý çalýþýr
                System.Diagnostics.Debug.WriteLine("---------- TOKEN BAÞARIYLA DOÐRULANDI! ----------");
                System.Diagnostics.Debug.WriteLine($"Kullanýcý: {context.Principal.Identity.Name}");
                foreach (var claim in context.Principal.Claims)
                {
                    System.Diagnostics.Debug.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Token doðrulama baþarýsýz olduðunda burasý çalýþýr
                System.Diagnostics.Debug.WriteLine("---------- TOKEN DOÐRULAMA BAÞARISIZ OLDU! ----------");
                System.Diagnostics.Debug.WriteLine($"Hata Mesajý: {context.Exception.Message}");
                if (context.Exception.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Ýç Hata: {context.Exception.InnerException.Message}");
                }
                return Task.CompletedTask;
            }
        };
    });

// 2.6. Yetkilendirme Politikalarýný Kaydet (Rol bazlý yetkilendirme için)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminRole", policy => policy.RequireRole("Admin")); // Sadece "Admin" rolündekiler eriþebilir
    options.AddPolicy("UserRole", policy => policy.RequireRole("User", "Admin")); // "User" veya "Admin" rolündekiler eriþebilir
});


var app = builder.Build(); // Uygulama yapýsýný oluþtur

// 3. Middleware Sýralamasý (app.Use... satýrlarý) - BU SIRALAMA ÇOK KRÝTÝKTÝR!

// Geliþtirme ortamýnda Swagger'ý etkinleþtir
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geliþtirme sýrasýnda detaylý hata sayfalarýný gösterir
    app.UseSwagger(); // Swagger JSON endpoint'ini etkinleþtirir
    app.UseSwaggerUI(); // Swagger UI arayüzünü etkinleþtirir
}
else
{
    // Üretim ortamý için hata yönetimi ve güvenlik ayarlarý
    // app.UseExceptionHandler("/Error"); // Üretim hatalarý için özel sayfa
    // app.UseHsts(); // HSTS (HTTP Strict Transport Security)
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'ye yönlendirir

// Kimlik doðrulama ve yetkilendirme middleware'lerini doðru sýrada ekle
// ÖNCE kimlik doðrulanýr, SONRA yetki kontrol edilir!
app.UseAuthentication(); // Gelen isteklerde kimlik bilgilerini (JWT gibi) doðrular
app.UseAuthorization();  // Doðrulanmýþ kimliðe göre kullanýcýnýn belirli kaynaklara eriþim yetkisini kontrol eder

app.MapControllers(); // API Controller'larý için rota eþleþtirmesini etkinleþtirir

app.Run(); // Uygulamayý baþlat