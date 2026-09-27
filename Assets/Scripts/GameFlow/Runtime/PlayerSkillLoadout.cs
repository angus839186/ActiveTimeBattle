using System.Collections.Generic;

public class PlayerSkillLoadout
{
    private readonly List<BattleSkillDefinition> ownedSkills =
        new List<BattleSkillDefinition>();

    private readonly Dictionary<BattleSkillSlot, BattleSkillDefinition> equippedSkills =
        new Dictionary<BattleSkillSlot, BattleSkillDefinition>();

    public IReadOnlyList<BattleSkillDefinition> OwnedSkills => ownedSkills;

    public bool AddOwnedSkill(BattleSkillDefinition skill)
    {
        if (skill == null || ownedSkills.Contains(skill))
        {
            return false;
        }

        ownedSkills.Add(skill);
        return true;
    }

    public bool EquipSkill(BattleSkillSlot slot, BattleSkillDefinition skill)
    {
        if (skill == null || !ownedSkills.Contains(skill))
        {
            return false;
        }

        BattleSkillSlot? previousSlot = null;

        foreach (KeyValuePair<BattleSkillSlot, BattleSkillDefinition> pair in equippedSkills)
        {
            if (pair.Value == skill)
            {
                previousSlot = pair.Key;
                break;
            }
        }

        if (previousSlot.HasValue)
        {
            equippedSkills.Remove(previousSlot.Value);
        }

        equippedSkills[slot] = skill;
        return true;
    }

    public BattleSkillDefinition GetEquippedSkill(BattleSkillSlot slot)
    {
        return equippedSkills.TryGetValue(slot, out BattleSkillDefinition skill)
            ? skill
            : null;
    }

    public void UnequipSkill(BattleSkillSlot slot)
    {
        equippedSkills.Remove(slot);
    }

    public void Reset()
    {
        ownedSkills.Clear();
        equippedSkills.Clear();
    }
}