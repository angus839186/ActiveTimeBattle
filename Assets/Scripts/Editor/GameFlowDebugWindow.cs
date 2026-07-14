#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class GameFlowDebugWindow : EditorWindow
{
    private PlayerClassDefinition testClass;
    [MenuItem("ActiveTimeBattle/Game Flow Debug")]
    private static void Open()
    {
        GetWindow<GameFlowDebugWindow>("Game Flow Debug");
    }

    private void OnGUI()
    {
        GameFlowController controller = FindFirstObjectByType<GameFlowController>();

        if (controller != null)
        {
            EditorGUILayout.LabelField("Current State", controller.CurrentStateName);
        }

        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Lobby"))
        {
            ChangeState(controller => controller.ChangeToLobby());
        }

        if (GUILayout.Button("Explore"))
        {
            ChangeState(controller => controller.ChangeToExplore());
        }

        if (GUILayout.Button("Battle"))
        {
            ChangeState(controller => controller.ChangeToBattle());
        }

        if (GUILayout.Button("Start Test Run"))
        {
            ChangeState(controller => controller.StartNewRun("TestClass"));
        }
        testClass = (PlayerClassDefinition)EditorGUILayout.ObjectField(
                    "Test Class", testClass, typeof(PlayerClassDefinition), false);

        if (GUILayout.Button("Start Test Run With Class"))
        {
            ChangeState(controller => controller.StartNewRun(testClass));
        }

        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to switch states.", MessageType.Info);
        }
    }

    private static void ChangeState(System.Action<GameFlowController> changeState)
    {
        GameFlowController controller = FindFirstObjectByType<GameFlowController>();

        if (controller == null)
        {
            Debug.LogWarning("GameFlowController not found in the active scene.");
            return;
        }

        changeState.Invoke(controller);
    }
    [MenuItem("ActiveTimeBattle/Return To Main")]
    private static void ReturnToMain()
    {
        if (Application.isPlaying)
        {
            GameFlowController controller = FindFirstObjectByType<GameFlowController>();

            if (controller == null)
            {
                Debug.LogWarning("GameFlowController not found in the active scene.");
                return;
            }

            controller.ChangeToLobby();
            return;
        }

        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
    }
}
#endif