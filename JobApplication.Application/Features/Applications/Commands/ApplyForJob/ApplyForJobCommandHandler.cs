using JobApplication.Application.Common;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.ApplyForJob
{
    public class ApplyForJobCommandHandler : IRequestHandler<ApplyForJobCommand, Result<int>>
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly ICvStorage _cvStorage;

        public ApplyForJobCommandHandler(
            IRepository<JobCandidateApplication> jobApplicationRepository,
            IRepository<Candidate> candidateRepository,
            IRepository<Job> jobRepository,
            ICvStorage cvStorage)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _candidateRepository = candidateRepository;
            _jobRepository = jobRepository;
            _cvStorage = cvStorage;
        }

        public async Task<Result<int>> Handle(ApplyForJobCommand request, CancellationToken cancellationToken)
        {
            // CandidateId is resolved from the authenticated user, never taken from the client.
            var candidate = _candidateRepository.Get().FirstOrDefault(c => c.UserId == request.UserId);
            if (candidate is null)
                return Result<int>.Failure("Candidate profile not found.", ErrorKind.NotFound);

            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == request.JobId);
            if (job is null)
                return Result<int>.Failure("Job not found.", ErrorKind.NotFound);

            if (job.Status != JobStatus.Open)
                return Result<int>.Failure("This job is closed and no longer accepts applications.");

            var alreadyApplied = _jobApplicationRepository.Get()
                .Any(a => a.CandidateId == candidate.Id && a.JobId == job.Id);
            if (alreadyApplied)
                return Result<int>.Failure("You have already applied to this job.");

            var cvUrl = await _cvStorage.SaveAsync(request.Cv, request.CvExtension);

            // New application => status Applied (set by the entity constructor).
            var jobApplication = new JobCandidateApplication()
            {
                JobId = job.Id,
                CandidateId = candidate.Id,
                CvUrl = cvUrl
            };

            try
            {
                await _jobApplicationRepository.AddAsync(jobApplication);
                await _jobApplicationRepository.SaveChangesAsync();
            }
            catch
            {
                // Don't leave an orphan file (e.g. a concurrent duplicate hit the unique index).
                _cvStorage.Delete(cvUrl);
                throw;
            }

            return Result<int>.Success(jobApplication.Id);
        }
    }
}
