using JobApplication.Application.Features.Jobs.Commands.CloseStaleJobs;
using MediatR;
using Microsoft.Extensions.Options;

namespace JobApplication.API.BackgroundJobs
{
    /// <summary>
    /// The class Hangfire calls on the schedule. It is only a thin wrapper:
    /// the real logic lives in CloseStaleJobsCommandHandler (same CQRS style as the rest of the project).
    /// Hangfire creates it through DI inside a new scope for every run, so scoped services (DbContext, repositories) are safe here.
    /// </summary>
    public class CloseStaleJobsJob
    {
        private readonly IMediator _mediator;
        private readonly JobAutoCloseSettings _settings;
        private readonly ILogger<CloseStaleJobsJob> _logger;

        public CloseStaleJobsJob(
            IMediator mediator,
            IOptions<JobAutoCloseSettings> settings,
            ILogger<CloseStaleJobsJob> logger)
        {
            _mediator = mediator;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var closedCount = await _mediator.Send(new CloseStaleJobsCommand(_settings.MaxOpenDays), cancellationToken);

            _logger.LogInformation(
                "Auto-close finished: {ClosedCount} job(s) were open for more than {MaxOpenDays} days and got closed.",
                closedCount, _settings.MaxOpenDays);
        }
    }
}
