using CustomInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [Space, Header("Spawn Prefs")]
    [SerializeField] private float gameStartDelay;
    [SerializeField] private List<LineRenderer> movingLines = new List<LineRenderer>(3);
    [SerializeField] private List<Transform> hangingSpawnPositions = new List<Transform>(6);

    private GameMode currentGM;
    private AimController aimController;
    private ObjectPool op;

    private void Awake()
    {
        op ??= FindFirstObjectByType<ObjectPool>();

        StaticData.movingLines = movingLines;
        StaticData.hangingSpawnPositions = hangingSpawnPositions;
    }

    public void SetGameMode(GameMode gameMode, AimController aimController)
    {
        currentGM = gameMode;
        this.aimController = aimController;
    }

    public void RunGame()
    {
        aimController.SetAimPrefs(currentGM.aimPrefs);

        SpawnPrefs sp = currentGM.spawnPrefs;

        StartCoroutine(RunActs(sp));

        IEnumerator GameCoroutine()
        {
            yield return new WaitForSeconds(gameStartDelay);

            for (int i = 0;i < sp.spawnActs.Count;i++)
            {
                SpawnAct currentAct = sp.spawnActs[i];

                yield return new WaitForSeconds(currentAct.actStartDelay);

                for (int j = 0; j < currentAct.steps.Count;j++)
                {
                    SpawnStep currentStep = currentAct.steps[j];

                    yield return new WaitForSeconds(currentStep.stepStartDelay);

                    for (int k = 0; k < currentStep.spawnCount;k++)
                    {
                        Target target = op.GetObject().GetComponent<Target>();

                        target.SetTargetPrefs(currentStep, op);

                        float dly = k < currentStep.spawnCount - 1 ? currentStep.spawnDelay : Time.deltaTime;

                        yield return new WaitForSeconds(dly);
                    }
                }

                bool actEnded = false;

                while (!actEnded)
                {
                    actEnded = op.PoolsCount(true) == 0;
                    yield return null;
                }
            }

            if (currentGM.GameModeType == GMType.Infinite)
            {
                StartCoroutine(GameCoroutine());
            }
        }
    }

    #region Game Runners

    private IEnumerator RunActs(SpawnPrefs sp)
    {
        yield return new WaitForSeconds(gameStartDelay);

        foreach (var act in sp.spawnActs)
        {
            yield return RunAct(act);

            // ждЄм, пока все цели этого акта исчезнут
            yield return new WaitUntil(() => op.PoolsCount(true) == 0);
        }

        if (currentGM.GameModeType == GMType.Infinite)
        {
            StartCoroutine(RunActs(sp));
        }
    }

    private IEnumerator RunAct(SpawnAct act)
    {
        yield return new WaitForSeconds(act.actStartDelay);

        foreach (var step in act.steps)
            yield return RunStep(step);
    }

    private IEnumerator RunStep(SpawnStep step)
    {
        yield return new WaitForSeconds(step.stepStartDelay);

        for (int k = 0; k < step.spawnCount; k++)
        {
            Target target = op.GetObject().GetComponent<Target>();
            target.SetTargetPrefs(step, op);

            // если не последний спавн Ч ждЄм задержку, иначе 1 кадр
            float delay = k < step.spawnCount - 1 ? step.spawnDelay : Time.deltaTime;
            yield return new WaitForSeconds(delay);
        }
    }

    #endregion
}
