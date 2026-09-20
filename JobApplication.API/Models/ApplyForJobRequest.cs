using System.ComponentModel.DataAnnotations;
using JobApplication.Application.DTOs;

namespace JobApplication.API.Models
{
    // multipart/form-data body of POST /api/applications: JobId (inherited) + the CV file.
    // Swashbuckle needs IFormFile inside one complex type when the form has other fields.
    public class ApplyForJobRequest : CreateJobCandidateApplicationDto
    {
        [Required]
        public IFormFile Cv { get; set; } = null!;
    }
}
