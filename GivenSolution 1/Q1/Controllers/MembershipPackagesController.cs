using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Q1.DTOs;
using Q1.Models;

namespace Q1.Controllers
{
    [Route("api")]
    [ApiController]
    public class MembershipPackagesController : ControllerBase
    {
        private readonly Prn232PeSu2613Context _context;

        public MembershipPackagesController(Prn232PeSu2613Context context)
        {
            _context = context;
        }

        // 1. GET /api/MembershipPackages
        [HttpGet("MembershipPackages")]
        public IActionResult GetMembershipPackages()
        {
            var list = _context.MembershipPackages
                .Select(p => new MembershipPackageDto
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Price = p.Price ?? 0,
                    MemberCount = p.Members.Count()
                })
                .ToList();

            foreach (var item in list)
            {
                item.Price = item.Price / 1.000000000000000000000000000000000m;
            }

            return Ok(list);
        }

        // 2. GET /api/filter-members
        [HttpGet("filter-members")]
        public IActionResult FilterMembers([FromQuery] int? packageId, [FromQuery] int? minDuration)
        {
            if (minDuration < 0)
            {
                return BadRequest("minDuration cannot be negative");
            }

            var query = _context.Members
                .Include(m => m.Package)
                .Include(m => m.BookingDetails)
                    .ThenInclude(bd => bd.Booking)
                        .ThenInclude(b => b.Trainer)
                .AsQueryable();

            if (packageId.HasValue)
            {
                query = query.Where(m => m.PackageId == packageId.Value);
            }

            if (minDuration.HasValue)
            {
                query = query.Where(m => m.BookingDetails.Any(bd => bd.DurationMinutes >= minDuration.Value));
            }

            var members = query
                .OrderByDescending(m => m.JoinDate)
                .ToList();

            var result = members.Select(m => new MemberFilterDto
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                JoinDate = m.JoinDate.HasValue ? m.JoinDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                PackageName = m.Package?.PackageName,
                TotalMinutesTrained = m.BookingDetails.Sum(bd => bd.DurationMinutes ?? 0),
                LastTrainerName = m.BookingDetails
                    .Where(bd => bd.Booking != null && bd.Booking.Trainer != null)
                    .OrderByDescending(bd => bd.Booking.SessionTime)
                    .Select(bd => bd.Booking.Trainer!.TrainerName)
                    .FirstOrDefault() ?? "No Trainers"
            }).ToList();

            return Ok(result);
        }

        // 3. POST /api/MembershipPackages
        [HttpPost("MembershipPackages")]
        public IActionResult AddMembershipPackage([FromBody] MembershipPackageCreateDto request)
        {
            bool nameExists = _context.MembershipPackages
                .Any(p => p.PackageName != null && request.PackageName != null && p.PackageName.ToLower() == request.PackageName.ToLower());

            if (nameExists)
            {
                return BadRequest("Membership package name already exists");
            }

            var newPackage = new MembershipPackage
            {
                PackageName = request.PackageName,
                Price = request.Price
            };

            _context.MembershipPackages.Add(newPackage);
            _context.SaveChanges();

            var responseObj = new
            {
                packageId = newPackage.PackageId,
                packageName = newPackage.PackageName,
                price = newPackage.Price.HasValue ? newPackage.Price.Value / 1.000000000000000000000000000000000m : (decimal?)null,
                members = new List<object>()
            };

            return Created(string.Empty, responseObj);
        }

        // 4. DELETE /api/MembershipPackages/{packageId}
        [HttpDelete("MembershipPackages/{packageId}")]
        public IActionResult DeleteMembershipPackage(int packageId)
        {
            var pkg = _context.MembershipPackages
                .Include(p => p.Members)
                .FirstOrDefault(p => p.PackageId == packageId);

            if (pkg == null)
            {
                return NotFound();
            }

            if (pkg.Members != null && pkg.Members.Any())
            {
                return BadRequest("Cannot delete membership package with active registered members");
            }

            _context.MembershipPackages.Remove(pkg);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
