using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Services
{
    public class JDoodleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public JDoodleService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _clientId = _configuration["JDoodle:ClientId"];
            _clientSecret = _configuration["JDoodle:ClientSecret"];
        }

        public async Task<string> ExecuteCode(string code, string language)
        {
            var payload = new
            {
                clientId = _clientId,
                clientSecret = _clientSecret,
                script = code,
                language = language,
                versionIndex = "3"  // Đảm bảo phiên bản JDoodle đúng
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://api.jdoodle.com/v1/execute", content);
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: Không thể kết nối JDoodle. Status Code: {response.StatusCode}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JDoodleResult>(responseString);

                if (result != null && !string.IsNullOrEmpty(result.output))
                {
                    return result.output.Trim();
                }
                else
                {
                    return "Không có kết quả từ JDoodle.";
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi khi kết nối JDoodle: {ex.Message}";
            }
        }
    }
}
