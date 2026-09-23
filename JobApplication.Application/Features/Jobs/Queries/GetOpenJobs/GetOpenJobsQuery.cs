using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries.GetOpenJobs
{
    public class GetOpenJobsQuery : IRequest<IEnumerable<JobDto>>
    {
    }
}
