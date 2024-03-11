
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

namespace GGJ.Levels
{

    public class LevelLoadedEventArgs : EventArgs
    {

        public LevelLoadedEventArgs(Level level)
        {
            Level = level;
        }

        public Level Level { get; private set; }

    }

    public class LevelManager : MonoBehaviour
    {
        [SerializeField]
        private Image transitionScreen;

        public List<string> levelScenes;

        public Level CurrentLevel { get; private set; }

        public event EventHandler<LevelLoadedEventArgs> OnLevelLoaded;

        [SerializeField]
        private GameObject startScreen;

        public async void LoadFirstLevel()
        {
            await transitionScreen.DOFade(1f, 0.3f).AsyncWaitForCompletion();

            SceneManager.LoadScene(levelScenes[0]);
        }
        public async void LoadNextLevel()
        {
            int actualLevelIndex = levelScenes.IndexOf(levelScenes.FirstOrDefault(s => s == CurrentLevel.LevelName));
            if ((actualLevelIndex+1) == levelScenes.Count())
            {
                FinalLevelCompleted();
                return;
            }

            await transitionScreen.DOFade(1f, 0.3f).AsyncWaitForCompletion();

            Debug.Log(levelScenes[actualLevelIndex++]);

            SceneManager.LoadScene(levelScenes[actualLevelIndex++]);
        }

        public async void FinalLevelCompleted()
        {
            await transitionScreen.DOFade(1f, 0.3f).AsyncWaitForCompletion();

            SceneManager.UnloadSceneAsync(CurrentLevel.LevelName);

            startScreen.SetActive(true);

            transitionScreen.DOFade(0f, 0.3f);
        }

        public void LevelLoaded(Level level)
        {
            CurrentLevel = level;
            OnLevelLoaded?.Invoke(this, new LevelLoadedEventArgs(level));
            transitionScreen.DOFade(0f, 0.3f);
        }

    }
}
