using LasyBastardEngineer;
using RoR2;
using System;
using UnityEngine;

namespace LasyBastardEngineer.Achievements
{

    class LazyBastardSkinUnlockAchievement : ModdedUnlockable
    {
        public override String AchievementIdentifier { get; } = "LASYBASTARD_SKINUNLOCKABLE_ACHIEVEMENT_ID";
        public override String UnlockableIdentifier { get; } = "LASYBASTARD_SKINUNLOCKABLE_REWARD_ID";
        public override String PrerequisiteUnlockableIdentifier { get; } = "Complete30StagesCareer";
        public override String AchievementNameToken { get; } = "LASYBASTARD_SKINUNLOCKABLE_ACHIEVEMENT_NAME";
        public override String AchievementDescToken { get; } = "LASYBASTARD_SKINUNLOCKABLE_ACHIEVEMENT_DESC";
        public override String UnlockableNameToken { get; } = "LASYBASTARD_SKINUNLOCKABLE_UNLOCKABLE_NAME";

        public override Sprite Sprite => throw new NotImplementedException();

        public override Func<string> GetHowToUnlock => throw new NotImplementedException();

        public override Func<string> GetUnlocked => throw new NotImplementedException();

        int skillUseCount = 0;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Base.engiBodyPrefab);
        }

        private void SkillCheck(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            if(self.bodyIndex == LookUpRequiredBodyIndex() && self.teamComponent.teamIndex == TeamIndex.Player)
            {
                if (skill != self.skillLocator.special)
                {
                    if (skillUseCount == 0)
                    {
                        Debug.Log("DEBUG: Lazy Bastard challenge failed.");
                        if (Base.AnnounceWhenFail.Value)
                            Chat.AddMessage("Lazy Bastard challenge failed!");
                    }

                    skillUseCount++;
                    //Debug.Log(skillUseCount);
                }
            }
            orig(self, skill);
        }

        private void ResetSkillUseCount(Run obj)
        {
            skillUseCount = 0;
        }

        public void ClearCheck(Run run, RunReport runReport)
        {
            bool isLazy = skillUseCount == 0 && base.meetsBodyRequirement;
            skillUseCount = 0;

            if (run is null) return;
            if (runReport is null) return;

            if (!runReport.gameEnding) return;


            if (runReport.gameEnding.isWin)
            {
                if (isLazy)
                {
                    base.Grant();
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();

            Run.onRunStartGlobal += this.ResetSkillUseCount;
            Run.onClientGameOverGlobal += this.ClearCheck;
            On.RoR2.CharacterBody.OnSkillActivated += this.SkillCheck;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            Run.onRunStartGlobal += this.ResetSkillUseCount;
            Run.onClientGameOverGlobal -= this.ClearCheck;
            On.RoR2.CharacterBody.OnSkillActivated -= this.SkillCheck;
        }
    }
}
