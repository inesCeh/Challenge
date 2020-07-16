using Microsoft.EntityFrameworkCore;

namespace ChallengeApi.Models
{
    public class MetricContext : DbContext
    {
        public MetricContext(DbContextOptions<MetricContext> options)
            : base(options)
        {
        }

        public DbSet<MetricItem> MetricItems { get; set; }
    }
}