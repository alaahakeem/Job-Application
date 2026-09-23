using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Applications.Queries.GetApplicationsForJob
{
    public class GetApplicationsForJobQueryHandler : IRequestHandler<GetApplicationsForJobQuery, Result<List<JobApplicationDto>>>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Job> _jobRepository;

        public GetApplicationsForJobQueryHandler(
            IRepository<JobCandidateApplication> jobApplicationRepository,
            IRepository<Job> jobRepository)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobRepository = jobRepository;
        }

        public Task<Result<List<JobApplicationDto>>> Handle(GetApplicationsForJobQuery request, CancellationToken cancellationToken)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == request.JobId);
            if (job is null)
                return Task.FromResult(Result<List<JobApplicationDto>>.Failure("Job not found.", ErrorKind.NotFound));

            if (job.RecruiterId != request.RecruiterId)
                return Task.FromResult(Result<List<JobApplicationDto>>.Failure(
                    "You can only view applications of your own jobs.", ErrorKind.Forbidden));

            // All statuses are returned (including Cancelled) - closing a job never hides its applications.
            var applications = _jobApplicationRepository.Get()
                .Where(a => a.JobId == request.JobId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(JobApplicationMapper.ToDto)
                .ToList();

            return Task.FromResult(Result<List<JobApplicationDto>>.Success(applications));
        }
    }
}
