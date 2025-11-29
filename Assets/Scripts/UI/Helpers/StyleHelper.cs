using UnityEngine;

[CreateAssetMenu(fileName = "StyleHelper", menuName = "CrossMathPuzzle/Style Helper")]
public class StyleHelper : ScriptableObject
{
    [SerializeField] public Color gameBlue;
    [SerializeField] public Color gameGreyDark;
    [SerializeField] public Color puzzleBoxEmpty;
    [SerializeField] public Color puzzleBoxFilled;
    [SerializeField] public Color puzzleBoxWrong;
    [SerializeField] public Color puzzleNumber;
    [SerializeField] public Color puzzleNumberWrong;
    public Color correctEquationBackground;
    public Color correctEquationOutline;
    public Color incorrectEquationBackground;
    public Color incorrectEquationOutline;
    public Color normalOutline;
    public Color gameGrey;


    public Color GameBlue => gameBlue;
    public Color GameGreyDark => gameGreyDark;
}
