using Microsoft.EntityFrameworkCore;

namespace GiantTeam.RecordsManagement.Data
{
    [PrimaryKey(nameof(TeamId), nameof(UserId))]
    public class TeamUser
    {
        public Guid TeamId { get; set; }
        public Team? Team { get; private set; }

        public Guid UserId { get; set; }
        public User? User { get; private set; }

        public DateTimeOffset Created { get; set; }
    }
}