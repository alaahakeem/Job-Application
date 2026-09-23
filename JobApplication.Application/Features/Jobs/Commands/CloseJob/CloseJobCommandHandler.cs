using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand, Result<JobDto>>
    {
        private readonly IRepository<Job> _jobRepository;

        public CloseJobCommandHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Result<JobDto>> Handle(CloseJobCommand request, CancellationToken cancellationToken)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == request.Id);
            if (job is null)
                return Result<JobDto>.Failure("Job not found.", ErrorKind.NotFound);

            if (job.RecruiterId != request.RecruiterId)
                return Result<JobDto>.Failure("You can only close your own jobs.", ErrorKind.Forbidden);

            if (job.Status != JobStatus.Open)
                return Result<JobDto>.Failure("Only an open job can be closed.");

            job.Close(); // soft close: Status = Closed + ClosedAt. Applications are untouched.
            await _jobRepository.SaveChangesAsync();

            return Result<JobDto>.Success(JobMapper.ToDto(job));
        }
    }
}
