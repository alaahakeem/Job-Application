using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob
{
    public class CreateJobCommand : IRequest<int>
    {
        public string Title { get; }
        public string Description { get; }
        public string RecruiterId { get; }

        public CreateJobCommand(string title, string description, string recruiterId)
        {
            Title = title;
            Description = description;
            RecruiterId = recruiterId;
        }
    }
}
