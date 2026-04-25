using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using EarthaquakeApplication.Queries;
using EarthaquakeInfrastructure.Data;
using EarthaquakeInfrastructure.Kafka.Message;
using EarthaquakeInfrastructure.Kafka.Producer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace EarthaquakeInfrastructure.Service
{
    public class EarthquakeService : EarthaquakeApplication.Interfaces.IEarthquakeService
    {
        private readonly IAfadClientService _afadClientService;
        private readonly IEarthquakeRepository _earthquakeRepository;
        private readonly IKafkaEarthquakeProducer _kafkaproducer;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext;

        public EarthquakeService(
            IAfadClientService afadClientService,
            IEarthquakeRepository earthquakeRepository,
            IKafkaEarthquakeProducer kafkaproducer,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext)
        {
            _afadClientService = afadClientService;
            _earthquakeRepository = earthquakeRepository;
            _kafkaproducer = kafkaproducer;
            _emailSender = emailSender;
            _dbContext = dbContext;
        }
        public async Task SyncEarthquakesAsync()
        {
            Console.WriteLine("[INFO] Sync işlemi başlatıldı");

            // GERÇEK AFAD API'sinden veri çeken orijinal satırı aktif hale getiriyoruz:
            var quakes = await _afadClientService.GetEarthquakesAsync();

            Console.WriteLine($"[DEBUG]{quakes.Count} adet deprem kaydı alındı.");

            if (quakes.Count > 0)
            {
                var newEarthquakes = new List<EarthquakeModel>();

                foreach (var quake in quakes)
                {
                    var exists = await _earthquakeRepository.CheckIfExistAsync(quake.EventID);
                    if (!exists)
                    {
                        newEarthquakes.Add(quake);
                    }
                }

                if (newEarthquakes.Any())
                {
                    await _earthquakeRepository.SaveManyAsync(newEarthquakes);
                    Console.WriteLine($"[INFO] {newEarthquakes.Count} adet yeni deprem kaydı veritabanına kaydedildi.");

                    foreach (var earthquake in newEarthquakes)
                    {
                        if (float.Parse(earthquake.Magnitude) >= 3.5)
                        {
                            await SendEarthquakeNotifications(earthquake);
                        }

                        var message = new EarthquakeMessage
                        {
                            EventID = earthquake.EventID,
                            Date = earthquake.Date,
                            Location = earthquake.Location,
                            Magnitude = earthquake.Magnitude,
                            // Kafka mesajına District de eklenmek istenirse EarthquakeMessage sınıfına eklenmeli
                        };
                        await _kafkaproducer.ProduceAsync(message);
                        Console.WriteLine($"[KAFKA] Gönderildi: {earthquake.EventID}");
                    }
                }
                else
                {
                    Console.WriteLine("[INFO] Veritabanına kaydedilecek yeni deprem verisi bulunamadı.");
                }
            }
            else
            {
                Console.WriteLine("[INFO] AFAD API'den deprem verisi alınamadı veya boş döndü."); // Mesajı güncelledik
            }
        }

        public async Task SaveFromKafkaAsync(EarthquakeModel earthquake)
        {
            var existing = await _earthquakeRepository.CheckIfExistAsync(earthquake.EventID);
            if (!existing)
            {
                await _earthquakeRepository.SaveManyAsync(new List<EarthquakeModel> { earthquake });
                Console.WriteLine($"[KAFKA - DB] Yeni deprem kaydı veritabanına eklendi:{earthquake.EventID}");
            }
            else
            {
                Console.WriteLine($"[KAFKA-DB] Deprem zaten kayıtlı:{earthquake.EventID} ");
            }
        }

        // Deprem bildirim maili gönderen özel metot
        private async Task SendEarthquakeNotifications(EarthquakeModel earthquake)
        {
            Console.WriteLine($"Deprem bildirimi için ilgili kullanıcılar aranıyor: {earthquake.Location} / {earthquake.Province}");

            // AFAD'dan gelen Location örn: "ISTANBUL (ANADOLU)" veya Province: "İstanbul"
            // Kullanıcının girdiği Province ile case-insensitive, kısmi eşleşme yapıyoruz
            var afadLocation = (earthquake.Location ?? "").ToLowerInvariant();
            var afadProvince = (earthquake.Province ?? "").ToLowerInvariant();

            var allFamilyMembers = await _dbContext.FamilyMembers
                .Where(fm => fm.Province != null)
                .Select(fm => new { fm.Province, fm.ApplicationUserId })
                .ToListAsync();

            var userIdsToNotify = allFamilyMembers
                .Where(fm =>
                {
                    var userProvince = fm.Province!.ToLowerInvariant();
                    return afadLocation.Contains(userProvince)
                        || afadProvince.Contains(userProvince)
                        || userProvince.Contains(afadLocation)
                        || userProvince.Contains(afadProvince);
                })
                .Select(fm => fm.ApplicationUserId)
                .Distinct()
                .ToList();

            if (!userIdsToNotify.Any())
            {
                Console.WriteLine($"Bildirim gönderilecek kullanıcı bulunamadı: {earthquake.Location}");
                return;
            }

            var usersToNotify = await _dbContext.Users
                .Where(u => userIdsToNotify.Contains(u.Id))
                .ToListAsync();

            if (!usersToNotify.Any())
            {
                Console.WriteLine($"Bulunan kullanıcı ID'leri için aktif kullanıcı bulunamadı: {earthquake.Location}");
                return;
            }

            foreach (var user in usersToNotify)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    var email = user.Email;
                    var subject = "Earthquake Alert: Important Information About Your Area!";
                    var body = $@"
                    <html>
                    <head>
                        <title>Earthquake Alert</title>
                    </head>
                    <body>
                        <p>Dear {user.UserName ?? user.Email},</p>
                        <p>An earthquake has been detected near your registered family member's location:</p>
                        <ul>
                            <li><strong>Location:</strong> {earthquake.Location}</li>
                            <li><strong>District:</strong> {earthquake.District}</li>
                            <li><strong>Magnitude:</strong> {earthquake.Magnitude}</li> 
                            <li><strong>Depth:</strong> {earthquake.Depth} km</li>
                            <li><strong>Date & Time:</strong> {earthquake.Date.ToString("dd.MM.yyyy HH:mm:ss")}</li>
                        </ul>
                        <p>Please stay informed and take necessary precautions if your family members are in the affected area.</p>
                        <p>You can check the latest earthquake data on our website for more details.</p>
                        <p>Best regards,</p>
                        <p>Your Earthquake Alert System</p>
                    </body>
                    </html>";

                    try
                    {
                        await _emailSender.SendEmailAsync(email, subject, body);
                        Console.WriteLine($"Email sent to {email} for earthquake in {earthquake.Location} (Magnitude: {earthquake.Magnitude}).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending email to {email} for earthquake in {earthquake.Location}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: User {user.UserName} ({user.Id}) has no email registered, skipping notification.");
                }
            }
        }
    }
}
