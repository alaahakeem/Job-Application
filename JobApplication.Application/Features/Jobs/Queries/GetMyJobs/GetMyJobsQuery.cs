using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries.GetMyJobs
{
    public class GetMyJobsQuery : IRequest<IEnumerable<JobDto>>
    {
        public string RecruiterId { get; }

        public GetMyJobsQuery(string recruiterId)
        {
            RecruiterId = recruiterId;
        }
    }
}
