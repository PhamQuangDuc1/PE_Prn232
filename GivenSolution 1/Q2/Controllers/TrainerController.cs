using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Q2.Models;

namespace Q2.Controllers
{
    public class TrainerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TrainerController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = (configuration["GivenAPIBaseUrl"] ?? "http://localhost:5100").TrimEnd('/');
        }

        [HttpGet("/Trainer")]
        public async Task<IActionResult> Index([FromQuery] int specId = 0, [FromQuery] string? trainerName = null)
        {
            var specsUrl = _baseUrl + "/api/specializations";
            var specsResponse = await _httpClient.GetStringAsync(specsUrl);
            var specs = JsonConvert.DeserializeObject<List<SpecializationDTO>>(specsResponse) ?? new List<SpecializationDTO>();

            var searchUrl = _baseUrl + "/api/trainers/search?specId=" + specId + "&trainerName=" + Uri.EscapeDataString(trainerName ?? "");
            var trainersResponse = await _httpClient.GetStringAsync(searchUrl);
            var trainers = JsonConvert.DeserializeObject<List<TrainerSearchDTO>>(trainersResponse) ?? new List<TrainerSearchDTO>();

            ViewBag.Specializations = specs;
            ViewBag.SelectedSpecId = specId;
            ViewBag.TrainerName = trainerName ?? "";

            return View(trainers);
        }

        [HttpGet("/Trainer/Analyze/{id}")]
        public async Task<IActionResult> Analyze(int id)
        {
            var detailUrl = _baseUrl + "/api/trainers/" + id;
            var response = await _httpClient.GetAsync(detailUrl);
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var trainer = JsonConvert.DeserializeObject<TrainerDetailDTO>(json);
            return View(trainer);
        }
    }
}
