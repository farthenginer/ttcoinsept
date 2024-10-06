// Copyright (C) 2017 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;

namespace GameVanilla.Game.Common
{
    /// <summary>
    /// This class handles the lives system in the game. It is used to add and remove lives and other classes
    /// can subscribe to it in order to receive a notification when the number of lives changes.
    /// </summary>
    public class LivesSystem : MonoBehaviour
    {

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Make sure to check the lives when the app goes from background to foreground.
        /// </summary>



        /// <summary>
        /// Removes a life from the system.
        /// </summary>
        public async void RemoveLife()
        {
            var numLives = await GoogleAuthentication.Instance.GetLiveCountFromDB();
            if (numLives < 0)
            {
                numLives = 0;
            }
            GoogleAuthentication.Instance.SaveLiveCount(true);

        }

        /// <summary>
        /// Sets the number of lives to the maximum number allowed by the game configuration.
        /// </summary>
        public void RefillLives(bool spend)
        {
            var refillCost = PuzzleMatchManager.instance.gameConfig.livesRefillCost;
            GoogleAuthentication.Instance.fulleLiveCount();
            if (spend)
            {
                PuzzleMatchManager.instance.coinsSystem.SpendCoins(refillCost);

            }
        }

        /// <summary>
    }
        
}