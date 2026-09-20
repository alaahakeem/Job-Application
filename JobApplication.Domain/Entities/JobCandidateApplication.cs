using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public  class JobCandidateApplication
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; }
        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }
        public JobApplicationStatus JobApplicationStatus { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime StatusUpdatedAt { get; set; }
        public string CvUrl { get; set; } = string.Empty;
        public DateTime? CancelledAt { get; set; }

        public JobCandidateApplication()
        {
            JobApplicationStatus = JobApplicationStatus.Applied;
            AppliedAt = DateTime.UtcNow;
            StatusUpdatedAt = AppliedAt;
        }


        private static readonly Dictionary<JobApplicationStatus, JobApplicationStatus[]> AllowedTransitions = new()
        {
            [JobApplicationStatus.Applied] = new[] { JobApplicationStatus.UnderReview },
            [JobApplicationStatus.UnderReview] = new[] { JobApplicationStatus.InterView },
            [JobApplicationStatus.InterView] = new[] { JobApplicationStatus.Accepted, JobApplicationStatus.Rejected },
            [JobApplicationStatus.Accepted] = Array.Empty<JobApplicationStatus>(),
            [JobApplicationStatus.Rejected] = Array.Empty<JobApplicationStatus>(),
            [JobApplicationStatus.Cancelled] = Array.Empty<JobApplicationStatus>(),
        };

        public void UpdateStatus(JobApplicationStatus newStatus)
        {
            if (!AllowedTransitions[JobApplicationStatus].Contains(newStatus))
            {
                throw new InvalidOperationException($"Cannot change status from '{JobApplicationStatus}' to '{newStatus}'."); 
            }

            JobApplicationStatus = newStatus;
            StatusUpdatedAt = DateTime.UtcNow;
        }

        // Only Applied / UnderReview can be cancelled. The row is kept (never deleted) and cannot be reopened.
        public void Cancel()
        {
            if (JobApplicationStatus != JobApplicationStatus.Applied &&
                JobApplicationStatus != JobApplicationStatus.UnderReview)
            {
                throw new InvalidOperationException($"Cannot cancel an application in status '{JobApplicationStatus}'.");
            }

            JobApplicationStatus = JobApplicationStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            StatusUpdatedAt = CancelledAt.Value;
        }
    }
}
