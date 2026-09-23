using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.CancelApplication
{
    public class CancelApplicationCommand : IRequest<Result<JobApplicationDto>>
    {
        public int Id { get; }
        public string UserId { get; }

        public CancelApplicationCommand(int id, string userId)
        {
            Id = id;
            UserId = userId;
        }
    }
}
