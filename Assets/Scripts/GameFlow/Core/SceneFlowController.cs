using UnityEngine.SceneManagement;

public class SceneFlowController
{
    public void LoadLobby()
    {
        LoadSceneIfNeeded("Main");
    }

    public void LoadExplore()
    {
        LoadSceneIfNeeded("Explore");
    }

    public void LoadBattle()
    {
        LoadSceneIfNeeded("Battle");
    }

    private void LoadSceneIfNeeded(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}