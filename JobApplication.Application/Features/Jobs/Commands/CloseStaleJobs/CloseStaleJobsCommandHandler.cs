using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseStaleJobs
{
    public class CloseStaleJobsCommandHandler : IRequestHandler<CloseStaleJobsCommand, int>
    {
        private readonly IRepository<Job> _jobRepository;

        public CloseStaleJobsCommandHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> Handle(CloseStaleJobsCommand request, CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow.AddDays(-request.MaxOpenDays);

            var staleJobs = _jobRepository.Get()
                .Where(j => j.Status == JobStatus.Open && j.CreatedAt <= cutoff)
                .ToList();

            if (staleJobs.Count == 0)
                return 0;

            foreach (var job in staleJobs)
                job.Close(); // same soft close as the recruiter's manual close: Status = Closed + ClosedAt

            await _jobRepository.SaveChangesAsync();

            return staleJobs.Count;
        }
    }
}
