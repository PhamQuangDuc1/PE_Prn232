using Q1.Models;

namespace Q1.DTOs
{
    public class TrainerDtos
    {
        public int TrainerId { get; set; }

        public string? TrainerName { get; set; }

        public string? Email { get; set; }

        public int? ExperienceYears { get; set; }
        public int TotalMinutesTrained { get; set; }

        public string? LastBookedMember { get; set; }
        public List<string> SpecializationList { get; set; } = new();
    }
}
