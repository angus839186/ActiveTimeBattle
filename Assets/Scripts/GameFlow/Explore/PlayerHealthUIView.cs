using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUIView : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image hpFillImage;

    private void Update()
    {
        RunSession runSession = GameFlowController.Instance?.CurrentRunSession;

        if (runSession == null)
        {
            hpText.text = "HP: -";
            hpFillImage.fillAmount = 0f;
            return;
        }

        hpText.text = $"HP: {runSession.PlayerCurrentHp}/{runSession.PlayerMaxHp}";
        hpFillImage.fillAmount = runSession.PlayerHpRate;
    }
}