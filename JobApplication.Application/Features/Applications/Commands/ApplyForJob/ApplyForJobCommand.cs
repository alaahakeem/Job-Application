using JobApplication.Application.Common;
using MediatR;

namespace JobApplication.Application.Features.Applications.Commands.ApplyForJob
{
    // Cv is a Stream (not IFormFile): the Application layer must stay free of Microsoft.AspNetCore.Http.
    // The controller is responsible for validating size/extension/signature before this is sent.
    public class ApplyForJobCommand : IRequest<Result<int>>
    {
        public string UserId { get; }
        public int JobId { get; }
        public Stream Cv { get; }
        public string CvExtension { get; }

        public ApplyForJobCommand(string userId, int jobId, Stream cv, string cvExtension)
        {
            UserId = userId;
            JobId = jobId;
            Cv = cv;
            CvExtension = cvExtension;
        }
    }
}
