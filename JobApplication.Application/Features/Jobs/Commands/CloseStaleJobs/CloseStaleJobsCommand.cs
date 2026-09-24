using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseStaleJobs
{
    /// <summary>
    /// Closes every job that has been Open for more than <see cref="MaxOpenDays"/> days.
    /// Returns how many jobs were closed. Sent by the Hangfire recurring job (no user involved).
    /// </summary>
    public class CloseStaleJobsCommand : IRequest<int>
    {
        public int MaxOpenDays { get; }

        public CloseStaleJobsCommand(int maxOpenDays)
        {
            // Guard: 0 or a negative value would close every open job.
            if (maxOpenDays <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxOpenDays), "MaxOpenDays must be greater than 0.");

            MaxOpenDays = maxOpenDays;
        }
    }
}
