using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Q2.Models;

namespace Q2.Controllers
{
    public class MemberController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MemberController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = (configuration["GivenAPIBaseUrl"] ?? "http://localhost:5100").TrimEnd('/');
        }

        [HttpGet("/")]
        [HttpGet("/Member")]
        public async Task<IActionResult> Index([FromQuery] string? memberName = null, [FromQuery] string? bookingStatus = "All statuses")
        {
            var statusesUrl = _baseUrl + "/api/booking-statuses";
            var statusesResponse = await _httpClient.GetStringAsync(statusesUrl);
            var statuses = JsonConvert.DeserializeObject<List<string>>(statusesResponse) ?? new List<string>();

            var searchUrl = _baseUrl + "/api/members/search?memberName=" + Uri.EscapeDataString(memberName ?? "") + "&bookingStatus=" + Uri.EscapeDataString(bookingStatus ?? "All statuses");
            var membersResponse = await _httpClient.GetStringAsync(searchUrl);
            var members = JsonConvert.DeserializeObject<List<MemberSearchDTO>>(membersResponse) ?? new List<MemberSearchDTO>();

            ViewBag.Statuses = statuses;
            ViewBag.MemberName = memberName ?? "";
            ViewBag.SelectedStatus = string.IsNullOrEmpty(bookingStatus) ? "All statuses" : bookingStatus;

            return View(members);
        }

        [HttpGet("/Member/Analyze/{id}")]
        public async Task<IActionResult> Analyze(int id)
        {
            var detailUrl = _baseUrl + "/api/members/" + id;
            var response = await _httpClient.GetAsync(detailUrl);
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var member = JsonConvert.DeserializeObject<MemberDetailDTO>(json);
            return View(member);
        }
    }
}
