using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int score;
    [Header("References"), Space]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Animator textAnimator;

    private void Start()
    {
        ChangeText();
    }

    public void AddScore(int s)
    {
        score += s;

        if (s > 0)
        {
            textAnimator.SetTrigger("Plus");
        }
        else if (s < 0)
        {
            textAnimator.SetTrigger("Minus");
        }

        ChangeText();
    }

    public void ChangeText()
    {
        scoreText.text = $"{score}";
    }
}
