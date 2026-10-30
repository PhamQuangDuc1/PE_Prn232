using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Q1.DTOs;
using Q1.Models;

namespace Q1.Controllers
{
    [Route("api")]
    [ApiController]
    public class SpecializationController : ControllerBase
    {
        private readonly Prn232PeSu2613Context _context;
        public SpecializationController(Prn232PeSu2613Context context)
        {
            _context = context;
        }

        // 1. GET /api/Specializations
        [HttpGet("Specializations")]
        public IActionResult GetSpecializations()
        {
            var result = _context.Specializations
                .Select(s => new SpecializationDto
                {
                    SpecId = s.SpecId,
                    SpecName = s.SpecName,
                    Description = s.Description,
                    TrainerCount = s.Trainers.Count()
                })
                .ToList();

            return Ok(result);
        }

        // 2. GET /api/filter-trainers
        [HttpGet("filter-trainers")]
        public IActionResult FilterTrainers([FromQuery] int? specId, [FromQuery] int? minExperience)
        {
            if (minExperience < 0)
            {
                return BadRequest("minExperience cannot be negative");
            }

            var query = _context.Trainers.AsQueryable();

            if (specId.HasValue)
            {
                query = query.Where(t => t.Specs.Any(s => s.SpecId == specId.Value));
            }

            if (minExperience.HasValue)
            {
                query = query.Where(t => t.ExperienceYears >= minExperience.Value);
            }

            var result = query
                .OrderByDescending(t => t.ExperienceYears)
                .Select(t => new TrainerDtos
                {
                    TrainerId = t.TrainerId,
                    TrainerName = t.TrainerName,
                    Email = t.Email,
                    ExperienceYears = t.ExperienceYears ?? 0,
                    TotalMinutesTrained = t.Bookings
                        .SelectMany(b => b.BookingDetails)
                        .Sum(bd => (int?)bd.DurationMinutes) ?? 0,
                    LastBookedMember = t.Bookings
                        .OrderByDescending(b => b.SessionTime)
                        .SelectMany(b => b.BookingDetails)
                        .Select(bd => bd.Member.FullName)
                        .FirstOrDefault() ?? "No Bookings",
                    SpecializationList = t.Specs.Select(s => s.SpecName!).ToList()
                })
                .ToList();

            return Ok(result);
        }

        // 3. POST /api/trainers
        [HttpPost("trainers")]
        public IActionResult AddTrainer([FromBody] TrainerCreateDTO request)
        {
            bool emailExists = _context.Trainers.Any(t => t.Email != null && request.Email != null && t.Email.ToLower() == request.Email.ToLower());
            if (emailExists)
            {
                return BadRequest("Trainer email already exists");
            }

            var newTrainer = new Trainer
            {
                TrainerName = request.TrainerName,
                Email = request.Email,
                ExperienceYears = request.ExperienceYears
            };

            _context.Trainers.Add(newTrainer);
            _context.SaveChanges();

            var responseObj = new
            {
                trainerId = newTrainer.TrainerId,
                trainerName = newTrainer.TrainerName,
                email = newTrainer.Email,
                experienceYears = newTrainer.ExperienceYears,
                bookings = new List<object>(),
                specs = new List<object>()
            };

            return Created(string.Empty, responseObj);
        }

        // 4. DELETE /api/trainers/{trainerId}
        [HttpDelete("trainers/{trainerId}")]
        public IActionResult DeleteTrainer(int trainerId)
        {
            var trainer = _context.Trainers
                .Include(t => t.Bookings)
                .Include(t => t.Specs)
                .FirstOrDefault(t => t.TrainerId == trainerId);

            if (trainer == null)
            {
                return NotFound();
            }

            if (trainer.Bookings != null && trainer.Bookings.Any())
            {
                return BadRequest("Cannot delete trainer with active booking records");
            }

            trainer.Specs.Clear();
            _context.Trainers.Remove(trainer);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
