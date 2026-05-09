using System.Collections.Generic;
using System.Linq;

public class GraphAnalyzer
{
    public List<ClassData> AllClasses { get; private set; }
    public HashSet<(string, string)> Cycles { get; private set; }

    // プロジェクト全体をスキャンして解析を実行
    public void Refresh()
    {
        AllClasses = ScriptScanner.GetProjectClasses();
        Cycles = FindCycles(AllClasses);
    }

    private HashSet<(string, string)> FindCycles(List<ClassData> classes)
    {
        var cycles = new HashSet<(string, string)>();
        var visited = new HashSet<string>();
        var recStack = new HashSet<string>();
        var map = classes.ToDictionary(c => c.FullName);

        void DFS(string name)
        {
            if (recStack.Contains(name)) return;
            if (visited.Contains(name)) return;

            visited.Add(name);
            recStack.Add(name);

            if (map.TryGetValue(name, out var c))
            {
                foreach (var dep in c.Dependencies)
                {
                    if (!map.ContainsKey(dep.FullName)) continue;
                    if (recStack.Contains(dep.FullName)) cycles.Add((name, dep.FullName));
                    else DFS(dep.FullName);
                }
            }
            recStack.Remove(name);
        }

        foreach (var c in classes) DFS(c.FullName);
        return cycles;
    }
}