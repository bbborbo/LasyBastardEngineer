using RoR2;
using System;
using System.Collections.Generic;

namespace LasyBastardEngineer
{
    internal static class UnlockableCompatibility
    {
        internal static void Init()
        {
            On.RoR2.UnlockableCatalog.SetUnlockableDefs += UnlockableCatalog_SetUnlockableDefs;
            On.RoR2.NetworkUser.CallCmdSendNewUnlockables += NetworkUser_CallCmdSendNewUnlockables;
            On.RoR2.NetworkUser.CallCmdReportUnlock += NetworkUser_CallCmdReportUnlock;
        }

        private static void UnlockableCatalog_SetUnlockableDefs(On.RoR2.UnlockableCatalog.orig_SetUnlockableDefs orig, UnlockableDef[] newUnlockableDefs)
        {
            // Keep every other unlock's index unchanged when this optional skin is installed.
            List<UnlockableDef> orderedUnlockables = new List<UnlockableDef>(newUnlockableDefs);
            if (orderedUnlockables.Remove(LazyBastardPlugin.unlock))
            {
                orderedUnlockables.Add(LazyBastardPlugin.unlock);
            }

            orig(orderedUnlockables.ToArray());
        }

        private static void NetworkUser_CallCmdSendNewUnlockables(On.RoR2.NetworkUser.orig_CallCmdSendNewUnlockables orig, NetworkUser self, UnlockableIndex[] newUnlockableIndices)
        {
            // Other players may not have this skin, so only send the remaining unlocks.
            UnlockableIndex[] sharedUnlockables = Array.FindAll(newUnlockableIndices, index => index != LazyBastardPlugin.unlock.index);
            orig(self, sharedUnlockables);
        }

        private static void NetworkUser_CallCmdReportUnlock(On.RoR2.NetworkUser.orig_CallCmdReportUnlock orig, NetworkUser self, UnlockableIndex unlockIndex)
        {
            // Skip only the network report; the achievement still grants and saves the skin locally.
            if (unlockIndex == LazyBastardPlugin.unlock.index)
            {
                return;
            }

            orig(self, unlockIndex);
        }
    }
}
