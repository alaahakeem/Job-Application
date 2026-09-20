using JobApplication.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description  { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Open;
        public DateTime? ClosedAt { get; set; }
        public string? RecruiterId { get; set; }

        public void Close()
        {
            if (Status != JobStatus.Open)
                throw new InvalidOperationException("Only an open job can be closed.");

            Status = JobStatus.Closed;
            ClosedAt = DateTime.UtcNow;
        }
    }
}
