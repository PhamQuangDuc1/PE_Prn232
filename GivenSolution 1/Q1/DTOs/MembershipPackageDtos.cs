namespace Q1.DTOs
{
    public class MembershipPackageDto
    {
        public int PackageId { get; set; }
        public string? PackageName { get; set; }
        public decimal Price { get; set; }
        public int MemberCount { get; set; }
    }

    public class MembershipPackageCreateDto
    {
        public string? PackageName { get; set; }
        public decimal? Price { get; set; }
    }

    public class MemberFilterDto
    {
        public int MemberId { get; set; }
        public string? FullName { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? PackageName { get; set; }
        public int TotalMinutesTrained { get; set; }
        public string? LastTrainerName { get; set; }
    }
}
