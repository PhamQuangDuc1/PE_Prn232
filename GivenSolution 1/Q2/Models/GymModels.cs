namespace Q2.Models
{
    // --- Member DTOs (De 1, 3, 5) ---
    public class MemberSearchDTO
    {
        public int MemberID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public string PackageName { get; set; } = string.Empty;
    }

    public class MemberSessionDTO
    {
        public int BookingID { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public DateTime SessionTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MemberDetailDTO
    {
        public int MemberID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public List<MemberSessionDTO> Sessions { get; set; } = new();
    }

    // --- Trainer DTOs (De 2, 4, 6) ---
    public class SpecializationDTO
    {
        public int SpecID { get; set; }
        public string SpecName { get; set; } = string.Empty;
    }

    public class TrainerSearchDTO
    {
        public int TrainerID { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public List<SpecializationDTO> Specializations { get; set; } = new();
    }

    public class BookingSessionDTO
    {
        public int MemberID { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int BookingID { get; set; }
        public DateTime SessionTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TrainerDetailDTO
    {
        public int TrainerID { get; set; }
        public string TrainerName { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public List<SpecializationDTO> Specializations { get; set; } = new();
        public List<BookingSessionDTO> BookingSessions { get; set; } = new();
    }
}
