using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace APVRising.Archipelago;
public class ArchipelagoData
{
    public string Uri;
    public string SlotName;
    public string Password;
    public int Index;

    public List<long> CheckedLocations1;
    public static List<int> ReceivedChecks = new List<int>();
    public static List<int> CheckedLocations = new List<int>();
    public static HashSet<int> ConfiguredLocations = new HashSet<int>();

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
        CheckedLocations1 = new();
    }

    public static List<int> GetReceivedChecks()
    {
        return ReceivedChecks;
    }
    public static void AddLocationCheck(int value)
    {
        if (!CheckedLocations.Contains(value))
        {
            Plugin.BepinLogger.LogInfo($"Checked {value}");

            CheckedLocations.Add(value);
        }
    }

    public static void AddReceivedCheck(int value)
    {
        if (!ReceivedChecks.Contains(value))
        {
            Plugin.BepinLogger.LogInfo($"received {value}");

            ReceivedChecks.Add(value);
        }
    }
    public static List<int> GetCheckedLocations()
    {
        return CheckedLocations;
    }
    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
        CheckedLocations1 = new();
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
    public string SlotDataOpts ()
    {
        return slotData.TryGetValue("goal", out var goal) ? goal.ToString() : string.Empty;
    }
    public bool IsDeathLinkEnabled()
    {
        if (!slotData.TryGetValue("death_link", out var value)) return false;
        return Convert.ToBoolean(value);
    }
}