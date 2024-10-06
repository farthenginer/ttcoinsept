// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Game.Common;
using GameVanilla.Game.Scenes;
using GameVanilla.Game.Popups;

namespace GameVanilla.Game.UI
{
    /// <summary>
    /// This class is used to manage the bar to buy lives that is located on the level scene.
    /// </summary>
    public class BuyLivesBar : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private LevelScene levelScene;

        [SerializeField]
        private Sprite enabledLifeSprite;

        [SerializeField]
        private Sprite disabledLifeSprite;

        [SerializeField]
        private Image lifeImage;

        [SerializeField]
        private Text numLivesText;

        [SerializeField]
        private Text timeToNextLifeText;

        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private Sprite enabledButtonSprite;

        [SerializeField]
        private Sprite disabledButtonSprite;

        [SerializeField]
        private GameObject NextTimeObject;

        [SerializeField]
        private TMPro.TMP_Text NextTimeText;

        [SerializeField]
        public GameObject loadingCanvas;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(levelScene);
            Assert.IsNotNull(enabledLifeSprite);
            Assert.IsNotNull(disabledLifeSprite);
            Assert.IsNotNull(lifeImage);
            Assert.IsNotNull(numLivesText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(enabledButtonSprite);
            Assert.IsNotNull(disabledButtonSprite);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        private void Start()
        {
            numLivesText.text = UserData.Instance.LiveCount.ToString();
            loadingCanvas.SetActive(true);
            InvokeRepeating("checkLives", 1, 3);
            checkLivesFirst();
        }
        async void checkLives()
        {
            var numLives = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            numLivesText.text = numLives.ToString();
            buttonImage.sprite = numLives > maxLives ? disabledButtonSprite : enabledButtonSprite;
            loadingCanvas.SetActive(false);
        }
        async void checkLivesFirst()
        {
            var numLives = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            numLivesText.text = numLives.ToString();
            buttonImage.sprite = numLives > maxLives ? disabledButtonSprite : enabledButtonSprite;
            
        }

        /// <summary>
        /// Unity's OnDestroy method.
        /// </summary>


        /// <summary>
        /// Called when the buy button is pressed.
        /// </summary>
        public async void OnBuyButtonPressed()
        {
            var numLives = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            string nextLiveDate = await GoogleAuthentication.Instance.GetNextLiveDateForUI();
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            if (numLives == 0)
            {
                levelScene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
            }
            else
            {
                NextTimeObject.SetActive(true);
                NextTimeText.text = nextLiveDate;
                Invoke("hideNextTime", 3);
            }
        }
        void hideNextTime()
        {
            NextTimeObject.SetActive(false);
        }
        
    }
}