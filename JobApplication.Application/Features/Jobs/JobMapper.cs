using JobApplication.Application.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Jobs
{
    internal static class JobMapper
    {
        public static JobDto ToDto(Job job) => new()
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Status = job.Status.ToString(),
            ClosedAt = job.ClosedAt
        };
    }
}
