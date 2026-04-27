using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using EarthaquakeApplication.Queries;
using EarthaquakeInfrastructure.Database;
using EarthaquakeInfrastructure.Data;
using EarthaquakeInfrastructure.Kafka.Producer;
using EarthaquakeInfrastructure.Service;
using Hangfire;
using Hangfire.MemoryStorage;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. SERVİS EKLEME KISMI
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Scoped servisler
builder.Services.AddScoped<AfadClientService>();
builder.Services.AddScoped<IKafkaEarthquakeProducer, KafkaEarthquakeProducer>();
builder.Services.AddScoped<EarthquakeService>();
builder.Services.AddScoped<IEarthquakeRepository, EarthquakeRepository>();
builder.Services.AddHttpClient<IAfadClientService, AfadClientService>();
builder.Services.AddScoped<IEarthquakeService, EarthquakeService>();

// ✅ Hangfire servisleri
builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UseMemoryStorage();
});
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetAllEarthquakesQuery).Assembly));

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

// ✅ KafkaFlow yapılandırması
builder.Services.AddKafka(kafka =>
    kafka.UseConsoleLog() // opsiyonel ama log için faydalı
         .AddCluster(cluster =>
         {
             var cfg = builder.Configuration.GetSection("Kafka");
             var brokers = cfg.GetValue<string>("BootstrapServers");
             var topic = cfg.GetValue<string>("Topic");
             var groupId = cfg.GetValue<string>("GroupId");

             // Broker
             cluster.WithBrokers(brokers.Split(',', StringSplitOptions.RemoveEmptyEntries));

             // Producer
             cluster.AddProducer("earthquake-producer", producer =>
                 producer.DefaultTopic(topic)
                         .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
             );

             // Consumer
             cluster.AddConsumer(consumer =>
                 consumer.Topic(topic)
                         .WithGroupId(groupId ?? "earthquake-consumer-group") // config'de yoksa fallback
                         .WithBufferSize(100)
                         .WithWorkersCount(1)
                         .AddMiddlewares(m => m
                             .AddDeserializer<JsonCoreDeserializer>()
                             .AddTypedHandlers(h => h.AddHandler<
                                 EarthaquakeInfrastructure.Kafka.Consumer_Handler.EarthquakeMessageHandler>()))
             );
         })
);

// ✅ DbContext ekleme
builder.Services.AddDbContext<EarthaquakeInfrastructure.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity yapılandırması (ApplicationUser + Roller)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Mail onaylı hesap istiyorsan true
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<EarthaquakeInfrastructure.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// ✅ Email servisi (Gmail SMTP)
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// ✅ Razor Pages ve Authentication
builder.Services.AddRazorPages();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

// ✅ 2. UYGULAMA OLUŞTUR
var app = builder.Build();

// ✅ 3. ORTAM KONTROLLERİ ve MIDDLEWARE
// -------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHangfireDashboard();

app.MapGet("/login", async ctx => {
    ctx.Response.Redirect("/Identity/Account/Login");
});
app.MapGet("/register", async ctx => {
    ctx.Response.Redirect("/Identity/Account/Register");
});

RecurringJob.AddOrUpdate<EarthquakeService>(
    "sync-earthquakes-job",
    service => service.SyncEarthquakesAsync(),
    "*/5 * * * *"
);

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Sadece geliştirme ortamında Swagger'ı etkinleştirme
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EarthaquakeInfrastructure.Data.ApplicationDbContext>();
    db.Database.Migrate(); // ya da EnsureCreated()
}

app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=EarthquakeList}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers();

app.Run();