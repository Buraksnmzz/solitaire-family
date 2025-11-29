using Core.Scripts.Helper.UI.Score;
using Core.Scripts.Services;
using UnityEngine;

public class TutorialGamePresenter : BasePresenter<TutorialGameView>
{
    private readonly IUIService _uiService;
    private readonly ISoundService _soundService;

    public TutorialGamePresenter()
    {
        _uiService = ServiceLocator.GetService<IUIService>();
        _soundService = ServiceLocator.GetService<ISoundService>();
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        View.OnTutorialCompleted += HandleTutorialCompleted;
        View.ValueMoved += HandleValueMoved;
        View.TargetClicked += HandleTargetClicked;
    }

    private void HandleTargetClicked()
    {
        _soundService.PlaySound(ClipName.ValueSelect);
    }

    private void HandleValueMoved()
    {
        _soundService.PlaySound(ClipName.CorrectEquation);
    }

    private void HandleTutorialCompleted()
    {
        _soundService.PlaySound(ClipName.GameEnd);
        PlayerPrefs.SetInt(StringConstants.IsTutorialShown, 1);
        PlayerPrefs.SetInt(StringConstants.IsTutorialSession, 1);
        PlayerPrefs.Save();

        View.Hide();
        _uiService.ShowPopup<ScorePresenter>();
    }

    public override void Cleanup()
    {
        base.Cleanup();
        View.OnTutorialCompleted -= HandleTutorialCompleted;
    }
}
