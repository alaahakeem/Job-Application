using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries.GetMyJobs
{
    public class GetMyJobsQueryHandler : IRequestHandler<GetMyJobsQuery, IEnumerable<JobDto>>
    {
        private readonly IRepository<Job> _jobRepository;

        public GetMyJobsQueryHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public Task<IEnumerable<JobDto>> Handle(GetMyJobsQuery request, CancellationToken cancellationToken)
        {
            var jobs = _jobRepository.Get()
                .Where(j => j.RecruiterId == request.RecruiterId)
                .OrderByDescending(j => j.Id)
                .ToList()
                .Select(JobMapper.ToDto);

            return Task.FromResult(jobs);
        }
    }
}
