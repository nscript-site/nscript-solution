using LiteDB;
using NScript.LiteDB;

namespace NScript.Supervisor.Data;

[LiteDBSet(FileName = "process_start_data.db", Indexer = typeof(ProcessLastStartDataIndexer))]
public class ProcessLastStartData
{
    public class ProcessLastStartDataIndexer : Indexer<ProcessLastStartData>
    {
        public override void EnsureIndex(ILiteCollection<ProcessLastStartData> col)
        {
            col.EnsureIndex(x => x.Id);
            col.EnsureIndex(x => x.ConfigName);
        }
    }

    public string Id { get; set; } = string.Empty;
    public string ConfigName { get; set; }
    public int ProcessId { get; set; }
    public long StartTimeTicks { get; set; }
}

public class ProcessLastStartDataService
{
    private static DataService<ProcessLastStartData> Service = new DataService<ProcessLastStartData>();

    public static ProcessLastStartData? Find(string configName)
    {
        return Service.FindOne(x=>x.ConfigName == configName);
    }

    public static void Update(string configName, int  processId, long startTime)
    {
        bool find = Service.UpdateOne(x => x.ConfigName == configName, x => { x.ProcessId = processId; x.StartTimeTicks = startTime; });
        if (find == false)
        {
            var newItem = new ProcessLastStartData() { Id = Guid.NewGuid().ToString("N"), ConfigName = configName, ProcessId = processId, StartTimeTicks = startTime };
            Service.Insert(newItem);
        }
    }
}