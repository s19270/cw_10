using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cw_3.Login;
using cw_3.NewModels;
using cw_3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace cw_3.Controllers
{
    [Route("api/enrollments")]
    [ApiController]
    //[Authorize(Roles = "employee")]
    public class EnrollmentsController : ControllerBase
    {
        public readonly string conString = "Data Source=db-mssql;Initial Catalog=s19270;Integrated Security=True";
        IStudentDbService service = new SqlServerDbService();
        public IConfiguration Configuration { get; set; }
        private readonly s19270Context _dbContext;
        public EnrollmentsController(s19270Context con)
        {
            _dbContext = con;
        }
        /*public EnrollmentsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }*/
        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            var st = _dbContext.Student.Where(s => s.IndexNumber == student.IndexNumber);
            if (st != null) return NotFound("Student o podanym indeksie juz istnieje");
            _dbContext.Student.Add(student);
            _dbContext.SaveChanges();
            return Ok("Dodano nowego studenta");
        }
        [HttpPost("promotions")]
        public IActionResult Promote(Promotion prom)
        {
            var en = _dbContext.Enrollment.Where(e => e.Semester == prom.Semester);
            var stud = _dbContext.Studies.Where(s => s.Name == prom.Studies).First();
            if (stud == null) return NotFound("Brak podanych studiow");
            var en1 = _dbContext.Enrollment.Where(e => e.Semester == (prom.Semester + 1));
            var e = en.Where(i => i.IdStudy == stud.IdStudy).First();
            if (e == null) return NotFound("Brak danych o danym kierunku");
            var e1 = en1.Where(i => i.IdStudy == stud.IdStudy).First();
            if(e1 == null)
            {
                var enr = new Enrollment();
                enr.IdEnrollment = _dbContext.Enrollment.Max().IdEnrollment + 1;
                enr.IdStudy = stud.IdStudy;
                enr.Semester = prom.Semester + 1;
                enr.StartDate = DateTime.Now;
                _dbContext.Enrollment.Add(enr);
                foreach (var stu in _dbContext.Student.Where(s => s.IdEnrollment == e.IdEnrollment))
                {
                    stu.IdEnrollment = enr.IdEnrollment;
                }
            }
            else
            {
                foreach (var stu in _dbContext.Student.Where(s => s.IdEnrollment == e.IdEnrollment))
                {
                    stu.IdEnrollment = e1.IdEnrollment;
                }
            }
            _dbContext.SaveChanges();
            return Ok();

        }/*
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest login)
        {
            if (!service.Logging(login.login, login.password)) return Unauthorized("Brak ucznia w bazie");
            var claims = new[]
{
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "doman"),
                new Claim(ClaimTypes.Role, "employee"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });
        }*/
        [HttpGet]
        public IActionResult GetStudent()
        {
            return Ok(_dbContext.Student.ToList());
        }
        [HttpPut("update")]
        public IActionResult UpdateStudent(Student student)
        {
            var st = _dbContext.Student.Where(s => s.IndexNumber == student.IndexNumber).First();
            if (st == null) return NotFound("Student o podanym indeksie nie istnieje");
            else {
                st.FirstName = student.FirstName;
                st.LastName = student.LastName;
                st.BirthDate = student.BirthDate;
                st.IdEnrollment = student.IdEnrollment;
                st.Password = student.Password;
                _dbContext.SaveChanges();
                return Ok("Zmieniono dane studenta"); 
            }
        }
        [HttpDelete("{indexnumber}")]
        public IActionResult DeleteStudent(string indexnumber)
        {
            var st = _dbContext.Student.Where(s => s.IndexNumber == indexnumber).First();
            if (st == null) return NotFound("Student o podanym indeksie nie istnieje");
            else
            {
                _dbContext.Student.Remove(st);
                _dbContext.SaveChanges();
                return Ok("Usunieto studenta");
            }
        }
    }
}