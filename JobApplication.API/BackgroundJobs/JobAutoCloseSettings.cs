namespace JobApplication.API.BackgroundJobs
{
    /// <summary>Bound from the "JobAutoClose" section of appsettings.json.</summary>
    public class JobAutoCloseSettings
    {
        public const string SectionName = "JobAutoClose";

        /// <summary>The id shown in the Hangfire Dashboard (Recurring Jobs tab). Same id = same job (AddOrUpdate).</summary>
        public const string RecurringJobId = "close-stale-jobs";

        /// <summary>A job that stays Open longer than this many days is closed automatically.</summary>
        public int MaxOpenDays { get; set; } = 30;

        /// <summary>
        /// Cron expression (minute hour day-of-month month day-of-week), evaluated in UTC.
        /// "0 2 * * *" = every day at 02:00 UTC.
        /// </summary>
        public string Cron { get; set; } = "0 2 * * *";
    }
}
