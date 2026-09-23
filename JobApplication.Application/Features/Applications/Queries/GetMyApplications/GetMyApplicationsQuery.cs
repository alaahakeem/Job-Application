using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Applications.Queries.GetMyApplications
{
    public class GetMyApplicationsQuery : IRequest<IEnumerable<JobApplicationDto>>
    {
        public string UserId { get; }

        public GetMyApplicationsQuery(string userId)
        {
            UserId = userId;
        }
    }
}
