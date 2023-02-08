using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace LasyBastardEngineer
{
    [RegisterAchievement("LazyBastardEngineer", "Skins.Engineer.LazyBastard", "Complete30StagesCareer", null)]
    public class Achievement : BaseAchievement
    {
        private int skillUseCount = 0;
        public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex("EngiBody");
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            Run.onRunStartGlobal += ResetSkillUseCount;
            Run.onClientGameOverGlobal += ClearCheck;
            On.RoR2.CharacterBody.OnSkillActivated += SkillCheck;
        }

        public override void OnBodyRequirementBroken()
        {
            Run.onRunStartGlobal += ResetSkillUseCount;
            Run.onClientGameOverGlobal -= ClearCheck;
            On.RoR2.CharacterBody.OnSkillActivated -= SkillCheck;
            base.OnBodyRequirementBroken();
        }

        private void SkillCheck(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            if (self.bodyIndex == LookUpRequiredBodyIndex() && self.teamComponent.teamIndex == TeamIndex.Player && skill != self.skillLocator.special)
            {
                if (skillUseCount == 0)
                {
                    Debug.Log("DEBUG: Lazy Bastard challenge failed.");
                    if (Base.AnnounceWhenFail.Value) Chat.AddMessage("Lazy Bastard challenge failed!");
                }
                ++skillUseCount;
            }
            orig(self, skill);
        }

        private void ResetSkillUseCount(Run obj) => skillUseCount = 0;

        public void ClearCheck(Run run, RunReport runReport)
        {
            bool flag = skillUseCount == 0 && meetsBodyRequirement;
            skillUseCount = 0;
            if (run == null || runReport == null || !(bool)runReport.gameEnding || !runReport.gameEnding.isWin || !flag) return;
            Grant();
        }
    }
}
