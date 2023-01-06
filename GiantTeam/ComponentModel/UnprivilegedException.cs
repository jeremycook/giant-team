namespace GiantTeam.ComponentModel
{
    public class UnprivilegedException : UnauthorizedException
    {
        public UnprivilegedException()
            : base("Elevated rights are required to perform this action. Please login with elevated rights, and try again.")
        {
        }
    }
}
