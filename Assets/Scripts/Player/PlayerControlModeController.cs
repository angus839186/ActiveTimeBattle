using UnityEngine;
public class PlayerControlModeController : MonoBehaviour
{
    public void SetLobby()
    {
        Debug.Log("Set Player mode: Lobby");
    }

    public void SetExplore()
    {
        Debug.Log("Set Player mode: Explore");
    }

    public void SetBattle()
    {
        Debug.Log("Set Player mode: Battle");
    }
}