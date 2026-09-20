using System.Security.Claims;
using JobApplication.API.Extensions;
using JobApplication.API.Models;
using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.API.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class JobCandidateApplicationsController : ControllerBase
    {
        private const long MaxCvBytes = 5 * 1024 * 1024;

        // Allowed CV types + the magic bytes the file must start with (extension alone is not trusted).
        private static readonly Dictionary<string, byte[]> CvSignatures = new()
        {
            [".pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },  // %PDF
            [".docx"] = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, // zip container
            [".doc"] = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }   // OLE2
        };

        private readonly IJobCandidateApplicationService _JobCandidateApplicationService;

        public JobCandidateApplicationsController(IJobCandidateApplicationService jobApplicationService)
        {
            _JobCandidateApplicationService = jobApplicationService;
        }

        // Identity always comes from the token, never from the request.
        private string UserId => User.FindFirstValue("sub")!;

        [Authorize(Roles = Roles.Candidate)]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Apply([FromForm] ApplyForJobRequest request)
        {
            var cv = request.Cv;

            if (cv.Length == 0 || cv.Length > MaxCvBytes)
                return BadRequest(new { errors = new[] { "CV must be a non-empty file up to 5 MB." } });

            var extension = Path.GetExtension(cv.FileName).ToLowerInvariant();
            if (!CvSignatures.TryGetValue(extension, out var signature))
                return BadRequest(new { errors = new[] { "CV must be a PDF, DOC or DOCX file." } });

            await using var stream = cv.OpenReadStream();
            var header = new byte[signature.Length];
            var read = await stream.ReadAsync(header, 0, header.Length);
            if (read != signature.Length || !header.SequenceEqual(signature))
                return BadRequest(new { errors = new[] { "The CV content does not match its file type." } });
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var result = await _JobCandidateApplicationService.ApplyAsync(
                    UserId, request.JobId, stream, extension);

                return result.Succeeded
                    ? StatusCode(StatusCodes.Status201Created, new { id = result.Value })
                    : this.FailureResult(result);
            }
            catch (DbUpdateException)
            {
                // Two simultaneous requests: the unique (CandidateId, JobId) index rejected the second one.
                return BadRequest(new { errors = new[] { "Could not save the application. You may have already applied to this job." } });
            }
        }

        [Authorize(Roles = Roles.Candidate)]
        [HttpGet("my")]
        public IActionResult GetMy()
        {
            return Ok(_JobCandidateApplicationService.GetMy(UserId));
        }

        [Authorize(Roles = Roles.Recruiter)]
        [HttpGet("~/api/jobs/{jobId:int}/applications")]
        public IActionResult GetForJob(int jobId)
        {
            var result = _JobCandidateApplicationService.GetForJob(jobId, UserId);
            return result.Succeeded ? Ok(result.Value) : this.FailureResult(result);
        }

        // Logical cancel (Status = Cancelled), not a physical delete.
        [Authorize(Roles = Roles.Candidate)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _JobCandidateApplicationService.CancelAsync(id, UserId);
            return result.Succeeded ? Ok(result.Value) : this.FailureResult(result);
        }

        [Authorize(Roles = Roles.Recruiter)]
        [HttpPatch("{id:int}/{status}")]
        public async Task<IActionResult> Update(int id, JobApplicationStatus status)
        {
            var result = await _JobCandidateApplicationService.UpdateStatus(id, status, UserId);
            return result.Succeeded ? Ok(new { id = result.Value }) : this.FailureResult(result);
        }
    }
}
