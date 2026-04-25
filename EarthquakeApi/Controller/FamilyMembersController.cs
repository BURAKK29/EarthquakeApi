using EarthaquakeApplication.Entities;
using EarthaquakeInfrastructure.Data;
using EarthaquakeApplication.FilterViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EarthquakeApi.Controllers
{
    public class FamilyMembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public FamilyMembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var familyMembers = await _context.FamilyMembers
                                              .Where(fm => fm.ApplicationUserId == currentUser.Id)
                                              .ToListAsync();

            var membersForMap = familyMembers.Select(m => new
            {
                m.FirstName,
                m.LastName,
                m.Province,
                m.Country,
                m.Latitude,
                m.Longitude
            }).ToList(); // ToList() ile anonim tip listesi oluşuyor

            TempData["MembersForMapJson"] = System.Text.Json.JsonSerializer.Serialize(membersForMap);


            var viewModel = new FamilyMembersViewModel
            {
                ExistingMembers = familyMembers,
                NewMember = new FamilyMember()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(FamilyMembersViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            ModelState.Remove("ExistingMembers"); //
            ModelState.Remove("StatusMessage"); //

            model.NewMember.ApplicationUserId = currentUser.Id; //

            if (ModelState.IsValid)
            {
                var query = $"{model.NewMember.Province}, {model.NewMember.Country}";
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "EarthquakeApiProject/1.0 (email@example.com)");

                try
                {
                    var response = await httpClient.GetAsync($"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query)}&limit=1");
                    response.EnsureSuccessStatusCode();

                    var jsonString = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<NominatimResult>>(jsonString);

                    if (results != null && results.Any())
                    {
                        model.NewMember.Latitude = double.Parse(results.First().Lat, System.Globalization.CultureInfo.InvariantCulture);
                        model.NewMember.Longitude = double.Parse(results.First().Lon, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        Console.WriteLine($"Nominatim sonuç döndürmedi: {query}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Nominatim API çağrısında hata: {ex.Message}");
                }
                catch (System.Text.Json.JsonException ex)
                {
                    Console.WriteLine($"Nominatim API yanıtı JSON ayrıştırma hatası: {ex.Message}");
                }

                try
                {
                    _context.FamilyMembers.Add(model.NewMember);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Aile üyesi başarıyla eklendi.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"Veritabanı kaydetme hatası: {ex.Message}"); //
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"İç hata: {ex.InnerException.Message}"); //
                    }
                    ModelState.AddModelError("", "Üye eklenirken bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            model.ExistingMembers = await _context.FamilyMembers
                                                  .Where(fm => fm.ApplicationUserId == currentUser.Id)
                                                  .ToListAsync();
            return View("Index", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // aile üyesi silme
            var familyMember = await _context.FamilyMembers
                                             .FirstOrDefaultAsync(m => m.Id == id && m.ApplicationUserId == currentUser.Id);

            if (familyMember == null)
            {
                return NotFound();
            }

            _context.FamilyMembers.Remove(familyMember);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Family member successfully deleted.";
            return RedirectToAction(nameof(Index));
        }
    }


    public class NominatimResult
    {
        [JsonProperty("lat")]
        public string Lat { get; set; }
        [JsonProperty("lon")]
        public string Lon { get; set; }
    }
}