
public class GameFlowContext
{
    public InputModeController InputMode { get; }
    public CameraModeController CameraMode { get; }
    public PlayerControlModeController PlayerControlMode { get; }
    public SceneFlowController SceneFlow { get; }
    public RunSession RunSession { get; }

    public GameFlowContext(
        InputModeController inputMode,
        CameraModeController cameraMode,
        PlayerControlModeController playerControlMode,
        SceneFlowController sceneFlow,
        RunSession runSession)
    {
        InputMode = inputMode;
        CameraMode = cameraMode;
        PlayerControlMode = playerControlMode;
        SceneFlow = sceneFlow;
        RunSession = runSession;
    }
}