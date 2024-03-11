using MoreMountains.CorgiEngine;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.UI.InGame
{
    public class HealthUI : MonoBehaviour
    {

        [SerializeField]
        private HeartUI heartPrefab;

        private readonly List<HeartUI> hearts = new List<HeartUI>();

        private Health characterHealth;

        public void Start()
        {
            var currentLevel = GGJ.Core.GameManager.Instance.LevelManager.CurrentLevel;
            if (currentLevel != null)
            {
                CreateHearts();
            }

            GGJ.Core.GameManager.Instance.LevelManager.OnLevelLoaded += LevelManager_OnLevelLoaded;
        }

        private void LevelManager_OnLevelLoaded(object sender, Levels.LevelLoadedEventArgs e)
        {
            CreateHearts();
        }

        private void CreateHearts()
        {
            var character = GGJ.Core.GameManager.Instance.LevelManager.CurrentLevel.Character;
            characterHealth = character.GetComponent<Health>();

            foreach (var heartUi in hearts)
            {
                Destroy(heartUi.gameObject);
            }

            hearts.Clear();

            int maximumHealth = Mathf.CeilToInt(characterHealth.MaximumHealth);
            for (int i = 0; i < maximumHealth; i++)
            {
                CreateHeart();
            }

            UpdateHearts();
        }

        private void CreateHeart()
        {
            HeartUI heart = Instantiate(heartPrefab);
            heart.transform.SetParent(transform);
            heart.transform.localScale = Vector3.one;
            hearts.Add(heart);
        }

        private void Update()
        {
            UpdateHearts();
        }

        private void UpdateHearts()
        {
            if (characterHealth == null)
            {
                return;
            }

            int currentHealth = Mathf.RoundToInt(characterHealth.CurrentHealth);
            for (int i = 0; i < hearts.Count; i++)
            {
                if (i < currentHealth) {
                    hearts[i].SetFull();
                } else
                {
                    hearts[i].SetEmpty();
                }
            }
        }
    }

}