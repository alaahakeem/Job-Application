using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IRepository<Job> _jobRepository;

        public JobService(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId)
        {   
            var job = new Job()
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                RecruiterId = recruiterId
            };
            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id; 
        }

        public IEnumerable<JobDto> GetOpen()
        {
            return _jobRepository.Get()
                .Where(j => j.Status == JobStatus.Open)
                .OrderByDescending(j => j.Id)
                .ToList()
                .Select(ToDto);
        }

        public JobDto? GetById(int id)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == id);
            return job is null ? null : ToDto(job);
        }

        public IEnumerable<JobDto> GetByRecruiter(string recruiterId)
        {
            return _jobRepository.Get()
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.Id)
                .ToList()
                .Select(ToDto);
        }

        public async Task<Result<JobDto>> CloseAsync(int id, string recruiterId)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == id);
            if (job is null)
                return Result<JobDto>.Failure("Job not found.", ErrorKind.NotFound);

            if (job.RecruiterId != recruiterId)
                return Result<JobDto>.Failure("You can only close your own jobs.", ErrorKind.Forbidden);

            if (job.Status != JobStatus.Open)
                return Result<JobDto>.Failure("Only an open job can be closed.");

            job.Close(); // soft close: Status = Closed + ClosedAt. Applications are untouched.
            await _jobRepository.SaveChangesAsync();

            return Result<JobDto>.Success(ToDto(job));
        }

        private static JobDto ToDto(Job job) => new()
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Status = job.Status.ToString(),
            ClosedAt = job.ClosedAt
        };
    }
}
