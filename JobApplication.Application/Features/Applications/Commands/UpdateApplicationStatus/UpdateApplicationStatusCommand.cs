using JobApplication.Application.Common;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.UpdateApplicationStatus
{
    public class UpdateApplicationStatusCommand : IRequest<Result<int>>
    {
        public int Id { get; }
        public JobApplicationStatus Status { get; }
        public string RecruiterId { get; }

        public UpdateApplicationStatusCommand(int id, JobApplicationStatus status, string recruiterId)
        {
            Id = id;
            Status = status;
            RecruiterId = recruiterId;
        }
    }
}
