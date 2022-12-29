using System.Text.Json;

namespace GiantTeam.Text
{
    public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            string newName = TextTransformers.Snakify(name);
            return newName;
        }
    }
}
