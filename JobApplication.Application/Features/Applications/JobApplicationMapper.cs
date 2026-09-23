using System.Linq.Expressions;
using JobApplication.Application.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Features.Applications
{
    // Kept as an Expression<Func<...>> (not a plain method) because it is used inside
    // IQueryable<JobCandidateApplication>.Select(...) so EF Core can translate it to SQL.
    internal static class JobApplicationMapper
    {
        public static readonly Expression<Func<JobCandidateApplication, JobApplicationDto>> ToDto = a => new JobApplicationDto
        {
            Id = a.Id,
            JobId = a.JobId,
            JobTitle = a.Job.Title,
            CandidateId = a.CandidateId,
            CandidateName = a.Candidate.Name,
            Status = a.JobApplicationStatus.ToString(),
            AppliedAt = a.AppliedAt,
            StatusUpdatedAt = a.StatusUpdatedAt,
            CancelledAt = a.CancelledAt,
            CvUrl = a.CvUrl
        };
    }
}
