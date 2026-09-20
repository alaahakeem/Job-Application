using JobApplication.Application.Common;
using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces
{
    public interface IJobService
    {
        Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId);
        IEnumerable<JobDto> GetOpen();
        JobDto? GetById(int id);
        IEnumerable<JobDto> GetByRecruiter(string recruiterId);
        Task<Result<JobDto>> CloseAsync(int id, string recruiterId);
    }
}
