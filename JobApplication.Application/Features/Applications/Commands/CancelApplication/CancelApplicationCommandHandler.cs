using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.CancelApplication
{
    public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, Result<JobApplicationDto>>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Candidate> _candidateRepository;

        public CancelApplicationCommandHandler(
            IRepository<JobCandidateApplication> jobApplicationRepository,
            IRepository<Candidate> candidateRepository)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _candidateRepository = candidateRepository;
        }

        public async Task<Result<JobApplicationDto>> Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
        {
            var jobApplication = _jobApplicationRepository.Get().FirstOrDefault(a => a.Id == request.Id);
            if (jobApplication is null)
                return Result<JobApplicationDto>.Failure("Application not found.", ErrorKind.NotFound);

            // Ownership is checked on the server: the application must belong to the logged-in candidate.
            var isOwner = _candidateRepository.Get()
                .Any(c => c.Id == jobApplication.CandidateId && c.UserId == request.UserId);
            if (!isOwner)
                return Result<JobApplicationDto>.Failure(
                    "You can only cancel your own applications.", ErrorKind.Forbidden);

            if (jobApplication.JobApplicationStatus == JobApplicationStatus.Cancelled)
                return Result<JobApplicationDto>.Failure("This application is already cancelled.");

            if (jobApplication.JobApplicationStatus is not (JobApplicationStatus.Applied or JobApplicationStatus.UnderReview))
                return Result<JobApplicationDto>.Failure("Only Applied or UnderReview applications can be cancelled.");

            jobApplication.Cancel(); // soft cancel: Status = Cancelled + CancelledAt, row is kept
            await _jobApplicationRepository.SaveChangesAsync();

            var dto = _jobApplicationRepository.Get().Where(a => a.Id == request.Id).Select(JobApplicationMapper.ToDto).First();
            return Result<JobApplicationDto>.Success(dto);
        }
    }
}
