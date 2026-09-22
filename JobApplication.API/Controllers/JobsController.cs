﻿using System.Security.Claims;
using JobApplication.API.Extensions;
using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Features.Jobs.Commands.CloseJob;
using JobApplication.Application.Features.Jobs.Commands.CreateJob;
using JobApplication.Application.Features.Jobs.Queries.GetJobById;
using JobApplication.Application.Features.Jobs.Queries.GetMyJobs;
using JobApplication.Application.Features.Jobs.Queries.GetOpenJobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Identity always comes from the token, never from the request body.
        private string UserId => User.FindFirstValue("sub")!;

        [Authorize(Roles = Roles.Recruiter)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var id = await _mediator.Send(new CreateJobCommand(createJobDto.Title, createJobDto.Description, UserId));
            return Ok(new
            {
                id = id
            });
        }

        [Authorize(Roles = Roles.Candidate)]
        [HttpGet]
        public async Task<IActionResult> GetOpen()
        {
            return Ok(await _mediator.Send(new GetOpenJobsQuery()));
        }

        [Authorize(Roles = $"{Roles.Candidate},{Roles.Recruiter}")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _mediator.Send(new GetJobByIdQuery(id));
            if (job is null)
                return NotFound(new { errors = new[] { "Job not found." } });

            return Ok(job);
        }

        [Authorize(Roles = Roles.Recruiter)]
        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            return Ok(await _mediator.Send(new GetMyJobsQuery(UserId)));
        }

        // Logical close (Status = Closed), not a physical delete.
        [Authorize(Roles = Roles.Recruiter)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Close(int id)
        {
            var result = await _mediator.Send(new CloseJobCommand(id, UserId));
            return result.Succeeded ? Ok(result.Value) : this.FailureResult(result);
        }
    }
}
