namespace GiantTeam.ComponentModel
{
    public class UnelevatedException : UnauthorizedException
    {
        public UnelevatedException()
            : base("Elevated rights are required to perform this action. Please login with elevated rights, and try again.")
        {
        }
    }
}
