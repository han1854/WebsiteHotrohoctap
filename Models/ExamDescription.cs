using System.Text.Json.Serialization;

namespace WebsiteHotrohoctap.Models
{
    public class ExamDescription
    {
        [JsonPropertyName("ProblemDescription")]
        public string ProblemDescription { get; set; }

        [JsonPropertyName("SampleInput")]
        public string SampleInput { get; set; }

        [JsonPropertyName("SampleOutput")]
        public string SampleOutput { get; set; }
    }
    public class TestCase
    {
        public string SampleInput { get; set; }
        public string SampleOutput { get; set; }
    }
}