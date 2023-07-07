using System.Collections.Concurrent;
using yabm;

var concurrency = 1;
var threadBeatmapIds = new int[concurrency];
var beatmaps = new ConcurrentQueue<int>(GetBeatmaps());
var tasks = new Task[concurrency];
            
int totalBeatmaps = beatmaps.Count;
int processedBeatmaps = 0;

for (int i = 0; i < concurrency; i++)
{
    int tmp = i;
    tasks[i] = Task.Factory.StartNew(() =>
    {
        var calculator = new DifficultyCalculator();
        while (beatmaps.TryDequeue(out int beatmapId))
        {
            threadBeatmapIds[tmp] = beatmapId;
            Console.WriteLine($"Processing difficulty for beatmap {beatmapId}.");

            try
            {
                var beatmap = LoaderWorkingBeatmap.GetBeatmap(beatmapId);

                // ensure the correct online id is set
                beatmap.BeatmapInfo.OnlineID = beatmapId;

                calculator.ProcessBeatmap(beatmap);
                            
                Console.WriteLine($"Difficulty updated for beatmap {beatmapId}.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{beatmapId} failed with: {e.Message}");
            }

            Interlocked.Increment(ref processedBeatmaps);
        }
    });
                
    Console.WriteLine($"Processing {totalBeatmaps} beatmaps.");
    Task.WaitAll(tasks);
    Console.WriteLine("Done.");
}

static IEnumerable<int> GetBeatmaps()
{
    var ids = new List<int>();

    foreach (var f in Directory.GetFiles(AppSettings.BEATMAPS_PATH))
    {
        var filename = Path.GetFileNameWithoutExtension(f);

        if (int.TryParse(filename.Split(' ')[0], out var id))
            ids.Add(id);
    }

    return ids;
}

