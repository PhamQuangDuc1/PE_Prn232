using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Q2.Models;
using System.Text.Json;

namespace Q2.Pages.Doctor
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public List<DepartmentDTO> Departments { get; set; } = new();
        public List<DoctorSearchDTO> Doctors { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public int DeptId { get; set; } = 0;
        [BindProperty(SupportsGet = true)]
        public string? DoctorName { get; set; }
        public async Task OnGetAsync()
        {
            var baseUrl = _configuration["GivenAPIBaseUrl"]?.TrimEnd('/');
            var client = _httpClientFactory.CreateClient();
            // 1. Lấy danh sách Department (dùng phép cộng chuỗi)
            var deptResponse = await client.GetAsync(baseUrl + "/api/departments");
            if (deptResponse.IsSuccessStatusCode)
            {
                var content = await deptResponse.Content.ReadAsStringAsync();
                Departments = JsonSerializer.Deserialize<List<DepartmentDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            // 2. Lấy danh sách Doctor tìm kiếm
            var searchUrl = baseUrl + "/api/doctors/search?deptId=" + DeptId;
            if (!string.IsNullOrEmpty(DoctorName))
            {
                searchUrl += "&doctorName=" + Uri.EscapeDataString(DoctorName);
            }
            var docResponse = await client.GetAsync(searchUrl);
            if (docResponse.IsSuccessStatusCode)
            {
                var content = await docResponse.Content.ReadAsStringAsync();
                Doctors = JsonSerializer.Deserialize<List<DoctorSearchDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
        }
    }
}
