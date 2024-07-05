using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PprWeek1.Base;
using PprWeek1.Data;
using PprWeek1.Models;
using PprWeek1.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pa.Api
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
        public async Task<ActionResult<ApiResponse<List<Employee>>>> GetAllEmployees()
        {
            var employees = await dbContext.Employees.ToListAsync();
            return new ApiResponse<List<Employee>>(employees);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<Employee>>> GetEmployeeById([FromRoute] Guid id)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return new ApiResponse<Employee>("Employee not found!")
                {
                    IsSuccess = false
                };
            }
            return new ApiResponse<Employee>(employee);
        }


        //Id parametresini yi query den alır
        [HttpGet("ByIdQuery")]
        public async Task<ActionResult<ApiResponse<Employee>>> GetEmployeeIdByQuery([FromQuery] Guid id)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return new ApiResponse<Employee>("Employee not found!")
                {
                    IsSuccess = false
                };
            }
            return new ApiResponse<Employee>(employee);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Employee>>> AddEmployee([FromBody] AddEmployeeDto addEmployeeDto)
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

            return new ApiResponse<Employee>(employeeEntity)
            {
                IsSuccess = true
            };
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<Employee>>> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return new ApiResponse<Employee>("Employee not found!")
                {
                    IsSuccess = false
                };
            }

            employee.Name = updateEmployeeDto.Name;
            employee.Email = updateEmployeeDto.Email;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary;

            await dbContext.SaveChangesAsync();

            return new ApiResponse<Employee>(employee);
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ApiResponse<Employee>>> PatchEmployee(Guid id, [FromBody] JsonPatchDocument<UpdateEmployeeDto> patchDocument)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return new ApiResponse<Employee>("Employee not found!")
                {
                    IsSuccess = false
                };
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

            return new ApiResponse<Employee>(employee);
        }


        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(Guid id)
        {
            var employee = await dbContext.Employees.FindAsync(id);
            if (employee == null)
            {
                return new ApiResponse<object>("Employee not found!")
                {
                    IsSuccess = false
                };
            }

            dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync();

            return new ApiResponse<object>(null)
            {
                IsSuccess = true
            };
        }

        //Queryden aldığı asc veya desc string ifadelerine göre salary sıralaması yapar. 
        [HttpGet("sort")]
        public async Task<ActionResult<ApiResponse<List<Employee>>>> SortEmployeesBySalary([FromQuery] string sort = "asc")
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
            return new ApiResponse<List<Employee>>(employees);
        }


        //Queryden aldığı string ifadeye göre filtreleme yapar. 
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<List<Employee>>>> ListEmployeesByName([FromQuery] string name = "")
        {
            var employees = await dbContext.Employees
                .Where(e => e.Name.ToLower().Contains(name.Trim().ToLower()))
                .ToListAsync();

            if (employees == null || employees.Count == 0)
            {
                return new ApiResponse<List<Employee>>($"No result for query '{name}'")
                {
                    IsSuccess = false
                };
            }

            return new ApiResponse<List<Employee>>(employees);
        }
    }
}
