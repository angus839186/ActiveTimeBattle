using ActiveTimeBattle.Presentation.Core;
using ActiveTimeBattle.Presentation.Hub;
using ActiveTimeBattle.Presentation.Narrative;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveTimeBattle.Editor
{
    public static class PrototypeDebugMenu
    {
        [MenuItem("ActiveTimeBattle/Debug/Start New Run")]
        public static void StartNewRun()
        {
            if (!TryGetBootstrapper(out GameBootstrapper bootstrapper))
            {
                return;
            }

            bootstrapper.RunFlowController
                .StartNewRunAsync(bootstrapper.destroyCancellationToken)
                .Forget();
        }

        [MenuItem("ActiveTimeBattle/Debug/Return To Hub")]
        public static void ReturnToHub()
        {
            if (!TryGetBootstrapper(out GameBootstrapper bootstrapper))
            {
                return;
            }

            bootstrapper.RunFlowController
                .EnterHubAsync(bootstrapper.destroyCancellationToken)
                .Forget();
        }

        [MenuItem("ActiveTimeBattle/Debug/Enter Combat")]
        public static void EnterCombat()
        {
            if (!TryGetBootstrapper(out GameBootstrapper bootstrapper))
            {
                return;
            }

            bootstrapper.RunFlowController
                .EnterCombatAsync(bootstrapper.destroyCancellationToken)
                .Forget();
        }

        [MenuItem("ActiveTimeBattle/Debug/Return To Exploration")]
        public static void ReturnToExploration()
        {
            if (!TryGetBootstrapper(out GameBootstrapper bootstrapper))
            {
                return;
            }

            bootstrapper.RunFlowController
                .ReturnToExplorationAsync(bootstrapper.destroyCancellationToken)
                .Forget();
        }

        [MenuItem("ActiveTimeBattle/Debug/Play Hub Keeper Dialogue")]
        public static void PlayHubKeeperDialogue()
        {
            if (!EditorApplication.isPlaying)
            {
                UnityEngine.Debug.LogWarning(
                    "Dialogue debug command requires Play Mode.");
                return;
            }

            DialogueRunner runner =
                Object.FindFirstObjectByType<DialogueRunner>();
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(
                "Assets/_Project/Data/Narrative/HubKeeper.asset");
            if (runner == null || dialogue == null)
            {
                UnityEngine.Debug.LogWarning(
                    "Hub dialogue runner or data was not found.");
                return;
            }

            runner.PlayAsync(dialogue, runner.destroyCancellationToken).Forget();
        }

        [MenuItem("ActiveTimeBattle/Debug/Verify Hub Entrance Input")]
        public static void VerifyHubEntranceInput()
        {
            if (!TryGetBootstrapper(out _))
            {
                return;
            }

            GameObject player = GameObject.Find("First Person Player");
            if (player == null || Keyboard.current == null)
            {
                UnityEngine.Debug.LogWarning(
                    "Hub player or keyboard was not found.");
                return;
            }

            player.transform.SetPositionAndRotation(
                new Vector3(0f, 0.05f, 3f),
                Quaternion.identity);
            Physics.SyncTransforms();

            InputAction interact =
                InputSystem.actions.FindAction("Hub/Interact", true);
            UnityEngine.Debug.Log(
                $"Hub/Interact enabled: "
                + $"{interact.enabled}.");
            HubInteractionDetector detector =
                player.GetComponent<HubInteractionDetector>();
            UnityEngine.Debug.Log(
                $"Hub entrance interaction started: "
                + $"{detector != null && detector.TryInteract()}.");
        }

        private static bool TryGetBootstrapper(
            out GameBootstrapper bootstrapper)
        {
            bootstrapper = GameBootstrapper.Instance;
            if (EditorApplication.isPlaying && bootstrapper != null)
            {
                return true;
            }

            UnityEngine.Debug.LogWarning(
                "Debug flow commands require Play Mode and GameBootstrapper.");
            return false;
        }
    }
}
