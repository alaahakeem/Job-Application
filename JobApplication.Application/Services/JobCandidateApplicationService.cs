using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JobApplication.Application.Services
{
    public class JobCandidateApplicationService : IJobCandidateApplicationService
    {
        private readonly IRepository<JobCandidateApplication> _jobApplicationRepository;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly ICvStorage _cvStorage;

        public JobCandidateApplicationService(
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

        public async Task<Result<int>> ApplyAsync(string userId, int jobId, Stream cv, string cvExtension)
        {
            // CandidateId is resolved from the authenticated user, never taken from the client.
            var candidate = _candidateRepository.Get().FirstOrDefault(c => c.UserId == userId);
            if (candidate is null)
                return Result<int>.Failure("Candidate profile not found.", ErrorKind.NotFound);

            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == jobId);
            if (job is null)
                return Result<int>.Failure("Job not found.", ErrorKind.NotFound);

            if (job.Status != JobStatus.Open)
                return Result<int>.Failure("This job is closed and no longer accepts applications.");

            var alreadyApplied = _jobApplicationRepository.Get()
                .Any(a => a.CandidateId == candidate.Id && a.JobId == job.Id);
            if (alreadyApplied)
                return Result<int>.Failure("You have already applied to this job.");

            var cvUrl = await _cvStorage.SaveAsync(cv, cvExtension);

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

        public IEnumerable<JobApplicationDto> GetMy(string userId)
        {
            return _jobApplicationRepository.Get()
                .Where(a => a.Candidate.UserId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(ToDto)
                .ToList();
        }

        public Result<List<JobApplicationDto>> GetForJob(int jobId, string recruiterId)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == jobId);
            if (job is null)
                return Result<List<JobApplicationDto>>.Failure("Job not found.", ErrorKind.NotFound);

            if (job.RecruiterId != recruiterId)
                return Result<List<JobApplicationDto>>.Failure(
                    "You can only view applications of your own jobs.", ErrorKind.Forbidden);

            // All statuses are returned (including Cancelled) - closing a job never hides its applications.
            var applications = _jobApplicationRepository.Get()
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(ToDto)
                .ToList();

            return Result<List<JobApplicationDto>>.Success(applications);
        }

        public async Task<Result<int>> UpdateStatus(int id, JobApplicationStatus status, string recruiterId)
        {
            var jobApplication = _jobApplicationRepository.Get().FirstOrDefault(a => a.Id == id);
            if (jobApplication is null)
                return Result<int>.Failure("Application not found.", ErrorKind.NotFound);

            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == jobApplication.JobId);
            if (job is null || job.RecruiterId != recruiterId)
                return Result<int>.Failure(
                    "You can only change the status of applications on your own jobs.", ErrorKind.Forbidden);

            try
            {
                jobApplication.UpdateStatus(status);
            }
            catch (InvalidOperationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }

            await _jobApplicationRepository.SaveChangesAsync();
            return Result<int>.Success(jobApplication.Id);
        }

        public async Task<Result<JobApplicationDto>> CancelAsync(int id, string userId)
        {
            var jobApplication = _jobApplicationRepository.Get().FirstOrDefault(a => a.Id == id);
            if (jobApplication is null)
                return Result<JobApplicationDto>.Failure("Application not found.", ErrorKind.NotFound);

            // Ownership is checked on the server: the application must belong to the logged-in candidate.
            var isOwner = _candidateRepository.Get()
                .Any(c => c.Id == jobApplication.CandidateId && c.UserId == userId);
            if (!isOwner)
                return Result<JobApplicationDto>.Failure(
                    "You can only cancel your own applications.", ErrorKind.Forbidden);

            if (jobApplication.JobApplicationStatus == JobApplicationStatus.Cancelled)
                return Result<JobApplicationDto>.Failure("This application is already cancelled.");

            if (jobApplication.JobApplicationStatus is not (JobApplicationStatus.Applied or JobApplicationStatus.UnderReview))
                return Result<JobApplicationDto>.Failure("Only Applied or UnderReview applications can be cancelled.");

            jobApplication.Cancel(); // soft cancel: Status = Cancelled + CancelledAt, row is kept
            await _jobApplicationRepository.SaveChangesAsync();

            var dto = _jobApplicationRepository.Get().Where(a => a.Id == id).Select(ToDto).First();
            return Result<JobApplicationDto>.Success(dto);
        }

        private static readonly Expression<Func<JobCandidateApplication, JobApplicationDto>> ToDto = a => new JobApplicationDto
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
