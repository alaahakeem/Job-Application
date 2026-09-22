using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob
{
    public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, int>
    {
        private readonly IRepository<Job> _jobRepository;

        public CreateJobCommandHandler(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            var job = new Job
            {
                Title = request.Title,
                Description = request.Description,
                RecruiterId = request.RecruiterId
            };

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id;
        }
    }
}
