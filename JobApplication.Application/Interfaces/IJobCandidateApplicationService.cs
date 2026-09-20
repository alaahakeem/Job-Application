using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Interfaces
{
    public interface IJobCandidateApplicationService
    {
        Task<Result<int>> ApplyAsync(string userId, int jobId, Stream cv, string cvExtension);
        IEnumerable<JobApplicationDto> GetMy(string userId);
        Result<List<JobApplicationDto>> GetForJob(int jobId, string recruiterId);
        Task<Result<int>> UpdateStatus(int id, JobApplicationStatus status, string recruiterId);
        Task<Result<JobApplicationDto>> CancelAsync(int id, string userId);
    }
}
