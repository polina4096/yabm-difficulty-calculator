using System.Reflection;
using Dapper;
using MySqlConnector;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace yabm;

public class DifficultyCalculator
{
    private static readonly List<Ruleset> AvailableRulesets = GetRulesets();
    private readonly List<Ruleset> _processableRulesets = new();

    public DifficultyCalculator()
    {
        _processableRulesets.AddRange(AvailableRulesets);
    }

    public void ProcessBeatmap(WorkingBeatmap beatmap)
    {
        int beatmapId = beatmap.BeatmapInfo.OnlineID;

        try
        {
            using var conn = Database.GetConnection();
            if (_processableRulesets.Any(r => r.RulesetInfo.OnlineID == beatmap.BeatmapInfo.Ruleset.OnlineID))
                ComputeDifficulty(beatmapId, beatmap, beatmap.BeatmapInfo.Ruleset.CreateInstance(), conn);
        }
        catch (Exception e)
        {
            throw new Exception($"{beatmapId} failed with: {e.Message}");
        }
    }

    private static void ComputeDifficulty(int beatmapId, WorkingBeatmap beatmap, Ruleset ruleset, MySqlConnection conn)
    {
        var attribute = ruleset.CreateDifficultyCalculator(beatmap).Calculate();

        conn.Execute(
              "INSERT INTO `difficulty` (`id`, `beatmapId`, `name`, `starRating`) "
            + "VALUES (@Id, @BeatmapId, @Name, @StarRating) "
            + "ON DUPLICATE KEY UPDATE `starRating` = @StarRating",
            new
            {
                Id = beatmapId,
                BeatmapId = beatmap.BeatmapInfo!.BeatmapSet!.OnlineID,
                Name = beatmap.BeatmapInfo!.DifficultyName,
                StarRating = attribute.StarRating
            });
    }

    private static List<Ruleset> GetRulesets()
    {
        const string rulesetLibraryPrefix = "osu.Game.Rulesets";

        var rulesetsToProcess = new List<Ruleset>();
        foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, $"{rulesetLibraryPrefix}.*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                Type type = assembly.GetTypes().First(t => t.IsPublic && t.IsSubclassOf(typeof(Ruleset)));
                rulesetsToProcess.Add((Ruleset)Activator.CreateInstance(type)!);
            }
            catch
            {
                throw new Exception($"Failed to load ruleset ({file})");
            }
        }

        return rulesetsToProcess;
    }
}
