using System.Collections.Generic;
using Newtonsoft.Json;

namespace APVRising.Archipelago;
public class ArchipelagoData
{
    public string Uri;
    public string SlotName;
    public string Password;
    public int Index;

    public List<long> CheckedLocations;
    public static List<int> APProgression = new List<int> { 507915220, -54738837 };
    public static List<int> ResearchedProgression = new List<int> { -632708133, -997169234, -212104516 };
    /// <summary>
    /// seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    /// load is valid to the room it's connecting to.
    /// </summary>
    private string seed;

    private Dictionary<string, object> slotData;

    public bool NeedSlotData => slotData == null;

    public ArchipelagoData()
    {
        Uri = "localhost";
        SlotName = "Daniel";
        CheckedLocations = new();
    }

    public static List<int> GetAPProgression()
    {
        return APProgression;
    }

    public static List<int> GetResearchProgression()
    {
        return ResearchedProgression;
    }
    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
        CheckedLocations = new();
    }

    /// <summary>
    /// assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public void SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        slotData = roomSlotData;
        seed = roomSeed;
    }

    /// <summary>
    /// returns the object as a json string to be written to a file which you can then load
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject($"{Uri} {SlotName} {Password} {Index} {seed} {slotData}");
    }
}