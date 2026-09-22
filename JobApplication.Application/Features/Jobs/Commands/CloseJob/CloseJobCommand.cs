using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob
{
    public class CloseJobCommand : IRequest<Result<JobDto>>
    {
        public int Id { get; }
        public string RecruiterId { get; }

        public CloseJobCommand(int id, string recruiterId)
        {
            Id = id;
            RecruiterId = recruiterId;
        }
    }
}
