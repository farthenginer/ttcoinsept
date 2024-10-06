// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using GameVanilla.Core;
using GameVanilla.Game.Common;

namespace GameVanilla.Game.Popups
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying lives.
    /// </summary>
    public class BuyLivesPopup : Popup
    {
#pragma warning disable 649
        [SerializeField]
        private Sprite lifeSprite;

        [SerializeField]
        private List<Image> lifeImages;

        [SerializeField]
        private Text refillCostText;

        [SerializeField]
        private Text timeToNextLifeText;

        [SerializeField]
        private ParticleSystem lifeParticles;

        [SerializeField]
        private AnimatedButton refillButton;

        [SerializeField]
        private Image refillButtonImage;

        [SerializeField]
        private Sprite refillButtonDisabledSprite;

        [SerializeField]
        private TMPro.TMP_Text remainVideo,nextlText;
        public Button videoWatch;
#pragma warning restore 649

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(lifeSprite);
            Assert.IsNotNull(refillCostText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(lifeParticles);
            Assert.IsNotNull(refillButton);
            Assert.IsNotNull(refillButtonImage);
            Assert.IsNotNull(refillButtonDisabledSprite);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            checkOnStart();
            InvokeRepeating("checkLiveSprites", 1, 2);
            checkVideoText();
            checkLiveSprites();
        }
        async System.Threading.Tasks.Task<bool> checkVideo()
        {
            bool videoPermission = await GoogleAuthentication.Instance.CheckVideoBoolJust();
            return videoPermission;
        }
         void checkVideoText()
        {

            bool status = UserData.Instance.videoBool;

            if (status)
            {
                remainVideo.text = 1.ToString();
            }
            else
            {
                remainVideo.text = 0.ToString();

            }
            InvokeRepeating("checkVideoTRY", 1,1);
        }
        async void checkVideoTRY()
        {

            bool status = await GoogleAuthentication.Instance.CheckVideoBoolJust();

            if (status)
            {
                remainVideo.text = 1.ToString();
            }
            else
            {
                remainVideo.text = 0.ToString();

            }
        }
        async void checkLiveSprites()
        {
            int numLife = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            string getLiveDate = await GoogleAuthentication.Instance.GetNextLiveDateForUI();
            nextlText.text = $"Next Time: {getLiveDate}";
            UpdateLifeSprites(numLife);
        }
        async void checkOnStart()
        {
            var maxLives = PuzzleMatchManager.instance.gameConfig.maxLives;
            var numLives = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            if (numLives >= maxLives)
            {
                DisableRefillButton();
            }
            UpdateLifeSprites(numLives);
            refillCostText.text = PuzzleMatchManager.instance.gameConfig.livesRefillCost.ToString();
        }
       
        public async void OnRefillButtonPressed()
        {
            await GoogleAuthentication.Instance.FetchTTCoinFromFirestore();
            var numCoins = UserData.Instance.TTCoin;
            if (numCoins >= PuzzleMatchManager.instance.gameConfig.livesRefillCost)
            {
                PuzzleMatchManager.instance.livesSystem.RefillLives(true);
                lifeParticles.Play();
                SoundManager.instance.PlaySound("BuyPopButton");
                DisableRefillButton();
                OnCloseButtonPressed();
            }
            else
            {
                var scene = parentScene;
                if (scene != null)
                {
                    scene.CloseCurrentPopup();
                    SoundManager.instance.PlaySound("Button");
                }
            }
        }
        public void refillWithThridyParty()
        {
            PuzzleMatchManager.instance.livesSystem.RefillLives(false);
            lifeParticles.Play();
            SoundManager.instance.PlaySound("BuyPopButton");
            remainVideo.text = 0.ToString();
        }
        public  void WatchAndRefill()
        {
            bool permission = UserData.Instance.videoBool;
            if (permission)
            {
                var applovindAds = FindObjectOfType<ApplovinAds>().GetComponent<ApplovinAds>();
                applovindAds.ShowVideo();
            }
            checkVideoText();
            OnCloseButtonPressed();
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        public void OnCloseButtonPressed()
        {
            Close();
        }

        /// <summary>
        /// Called when the lives countdown is updated.
        /// </summary>
        /// <param name="timeSpan">The time left for a free life.</param>
        /// <param name="lives">The current number of lives.</param>
        

        /// <summary>
        /// Updates the life sprites.
        /// </summary>
        /// <param name="lives">The current number of lives.</param>
        private void UpdateLifeSprites(int lives)
        {
            if (lives > 5)
            {
                lives = 5;
            }
            for (var i = 0; i < lives; i++)
            {
                lifeImages[i].sprite = lifeSprite;
            }

            if (lives == PuzzleMatchManager.instance.gameConfig.maxLives)
            {
                DisableRefillButton();
            }
        }

        /// <summary>
        /// Disables the refill button.
        /// </summary>
        private void DisableRefillButton()
        {
            refillButtonImage.sprite = refillButtonDisabledSprite;
            refillButton.interactable = false;
        }

       
    }
   
}