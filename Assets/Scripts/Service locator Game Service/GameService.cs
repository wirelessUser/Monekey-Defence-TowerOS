using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServiceLocator.Player;

namespace ServiceLocator.Main 

{
    public class GameService : GenericSingleton<GameService>
    {
        public PlayerService playerService { get; private set; }

        public PlayerScriptableObject playerScriptableObj;
        void Start()
        {
            playerService = new PlayerService(playerScriptableObj);
        }

        private void Update()
        {
            playerService.Update();
        }


    }


}

