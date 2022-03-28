using System;
using System.Collections.Generic;
using System.Text;
using LasyBastardEngineer;
using RoR2;
using UnityEngine;

namespace LasyBastardEngineer.Achievements
{
    class LazyBastardSkinUnlock : ModdedUnlockable
    {
        public static string tokenName = "LBESKIN";
        public static string UnlockName = "Engineer: Lazy Bastard";
        public static string UnlockDesc = "As Engineer, win the game without using your Primary, Secondary, or Utility skills.";

        public override string AchievementIdentifier { get; } = Base.modPrefix + tokenName + "_ACHIEVEMENT_ID";
        public override string UnlockableIdentifier { get; } = Base.modPrefix + tokenName + "_REWARD_ID";
        public override string PrerequisiteUnlockableIdentifier { get; } = "Complete30StagesCareer";
        public override string AchievementNameToken { get; } = Base.modPrefix + tokenName + "_ACHIEVEMENT_NAME";
        public override string AchievementDescToken { get; } = Base.modPrefix + tokenName + "_ACHIEVEMENT_DESC";
        public override string UnlockableNameToken { get; } = Base.modPrefix + tokenName + "_UNLOCKABLE_NAME";

        public override Sprite Sprite => Base.skinBundle.LoadAsset<Sprite>("icon.png");

        public override Func<string> GetHowToUnlock { get; } = 
        (() => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", new object[]
        {
            Language.GetString(Base.modPrefix + tokenName + "_ACHIEVEMENT_NAME"),
            Language.GetString(Base.modPrefix + tokenName + "_ACHIEVEMENT_DESC")
        }));
        public override Func<string> GetUnlocked { get; } = 
        (() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
        {
            Language.GetString(Base.modPrefix + tokenName + "_ACHIEVEMENT_NAME"),
            Language.GetString(Base.modPrefix + tokenName + "_ACHIEVEMENT_DESC")
        })); 

        int skillUseCount = 0;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex(Base.engiBodyPrefab);
        }

        private void SkillCheck(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            if (self.bodyIndex == LookUpRequiredBodyIndex() && self.teamComponent.teamIndex == TeamIndex.Player)
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
