# CHEAT SHEET ÔN THI PE PRN232 - CÂU 1 (ASP.NET CORE WEB API .NET 8)
> **Ghi chú**: File này là tài liệu hướng dẫn định dạng Markdown (`.md`), không ảnh hưởng đến quá trình build hay chạy của project .NET (`dotnet build`, `dotnet run` sẽ bỏ qua file này). Bạn có thể mở file này song song trong VS Code / Visual Studio để copy-paste code khi làm bài thi.

---

## MỤC LỤC
1. [Khung Controller Chuẩn](#1-khung-controller-chuẩn)
2. [Dạng 1: GET All + COUNT (Đếm số lượng quan hệ 1-N, N-N)](#2-dạng-1-get-all--count-đếm-số-lượng)
3. [Dạng 2: GET Filter / Tìm kiếm đa điều kiện (In hoa/thường, Case-Insensitive)](#3-dạng-2-get-filter--tìm-kiếm-đa-điều-kiện)
4. [Dạng 3: SUM / AVG / MIN / MAX (Tính tổng tiền qua 2-3 bảng quan hệ, chống lỗi Null)](#4-dạng-3-sum--avg--min--max-chống-lỗi-null)
5. [Dạng 4: Lấy bản ghi Mới nhất / Gần nhất (Recent / Latest) + Fallback chuỗi](#5-dạng-4-lấy-bản-ghi-mới-nhất--gần-nhất-recent--latest)
6. [Dạng 5: In hoa, In thường, Cắt chuỗi, Format Ngày tháng](#6-dạng-5-biến-đổi-chuỗi-in-hoathường-format-ngày)
7. [Dạng 6: Lọc theo khoảng thời gian (Date Range Filter)](#7-dạng-6-lọc-theo-khoảng-thời-gian-date-range)
8. [Dạng 7: Phân trang (Pagination: Skip & Take)](#8-dạng-7-phân-trang-pagination)
9. [Dạng 8: GET Chi tiết theo ID (Check 404 Not Found)](#9-dạng-8-get-chi-tiết-theo-id)
10. [CODE MẪU TỔNG HỢP: Câu 2 trong đề thi (Full tính năng)](#10-code-mẫu-tổng-hợp-câu-2-trong-đề-thi)
11. [Mẫu POST: Check trùng Case-Insensitive + Trả về 201 kèm Navigation `[]` rỗng](#11-mẫu-post-check-trùng--trả-về-201-kèm-navigation--rỗng)
12. [Mẫu DELETE: Check 404 + Check ràng buộc khóa ngoại 400 + Xóa 204](#12-mẫu-delete-check-404--check-khóa-ngoại-400--xóa-204)
13. [Mẫu PUT: Cập nhật + Check 404 + Check trùng](#13-mẫu-put-cập-nhật--check-404--check-trùng)
14. [Những lỗi "chí mạng" cần tránh để không bị 0 điểm](#14-những-lỗi-chí-mạng-cần-tránh-để-không-bị-0-điểm)
15. [ĐỀ BIẾN THỂ MỚI: OData, 204 No Content, 409 Conflict, Validate Email & String ID](#15-đề-biến-thể-mới-odata-204-no-content-409-conflict-validate-email--string-id)

---

## 1. Khung Controller Chuẩn

Đề thi thường yêu cầu đường dẫn có tiền tố `/api/...` (ví dụ: `/api/Departments`, `/api/filter-patients`). 
Vì vậy hãy cấu hình route `[Route("api")]`:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Q1.Models; // Đổi theo namespace Models trong bài thi của bạn

namespace Q1.Controllers
{
    [Route("api")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        // 1. Khai báo DbContext
        private readonly MyDbContext _context; // Đổi tên DbContext của đề thi

        public DepartmentsController(MyDbContext context)
        {
            _context = context;
        }

        // ================= CÁC ACTION VIẾT DƯỚI ĐÂY =================
    }
}
```

---

## 2. Dạng 1: GET All + COUNT (Đếm số lượng)

**Đề thi mẫu:** Lấy tất cả phòng ban kèm tổng số bác sĩ (`doctorCount`).

```csharp
// GET: /api/Departments
[HttpGet("Departments")]
public IActionResult GetDepartments()
{
    var result = _context.Departments
        .Select(d => new
        {
            deptId = d.DeptId,
            deptName = d.DeptName,
            floorNumber = d.FloorNumber,
            
            // 1. Đếm 1-N trực tiếp:
            doctorCount = d.Doctors.Count(),

            // 2. [Nếu đề yêu cầu] Đếm có điều kiện (ví dụ: chỉ đếm bác sĩ đang hoạt động):
            // activeDoctorCount = d.Doctors.Count(doc => doc.Status == 1),

            // 3. [Nếu đề yêu cầu] Đếm qua bảng N-N:
            // serviceCount = d.Doctors.SelectMany(doc => doc.DoctorServices).Count()
        })
        .ToList();

    return Ok(result);
}
```

---

## 3. Dạng 2: GET Filter / Tìm kiếm đa điều kiện
*(Keyword In hoa/thường, Số min/max, Dropdown ID, Status)*

```csharp
// GET: /api/filter-patients?keyword=a&minBirthYear=1990&deptId=1
[HttpGet("filter-patients")]
public IActionResult FilterPatients(
    [FromQuery] string? keyword, 
    [FromQuery] int? minBirthYear,
    [FromQuery] int? deptId)
{
    // BƯỚC 1: VALIDATION ĐẦU VÀO (Nếu đề yêu cầu BadRequest 400)
    if (minBirthYear.HasValue && minBirthYear.Value < 1900)
    {
        return BadRequest("minBirthYear cannot be less than 1900");
    }

    // BƯỚC 2: KHỞI TẠO QUERYABLE
    var query = _context.Patients.AsQueryable();

    // BƯỚC 3: LỌC CHUỖI KHÔNG PHÂN BIỆT HOA THƯỜNG (Case-insensitive)
    if (!string.IsNullOrWhiteSpace(keyword))
    {
        string kw = keyword.Trim().ToLower();
        query = query.Where(p => p.FullName != null && p.FullName.ToLower().Contains(kw));
    }

    // BƯỚC 4: LỌC THEO SỐ (>=, <=, ==)
    if (minBirthYear.HasValue)
    {
        query = query.Where(p => p.BirthYear >= minBirthYear.Value);
    }

    // BƯỚC 5: LỌC THEO KHÓA NGOẠI HOẶC BẢNG LIÊN KẾT (Nếu có)
    if (deptId.HasValue)
    {
        // Ví dụ: Bệnh nhân có lịch hẹn với bác sĩ thuộc deptId này
        query = query.Where(p => p.Appointments.Any(a => 
            a.AppointmentDetails.Any(ad => ad.Doctor.DeptId == deptId.Value)));
    }

    // BƯỚC 6: SẮP XẾP (Theo đề bài yêu cầu)
    var result = query
        .OrderByDescending(p => p.BirthYear) // Giảm dần
        // .ThenBy(p => p.FullName)          // Tiêu chí phụ nếu có
        .Select(p => new
        {
            patientId = p.PatientId,
            fullName = p.FullName,
            // [NẾU ĐỀ YÊU CẦU IN HOA CHỮ CÁI ĐẦU TIÊN]:
            // fullName = !string.IsNullOrEmpty(p.FullName) 
            //     ? p.FullName.Substring(0, 1).ToUpper() + p.FullName.Substring(1).ToLower() 
            //     : "",
            phone = p.Phone,
            birthYear = p.BirthYear
        })
        .ToList();

    return Ok(result);
}
```

---

## 4. Dạng 3: SUM / AVG / MIN / MAX (Chống lỗi Null)

> **Cực kỳ quan trọng**: Khi dùng `.Sum()`, `.Average()` trên bảng quan hệ (có thể rỗng hoặc giá trị null trong DB), luôn ép kiểu sang **nullable** `(decimal?)` hoặc `(int?)` và kết thúc bằng `?? 0`. Nếu không sẽ văng lỗi `InvalidOperationException` làm sập API.

```csharp
// 1. Tính tổng tiền qua bảng trung gian: Patients -> Appointments -> AppointmentDetails -> Doctors.ConsultationFee
totalConsultationFee = p.Appointments
    .SelectMany(a => a.AppointmentDetails)
    .Sum(ad => (decimal?)ad.Doctor.ConsultationFee) ?? 0,

// 2. Tính tổng tiền trực tiếp:
// totalAmount = o.OrderDetails.Sum(od => (decimal?)(od.Quantity * od.UnitPrice)) ?? 0,

// 3. Tính điểm trung bình (Average):
// avgScore = s.ExamResults.Average(er => (double?)er.Score) ?? 0,

// 4. Lấy giá trị lớn nhất (Max) / nhỏ nhất (Min):
// maxFee = p.Appointments.SelectMany(a => a.AppointmentDetails).Max(ad => (decimal?)ad.Doctor.ConsultationFee) ?? 0,
```

---

## 5. Dạng 4: Lấy bản ghi Mới nhất / Gần nhất (Recent / Latest)

**Đề thi mẫu:** Lấy tên bác sĩ khám bệnh gần đây nhất (dựa trên `ApptDate` giảm dần), nếu bệnh nhân chưa từng khám thì trả về chuỗi `"No Doctors"`.

```csharp
// Lấy thuộc tính của bản ghi mới nhất, nếu không có trả về giá trị mặc định:
lastDoctorVisited = p.Appointments
    .OrderByDescending(a => a.ApptDate) // Ngày gần nhất lên trước
    .SelectMany(a => a.AppointmentDetails)
    .Select(ad => ad.Doctor.DoctorName)
    .FirstOrDefault() ?? "No Doctors",

// Dạng khác: Lấy ngày gần nhất định dạng chuỗi:
// lastBookingDate = p.Bookings
//     .OrderByDescending(b => b.BookingDate)
//     .Select(b => b.BookingDate.ToString("yyyy-MM-dd"))
//     .FirstOrDefault() ?? "N/A",
```

---

## 6. Dạng 5: Biến đổi chuỗi (In hoa/thường, IN HOA CHỮ CÁI ĐẦU, Format ngày)

> ⚠️ **LƯU Ý CỰC QUAN TRỌNG VỀ IN HOA CHỮ CÁI ĐẦU TRONG EF CORE:**
> - Nếu dùng hàm `ToTitleCase` hoặc hàm tự viết phức tạp, EF Core sẽ **không dịch được sang SQL** và báo lỗi.
> - **Cách chuẩn nhất khi đi thi:** Hãy dùng `AsEnumerable()` hoặc `.ToList()` để lấy dữ liệu về bộ nhớ trước, rồi format chuỗi cực kỳ dễ dàng và an toàn 100%!

```csharp
// ==========================================================
// CÁCH 1: FORMAT TRỰC TIẾP TRONG LINQ (Dịch được sang SQL)
// ==========================================================

// 1. In hoa toàn bộ chuỗi:
fullNameUpper = p.FullName != null ? p.FullName.ToUpper() : "",

// 2. In thường toàn bộ chuỗi:
emailLower = p.Email != null ? p.Email.ToLower() : "",

// 3. IN HOA CHỮ CÁI ĐẦU TIÊN CỦA CHUỖI (Ví dụ: "khoa noi" -> "Khoa noi"):
// Dùng Substring(0, 1).ToUpper() + Substring(1).ToLower()
nameCapitalized = !string.IsNullOrEmpty(p.FullName) 
    ? p.FullName.Substring(0, 1).ToUpper() + p.FullName.Substring(1).ToLower() 
    : "",


// ==========================================================
// CÁCH 2: IN HOA CHỮ CÁI ĐẦU CỦA MỖI TỪ (Title Case, khuyên dùng)
// Ví dụ: "nguyen van an" -> "Nguyen Van An"
// ==========================================================
// Bước 1: Query database và gọi .ToList() hoặc .AsEnumerable() để kéo dữ liệu về
var listFromDb = query.ToList();

// Bước 2: Dùng TextInfo.ToTitleCase format chữ cái đầu mỗi từ
var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;

var result = listFromDb.Select(p => new
{
    patientId = p.PatientId,
    // Format "nguyen van an" -> "Nguyen Van An":
    fullName = !string.IsNullOrEmpty(p.FullName) 
        ? textInfo.ToTitleCase(p.FullName.ToLower()) 
        : "",
    phone = p.Phone,
    birthYear = p.BirthYear
}).ToList();


// ==========================================================
// CÁC ĐỊNH DẠNG KHÁC THƯỜNG GẶP
// ==========================================================

// Format ngày tháng dạng dd/MM/yyyy:
dobString = p.Dob.HasValue ? p.Dob.Value.ToString("dd/MM/yyyy") : "",

// Format số tiền có dấu phân cách (Ví dụ: 1000000 -> "1,000,000 VND"):
// priceFormatted = p.Price.ToString("N0") + " VND",

// Lấy danh sách chuỗi con (List<string>) từ quan hệ N-N:
specializationList = doc.Specs.Select(s => s.SpecName).ToList(),
```

---

## 7. Dạng 6: Lọc theo khoảng thời gian (Date Range)

```csharp
// GET: /api/appointments?fromDate=2024-01-01&toDate=2024-12-31
[HttpGet("appointments-by-date")]
public IActionResult GetByDate([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
{
    var query = _context.Appointments.AsQueryable();

    if (fromDate.HasValue)
    {
        query = query.Where(a => a.ApptDate >= fromDate.Value);
    }

    if (toDate.HasValue)
    {
        // Cộng 1 ngày để bao trọn cả ngày kết thúc (tránh lỗi do giờ:phút:giây 00:00:00)
        DateTime nextDay = toDate.Value.Date.AddDays(1);
        query = query.Where(a => a.ApptDate < nextDay);
    }

    var result = query.ToList();
    return Ok(result);
}
```

---

## 8. Dạng 7: Phân trang (Pagination)

```csharp
// GET: /api/patients-paged?page=1&pageSize=10
[HttpGet("patients-paged")]
public IActionResult GetPatientsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 5;

    var totalCount = _context.Patients.Count();

    var data = _context.Patients
        .OrderBy(p => p.PatientId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new
        {
            patientId = p.PatientId,
            fullName = p.FullName,
            phone = p.Phone
        })
        .ToList();

    return Ok(new
    {
        totalRecords = totalCount,
        pageIndex = page,
        pageSize = pageSize,
        items = data
    });
}
```

---

## 9. Dạng 8: GET Chi tiết theo ID

```csharp
// GET: /api/Departments/{id}
[HttpGet("Departments/{id}")]
public IActionResult GetDepartmentById(int id)
{
    var dept = _context.Departments
        .Where(d => d.DeptId == id)
        .Select(d => new
        {
            deptId = d.DeptId,
            deptName = d.DeptName,
            floorNumber = d.FloorNumber,
            doctorCount = d.Doctors.Count()
        })
        .FirstOrDefault();

    if (dept == null)
    {
        return NotFound(); // 404
    }

    return Ok(dept); // 200
}
```

---

## 10. CODE MẪU TỔNG HỢP: Câu 2 trong đề thi
*(Tích hợp trọn vẹn: Validation 400 + Case-Insensitive Filter + Số Min + Sắp xếp + Sum bảng trung gian + Recent Item có Fallback)*

```csharp
// GET /api/filter-patients?keyword=a&minBirthYear=1990
[HttpGet("filter-patients")]
public IActionResult FilterPatients([FromQuery] string? keyword, [FromQuery] int? minBirthYear)
{
    // 1. Kiểm tra validation theo đề bài
    if (minBirthYear.HasValue && minBirthYear.Value < 1900)
    {
        return BadRequest("minBirthYear cannot be less than 1900");
    }

    var query = _context.Patients.AsQueryable();

    // 2. Lọc theo keyword (không phân biệt hoa thường)
    if (!string.IsNullOrWhiteSpace(keyword))
    {
        string kw = keyword.Trim().ToLower();
        query = query.Where(p => p.FullName != null && p.FullName.ToLower().Contains(kw));
    }

    // 3. Lọc theo năm sinh
    if (minBirthYear.HasValue)
    {
        query = query.Where(p => p.BirthYear >= minBirthYear.Value);
    }

    // 4. Sắp xếp giảm dần theo năm sinh (trẻ nhất lên trước)
    var result = query
        .OrderByDescending(p => p.BirthYear)
        .Select(p => new
        {
            patientId = p.PatientId,
            fullName = p.FullName,
            // [NẾU ĐỀ YÊU CẦU IN HOA CHỮ CÁI ĐẦU TIÊN]:
            // fullName = !string.IsNullOrEmpty(p.FullName) 
            //     ? p.FullName.Substring(0, 1).ToUpper() + p.FullName.Substring(1).ToLower() 
            //     : "",
            phone = p.Phone,
            birthYear = p.BirthYear,

            // Tổng tiền khám: ép kiểu (decimal?) và ?? 0
            totalConsultationFee = p.Appointments
                .SelectMany(a => a.AppointmentDetails)
                .Sum(ad => (decimal?)ad.Doctor.ConsultationFee) ?? 0,

            // Bác sĩ khám gần nhất:
            lastDoctorVisited = p.Appointments
                .OrderByDescending(a => a.ApptDate)
                .SelectMany(a => a.AppointmentDetails)
                .Select(ad => ad.Doctor.DoctorName)
                .FirstOrDefault() ?? "No Doctors"
        })
        .ToList();

    return Ok(result);
}
```

---

## 11. Mẫu POST: Check trùng + Trả về 201 kèm Navigation `[]` rỗng

**Yêu cầu đề bài:**
- Nhận dữ liệu từ body.
- Kiểm tra tên đã tồn tại (không phân biệt hoa thường), nếu trùng trả về `400 BadRequest`.
- Trả về `201 Created` kèm thực thể vừa tạo, các bảng quan hệ navigation (như Doctors) phải trả về **mảng rỗng `[]`** thay vì null (để tránh lỗi vòng lặp JSON).

```csharp
// DTO nhận dữ liệu từ Client (bỏ qua Id tự tăng)
public class DepartmentCreateDto
{
    public string DeptName { get; set; } = null!;
    public int FloorNumber { get; set; }
}

// POST: /api/Departments
[HttpPost("Departments")]
public IActionResult CreateDepartment([FromBody] DepartmentCreateDto request)
{
    // 1. Check trùng (case-insensitive)
    bool isExist = _context.Departments.Any(d => 
        d.DeptName != null && request.DeptName != null &&
        d.DeptName.ToLower() == request.DeptName.Trim().ToLower());

    if (isExist)
    {
        return BadRequest("Department name already exists");
    }

    // 2. Tạo đối tượng mới
    var newDept = new Department
    {
        DeptName = request.DeptName,
        FloorNumber = request.FloorNumber
    };

    _context.Departments.Add(newDept);
    _context.SaveChanges();

    // 3. Trả về đúng mẫu yêu cầu của đề: Navigation là list rỗng new List<object>()
    var responseObj = new
    {
        deptId = newDept.DeptId,
        deptName = newDept.DeptName,
        floorNumber = newDept.FloorNumber,
        doctors = new List<object>() // Bắt buộc trả về mảng rỗng []
    };

    return StatusCode(StatusCodes.Status201Created, responseObj);
    // hoặc: return Created(string.Empty, responseObj);
}
```

---

## 12. Mẫu DELETE: Check 404 + Check khóa ngoại 400 + Xóa 204

**Yêu cầu đề bài:**
- Nếu không tìm thấy: trả về `404 Not Found`.
- Nếu phòng ban vẫn còn bác sĩ trực thuộc (có khóa ngoại): trả về `400 Bad Request` kèm thông báo `"Cannot delete department with assigned doctors"`.
- Nếu xóa thành công: trả về `204 NoContent`.

```csharp
// DELETE: /api/Departments/{deptId}
[HttpDelete("Departments/{deptId}")]
public IActionResult DeleteDepartment(int deptId)
{
    // Include bảng con để kiểm tra
    var dept = _context.Departments
        .Include(d => d.Doctors)
        .FirstOrDefault(d => d.DeptId == deptId);

    // 1. Không tìm thấy -> 404
    if (dept == null)
    {
        return NotFound();
    }

    // 2. Kiểm tra còn ràng buộc bảng con -> 400
    if (dept.Doctors != null && dept.Doctors.Any())
    {
        return BadRequest("Cannot delete department with assigned doctors");
    }

    // 3. Xóa và lưu thay đổi -> 204
    _context.Departments.Remove(dept);
    _context.SaveChanges();

    return NoContent();
}
```

---

## 13. Mẫu PUT: Cập nhật + Check 404 + Check trùng

```csharp
// PUT: /api/Departments/{deptId}
[HttpPut("Departments/{deptId}")]
public IActionResult UpdateDepartment(int deptId, [FromBody] DepartmentCreateDto request)
{
    var dept = _context.Departments.FirstOrDefault(d => d.DeptId == deptId);
    if (dept == null)
    {
        return NotFound(); // 404
    }

    // Check trùng tên với phòng ban KHÁC (khác deptId hiện tại)
    bool isDuplicate = _context.Departments.Any(d => 
        d.DeptId != deptId && 
        d.DeptName != null && 
        d.DeptName.ToLower() == request.DeptName.Trim().ToLower());

    if (isDuplicate)
    {
        return BadRequest("Department name already exists");
    }

    dept.DeptName = request.DeptName;
    dept.FloorNumber = request.FloorNumber;

    _context.SaveChanges();

    return Ok(dept); // hoặc return NoContent(); tùy đề
}
```

---

## 14. Những Lỗi "Chí Mạng" Cần Tránh Để Không Bị 0 Điểm

1. **Chuỗi kết nối DB (`appsettings.json`)**:
   - Đề thi cấm tuyệt đối hardcode port. Chuỗi kết nối bắt buộc đặt tên là `"MyCnn"` theo đúng mẫu đề:
     ```json
     "ConnectionStrings": {
       "MyCnn": "Server=...;Database=...;..."
     }
     ```
2. **Lỗi vòng lặp tuần hoàn JSON (Cycle Reference)**:
   - Khi `return Ok(entity)` mà entity có quan hệ cha-con (ví dụ: Department có Doctors, Doctor lại có Department), JSON Serializer sẽ bị văng lỗi 500.
   - **Giải pháp**: Luôn dùng `.Select(x => new { ... })` để chỉ lấy các trường đề yêu cầu, hoặc dùng DTO.
3. **Lỗi Sum khi tập dữ liệu rỗng**:
   - Luôn viết: `.Sum(x => (decimal?)x.Price) ?? 0`
   - Đừng viết: `.Sum(x => x.Price)` (sẽ bị lỗi nếu không có phần tử nào).
4. **Không phân biệt hoa thường (Case-Insensitive)**:
   - Luôn dùng `.ToLower().Contains(kw.ToLower())` hoặc `.ToLower() == request.Name.ToLower()`.
5. **Cổng chạy ứng dụng (Port)**:
   - Không được hardcode cổng trong `Program.cs`. Hãy để nguyên cấu hình mặc định của Visual Studio.
6. **Lệnh nộp bài (Publish command)**:
   - Trước khi nộp vào PEA Client, mở Terminal tại thư mục project và chạy lệnh đúng như đề yêu cầu:
     ```bash
     dotnet publish -c Release -o ./[QuestionNumber_StudentAccount]
     ```
     *(Ví dụ: `dotnet publish -c Release -o ./Q1_duclthe123456`)*

---

## 15. ĐỀ BIẾN THỂ MỚI: OData, 204 No Content, 409 Conflict, Validate Email & String ID

> 🚨 **CẢNH BÁO:** Đây là bộ đề thi dạng 2 (chủ đề Shop / Products / Orders / Customers) có sự khác biệt rất lớn về **Mã trạng thái HTTP (Status Code)** và **Cấu trúc JSON**. Đọc kỹ đề để không bị mất điểm!

### 15.1. Bảng so sánh sự khác biệt giữa 2 dạng đề thi:

| Yêu cầu | Dạng đề 1 (Gym, Bệnh viện) | Dạng đề 2 (Shop, Products, Orders) |
| :--- | :--- | :--- |
| **Khi danh sách rỗng (Empty List)** | Trả về `200 OK` với mảng `[]` | Trả về **`204 No Content`** |
| **Check trùng khi POST** | Trả về `400 Bad Request` | Trả về **`409 Conflict`** |
| **Khi DELETE thành công** | Trả về `204 NoContent` | Trả về **`200 OK`** |
| **Tham số ID trên URL** | Nhận `int id` trực tiếp | Nhận `string tagId` rồi tự ép kiểu kiểm tra hợp lệ |
| **Hỗ trợ truy vấn** | Tự viết LINQ Where/OrderBy | Tích hợp **OData** (`$filter`, `$orderby`, `$top`, `$skip`) |

---

### 15.2. Mẫu 1: GET Orders có OData + Trả về 204 No Content nếu rỗng + JSON con lồng nhau

```csharp
// GET: /api/orders
// Hỗ trợ OData: $filter, $orderby, $top, $skip
[HttpGet("orders")]
[EnableQuery] // Dùng thuộc tính này nếu project đã cài OData
public IActionResult GetOrders()
{
    var orders = _context.Orders
        .Select(o => new
        {
            orderId = o.OrderId,
            orderDate = o.OrderDate.HasValue ? o.OrderDate.Value.ToString("yyyy-MM-dd") : null,
            // Đối tượng con lồng nhau:
            customer = new
            {
                customerId = o.Customer.CustomerId,
                fullName = o.Customer.FullName
            }
        })
        .ToList();

    // ĐỀ YÊU CẦU: Trả về 204 No Content nếu không có đơn hàng nào
    if (orders == null || !orders.Any())
    {
        return NoContent(); // 204 No Content
    }

    return Ok(orders); // 200 OK
}
```

---

### 15.3. Mẫu 2: GET Products By Tag nhận `string {tagId}` + Validate 400, 404, 204, 200

```csharp
// GET: /api/products/tag/{tagId}
[HttpGet("products/tag/{tagId}")]
public IActionResult GetProductsByTag(string tagId)
{
    // 1. Kiểm tra {tagId} có phải số nguyên hợp lệ không -> 400 Bad Request
    if (!int.TryParse(tagId, out int id))
    {
        return BadRequest(); // 400
    }

    // 2. Kiểm tra tag có tồn tại trong database không -> 404 Not Found
    bool tagExists = _context.Tags.Any(t => t.TagId == id);
    if (!tagExists)
    {
        return NotFound(); // 404
    }

    // 3. Lấy danh sách sản phẩm thuộc tag này (kèm danh sách tags của sản phẩm đó)
    var products = _context.Products
        .Where(p => p.ProductTags.Any(pt => pt.TagId == id))
        .Select(p => new
        {
            productId = p.ProductId,
            name = p.Name,
            price = p.Price,
            // Mảng con chứa danh sách tags:
            tags = p.ProductTags.Select(pt => new
            {
                id = pt.Tag.TagId,
                name = pt.Tag.TagName
            }).ToList()
        })
        .ToList();

    // 4. Nếu không có sản phẩm nào thuộc tag -> 204 No Content
    if (!products.Any())
    {
        return NoContent(); // 204
    }

    return Ok(products); // 200
}
```

---

### 15.4. Mẫu 3: POST Customer + Validate Email rỗng / sai định dạng (400) + Check trùng (409 Conflict)

```csharp
public class CustomerCreateDto
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

// POST: /api/customers
[HttpPost("customers")]
public IActionResult CreateCustomer([FromBody] CustomerCreateDto request)
{
    // 1. Kiểm tra thiếu trường (missing fields) -> 400 Bad Request
    if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
    {
        return BadRequest("FullName and Email are required");
    }

    // 2. Kiểm tra định dạng Email hợp lệ -> 400 Bad Request
    var emailValidator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
    if (!emailValidator.IsValid(request.Email))
    {
        return BadRequest("Email is malformed");
    }

    // 3. Kiểm tra email đã tồn tại -> 409 CONFLICT (RẤT QUAN TRỌNG: Đề yêu cầu 409 thay vì 400!)
    bool emailExists = _context.Customers.Any(c => 
        c.Email != null && c.Email.ToLower() == request.Email.Trim().ToLower());

    if (emailExists)
    {
        return StatusCode(StatusCodes.Status409Conflict, "Email is already exist in the database");
        // Hoặc: return Conflict("Email is already exist in the database");
    }

    // 4. Thêm khách hàng mới -> 201 Created
    var newCustomer = new Customer
    {
        FullName = request.FullName.Trim(),
        Email = request.Email.Trim()
    };

    _context.Customers.Add(newCustomer);
    _context.SaveChanges();

    return StatusCode(StatusCodes.Status201Created, newCustomer);
}
```

---

### 15.5. Mẫu 4: DELETE Customer + Trả về 200 OK (thay vì 204)

```csharp
// DELETE: /api/customers/{customerId}
[HttpDelete("customers/{customerId}")]
public IActionResult DeleteCustomer(int customerId)
{
    // 1. Kiểm tra khách hàng có tồn tại không -> 404 Not Found
    var customer = _context.Customers
        .Include(c => c.Orders)
        .FirstOrDefault(c => c.CustomerId == customerId);

    if (customer == null)
    {
        return NotFound(); // 404
    }

    // 2. Kiểm tra nếu đã có đơn hàng -> 400 Bad Request
    if (customer.Orders != null && customer.Orders.Any())
    {
        return BadRequest("Customer has existing orders and cannot be deleted");
    }

    // 3. Xóa khách hàng -> 200 OK (ĐỀ NÀY YÊU CẦU 200 OK THAY VÌ 204!)
    _context.Customers.Remove(customer);
    _context.SaveChanges();

    return Ok("Customer deleted successfully"); // 200 OK
}
```
