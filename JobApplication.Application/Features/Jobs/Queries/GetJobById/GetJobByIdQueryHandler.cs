using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries.GetJobById
{
    public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto?>
    {
        private readonly IRepository<Job> _jobRepository;

        public GetJobByIdQueryHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public Task<JobDto?> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == request.Id);
            return Task.FromResult(job is null ? null : JobMapper.ToDto(job));
        }
    }
}
