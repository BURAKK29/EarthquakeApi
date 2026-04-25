using EarthaquakeApplication.Queries;
using EarthaquakeInfrastructure.Service;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeController.Controllers
{
    [Route("api/afad")]
    [ApiController]//ControllerBase nin
    public class EarthquakeController : ControllerBase//bu sınıf web api için gereklidir.
    {
        private readonly EarthquakeService _earthquakeService;
        private readonly IMediator _mediator;
        public EarthquakeController(EarthquakeService earthquakeService, IMediator mediator)
        {
            _mediator = mediator;
            _earthquakeService = earthquakeService;
        }
        [HttpGet("earthquakes")]
        public async Task<IActionResult> SyncEarthquakes()//IActionResult:HTTP yanıtlarını temsil eder.(ok, error, not found...)
        {
            try
            {
                await _earthquakeService.SyncEarthquakesAsync();
                return Ok("deprem verileri geldi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "hata oluştu" + ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEarthquakes()
        {
            var result = await _mediator.Send(new GetAllEarthquakesQuery());
            return Ok(result);
        }
    }
}
