using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Q2.Models;
using System.Text.Json;

namespace Q2.Pages.Doctor
    {
        public class AnalyzeModel : PageModel
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IConfiguration _configuration;

            public AnalyzeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _httpClientFactory = httpClientFactory;
                _configuration = configuration;
            }

            public DoctorDetailDTO? Doctor { get; set; }

            public async Task<IActionResult> OnGetAsync(int doctorId)
            {
                var baseUrl = _configuration["GivenAPIBaseUrl"]?.TrimEnd('/');
                var client = _httpClientFactory.CreateClient();

                // Gọi API chi tiết Doctor (dùng phép cộng chuỗi)
                var response = await client.GetAsync(baseUrl + "/api/doctors/" + doctorId);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Doctor = JsonSerializer.Deserialize<DoctorDetailDTO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                if (Doctor == null)
                {
                    return NotFound();
                }

                return Page();
            }
        }
    }

