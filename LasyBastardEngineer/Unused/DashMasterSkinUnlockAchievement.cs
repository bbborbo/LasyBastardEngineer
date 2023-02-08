using LasyBastardEngineer;
using RoR2;
using EntityStates.Merc;
using System;
using UnityEngine;

namespace LasyBastardEngineer.Achievements
{
    /*[R2APISubmoduleDependency(nameof(UnlockablesAPI))]

    class DashMasterSkinUnlockAchievement : ModdedUnlockableAndAchievement<CustomSpriteProvider>
    {
        public override String AchievementIdentifier { get; } = "DASHMASTER_SKINUNLOCKABLE_ACHIEVEMENT_ID";
        public override String UnlockableIdentifier { get; } = "DASHMASTER_SKINUNLOCKABLE_REWARD_ID";
        public override String PrerequisiteUnlockableIdentifier { get; } = "";
        public override String AchievementNameToken { get; } = "DASHMASTER_SKINUNLOCKABLE_ACHIEVEMENT_NAME";
        public override String AchievementDescToken { get; } = "DASHMASTER_SKINUNLOCKABLE_ACHIEVEMENT_DESC";
        public override String UnlockableNameToken { get; } = "DASHMASTER_SKINUNLOCKABLE_UNLOCKABLE_NAME";
        protected override CustomSpriteProvider SpriteProvider { get; } = new CustomSpriteProvider("");

        int requiredChainDashCount = 800;
        int currentChainDashCount = 0;

        private void ChainDashCheck(On.EntityStates.Merc.Assaulter2.orig_OnExit orig, EntityStates.Merc.Assaulter2 self)
        {
            if (base.meetsBodyRequirement)
            {
                if (self.grantAnotherDash == true)
                {
                    currentChainDashCount++;
                    if (currentChainDashCount >= requiredChainDashCount)
                    {
                        base.Grant();
                    }
                }
                else
                {
                    currentChainDashCount = 0;
                }
            }

            orig(self);
        }

        public override int LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("MercBody");
        }

        private void ResetChainDashCount(Run obj)
        {
            currentChainDashCount = 0;
        }

        public override void OnInstall()
        {
            base.OnInstall();

            Run.onRunStartGlobal += this.ResetChainDashCount;
            On.EntityStates.Merc.Assaulter2.OnExit += this.ChainDashCheck;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            Run.onRunStartGlobal += this.ResetChainDashCount;
            On.EntityStates.Merc.Assaulter2.OnExit -= this.ChainDashCheck;
        }
    }*/
}
