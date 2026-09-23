using JobApplication.Application.Common;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus
{
    public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, Result<int>>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Job> _jobRepository;

        public UpdateApplicationStatusCommandHandler(
            IRepository<JobCandidateApplication> jobApplicationRepository,
            IRepository<Job> jobRepository)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobRepository = jobRepository;
        }

        public async Task<Result<int>> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
        {
            var jobApplication = _jobApplicationRepository.Get().FirstOrDefault(a => a.Id == request.Id);
            if (jobApplication is null)
                return Result<int>.Failure("Application not found.", ErrorKind.NotFound);

            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == jobApplication.JobId);
            if (job is null || job.RecruiterId != request.RecruiterId)
                return Result<int>.Failure(
                    "You can only change the status of applications on your own jobs.", ErrorKind.Forbidden);

            try
            {
                jobApplication.UpdateStatus(request.Status);
            }
            catch (InvalidOperationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }

            await _jobApplicationRepository.SaveChangesAsync();
            return Result<int>.Success(jobApplication.Id);
        }
    }
}
