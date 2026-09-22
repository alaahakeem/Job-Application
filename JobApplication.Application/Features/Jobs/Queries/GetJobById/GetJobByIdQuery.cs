using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Queries.GetJobById
{
    public class GetJobByIdQuery : IRequest<JobDto?>
    {
        public int Id { get; }

        public GetJobByIdQuery(int id)
        {
            Id = id;
        }
    }
}
