using JobApplication.Application.Common;
using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Applications.Queries.GetApplicationsForJob
{
    public class GetApplicationsForJobQuery : IRequest<Result<List<JobApplicationDto>>>
    {
        public int JobId { get; }
        public string RecruiterId { get; }

        public GetApplicationsForJobQuery(int jobId, string recruiterId)
        {
            JobId = jobId;
            RecruiterId = recruiterId;
        }
    }
}
