using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerClassDefinition",
    menuName = "ActiveTimeBattle/Player Class Definition")]
public class PlayerClassDefinition : ScriptableObject
{
    [SerializeField] private string classId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;

    public string ClassId => classId;
    public string DisplayName => displayName;
    public string Description => description;
}