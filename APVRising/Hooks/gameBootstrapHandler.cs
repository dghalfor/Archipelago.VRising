using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APVRising.Hooks;
[HarmonyPatch(typeof(GameBootstrap))]
public static class GameBootstrapPatches
{
    [HarmonyPatch(nameof(GameBootstrap.GetServerSaveSettings))]
    [HarmonyPostfix]
    public static void GetServerSaveSettings_Postfix(
        string serverSaveName,
        string serverSaveFileName,
        ServerHostSettings serverHostSettings,
        ulong platformId,
        ref string saveDirectoryPath,
        ref string saveToLoadOnStart)
    {
        Plugin.ServerSaveName = serverSaveName;
    }
}