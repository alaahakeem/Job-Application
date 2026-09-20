using System.Security.Claims;
using JobApplication.API.Extensions;
using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _JobService;

        public JobsController(IJobService jobService)
        {
            _JobService = jobService;
        }

        // Identity always comes from the token, never from the request body.
        private string UserId => User.FindFirstValue("sub")!;

        [Authorize(Roles = Roles.Recruiter)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var id = await _JobService.CreateAsync(createJobDto, UserId);
            return Ok(new
            {
                id = id 
            }); 
        }

        [Authorize(Roles = Roles.Candidate)]
        [HttpGet]
        public IActionResult GetOpen()
        {
            return Ok(_JobService.GetOpen());
        }

        [Authorize(Roles = $"{Roles.Candidate},{Roles.Recruiter}")]
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var job = _JobService.GetById(id);
            if (job is null)
                return NotFound(new { errors = new[] { "Job not found." } });

            return Ok(job);
        }

        [Authorize(Roles = Roles.Recruiter)]
        [HttpGet("my-jobs")]
        public IActionResult GetMyJobs()
        {
            return Ok(_JobService.GetByRecruiter(UserId));
        }

        // Logical close (Status = Closed), not a physical delete.
        [Authorize(Roles = Roles.Recruiter)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _JobService.CloseAsync(id, UserId);
            return result.Succeeded ? Ok(result.Value) : this.FailureResult(result);
        }
    }
}
