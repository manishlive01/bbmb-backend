using BBMB_Budget.Features.Geographical.Districts.Entities;
using BBMB_Budget.Features.Geographical.States.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Infrastructure.Persistence
{
    public class BBMBContext : DbContext
    {
        public BBMBContext(DbContextOptions<BBMBContext> options)
            : base(options)
        {
        }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<DistrictMaster> DistrictMasters { get; set; }
    }
}
