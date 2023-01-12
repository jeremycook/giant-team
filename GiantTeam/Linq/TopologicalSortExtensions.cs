namespace GiantTeam.Linq;

public static class TopologicalSortExtensions
{
    public static IEnumerable<T> TopologicalSort<T>(this IEnumerable<T> datums, IEnumerable<(T, T)> edges) where T : notnull
    {
        // Empty list that will contain the sorted elements
        var sortedList = new List<T>();

        // A copy of edges that will be manipulated
        var graph = edges.ToHashSet();

        // The set of all datums with no incoming edges
        var set = datums.Except(graph.Select(e => e.Item2)).ToHashSet(); //.Where(n => edges.All(e => !e.Item2.Equals(n))).ToHashSet();

        // While set is not empty do
        while (set.Any())
        {
            // Remove datum n from set
            var n = set.First();
            set.Remove(n);

            // Add n to tail of sorted list
            sortedList.Add(n);

            // For each datum m with an edge e from n to m do
            foreach (var e in graph.Where(e => e.Item1.Equals(n)).ToList())
            {
                var m = e.Item2;

                // Remove edge e from the graph
                graph.Remove(e);

                // If m has no other incoming edges then
                if (graph.All(me => !me.Item2.Equals(m)))
                {
                    // insert m into set
                    set.Add(m);
                }
            }
        }

        if (graph.Any())
        {
            throw new ArgumentException($"The {nameof(edges)} argument contains a cycle.", nameof(edges));
        }
        else
        {
            return sortedList;
        }
    }
}
