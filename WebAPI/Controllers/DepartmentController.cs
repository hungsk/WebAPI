using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Repository;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _department;
        private readonly IEFCacheServiceProvider _cacheServiceProvider;

        public DepartmentController(IDepartmentRepository department, IEFCacheServiceProvider cacheServiceProvider)
        {
            _department = department ?? throw new ArgumentNullException(nameof(department));
            _cacheServiceProvider = cacheServiceProvider;
        }

        [HttpGet]
        [Route("ClearAllCache")]
        public JsonResult ClearAllCache()
        {
            _cacheServiceProvider.ClearAllCachedEntries();

            return new JsonResult("Clear All Cache Successfully");
        }

        [HttpGet]
        [Route("GetDepartment")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _department.GetDepartment());
        }

        [HttpGet]
        [Route("GetDepartmentByID/{Id}")]
        public async Task<IActionResult> GetDeptById(int Id)
        {
            return Ok(await _department.GetDepartmentByID(Id));
        }

        [HttpPost]
        [Route("AddDepartment")]
        public async Task<IActionResult> Post(Department dep)
        {
            var result = await _department.InsertDepartment(dep);
            if (result.DepartmentId == 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something Went Wrong");
            }
            return Ok("Added Successfully");
        }

        [HttpPut]
        [Route("UpdateDepartment")]
        public async Task<IActionResult> Put(Department dep)
        {
            await _department.UpdateDepartment(dep);
            return Ok("Updated Successfully");
        }


        [HttpDelete]
        //[HttpDelete("{id}")]
        [Route("DeleteDepartment")]
        public JsonResult Delete(int id)
        {
            _department.DeleteDepartment(id);
            return new JsonResult("Deleted Successfully");
        }
    }
}
