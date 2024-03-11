using GGJ.Core;
using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GGJ.Levels
{

    public enum LevelState
    {
        Loading,
        Started,
        Finished
    }

    public class Level : MonoBehaviour, ILevel
    {
        public Character Character { get; private set; }
        public string LevelName { get; private set; }
        public LevelState CurrentState { get; private set; }

        private void Awake()
        {
            LevelName = SceneManager.GetActiveScene().name;

            if (GGJ.Core.GameManager.Instance == null)
            {
                SceneManager.LoadScene("Core", LoadSceneMode.Additive);
            }
        }

        private IEnumerator Start()
        {
            StartLevel();

            yield return null;

            Character = FindObjectOfType<Character>();

            GGJ.Core.GameManager.Instance.LevelManager.LevelLoaded(this);
        }

        public void StartLevel()
        {
            ChangeState(LevelState.Started);
        }

        public void FinishLevel()
        {
            if (CurrentState == LevelState.Finished)
            {
                return;
            }

            ChangeState(LevelState.Finished);
            GGJ.Core.GameManager.Instance.LevelManager.LoadNextLevel();
        }

        private void ChangeState(LevelState state)
        {
            CurrentState = state;
        }
    }

}