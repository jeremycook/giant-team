namespace GiantTeam.Charting
{
    public static class ColorMaker
    {
        public static IEnumerable<string> HslRange(int count, int h = 153, int s = 80, int l = 50)
        {
            var stepSize = Math.Max(1, 360 / count);
            for (int i = 0; i < count; i++)
            {
                h += stepSize;
                if (h > 360)
                {
                    h -= 360;
                }
                yield return $"hsl({h},{s}%,{l}%)";
            }
        }
    }
}
