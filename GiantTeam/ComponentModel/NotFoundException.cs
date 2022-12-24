namespace GiantTeam.ComponentModel
{
    public class NotFoundException : StatusCodeException
    {
        public NotFoundException()
            : base(404, "Not Found", "The requested resource was not found.")
        {
        }
    }
}
