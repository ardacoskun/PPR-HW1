using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PprWeek1.Data;
using PprWeek1.Models;
using PprWeek1.Models.Entities;

namespace PprWeek1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeesController(ApplicationDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<Employee>>> GetAllEmployees()
        {
            var employees = await dbContext.Employees.ToListAsync();
            return Ok(employees);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(Guid id)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound("Employee not found!");
            }

            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<List<Employee>>> AddEmployee([FromBody] AddEmployeeDto addEmployeeDto)
        {
            var employeeEntity = new Employee()
            {
                Name = addEmployeeDto.Name,
                Email = addEmployeeDto.Email,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary,
            };

            dbContext.Employees.Add(employeeEntity);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employeeEntity.Id }, new { Message = "User created successfully.", Employee = employeeEntity });

        }

        [HttpPut("{id:guid}")]
        public async Task <ActionResult<List<Employee>>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await dbContext.Employees.FindAsync(id);

            if (employee is null)
            {
                return NotFound("Employee not found!");
            }

            employee.Name = updateEmployeeDto.Name;
            employee.Email = updateEmployeeDto.Email;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary;

            await dbContext.SaveChangesAsync();

            return Ok(employee);

        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<Employee>> PatchEmployee(Guid id, [FromBody] JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            var employee = await dbContext.Employees.FindAsync(id);

            if (employee is null)
            {
                return NotFound("Employee not found!");
            }

            var employeeDto = new UpdateEmployeeDto
            {
                Name = employee.Name,
                Email = employee.Email,
                Phone = employee.Phone,
                Salary = employee.Salary,
            };

            patchDocument.ApplyTo(employeeDto, ModelState);

            if (!TryValidateModel(employeeDto))
            {
                return BadRequest(ModelState);
            }

            employee.Name = employeeDto.Name;
            employee.Email = employeeDto.Email;
            employee.Phone = employeeDto.Phone;
            employee.Salary = employeeDto.Salary;

            await dbContext.SaveChangesAsync();

            return Ok(employee);
        }


        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<List<Employee>>> DeleteEmployee(Guid id)
        {

            var employee = await dbContext.Employees.FindAsync(id);

            if (employee is null)
            {
                return NotFound("Employee not found!");
            }

             dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync();

            return NoContent();

        }

        [HttpGet("sort")]
        public async Task<ActionResult<List<Employee>>> SortEmployeesBySalary([FromQuery] string sort = "asc")
        {
            IQueryable<Employee> employeesQuery = dbContext.Employees;

            if (sort?.ToLower() == "desc")
            {
                employeesQuery = employeesQuery.OrderByDescending(e => e.Salary);
            }
            else
            {
                employeesQuery = employeesQuery.OrderBy(e => e.Salary);
            }

            var employees = await employeesQuery.ToListAsync();
            return Ok(employees);
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<Employee>>> ListEmployeesByName([FromQuery] string name = "")
        {
            var employees = await dbContext.Employees
                .Where(e => e.Name.ToLower().Contains(name.Trim().ToLower()))
                .ToListAsync();

            if (employees == null || employees.Count == 0)
            {
                return NotFound($"No result for query '{name}'");
            }

            return Ok(employees);
        }

    }
}
