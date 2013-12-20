using System;

namespace OSSFinder.Models
{
    public class StatisticsReport
    {
        public string Content { get; private set; }
        public DateTime? LastUpdatedUtc { get; private set; }

        public StatisticsReport(string content, DateTime? lastUpdatedUtc)
        {
            Content = content;
            LastUpdatedUtc = lastUpdatedUtc;
        }
    }
}