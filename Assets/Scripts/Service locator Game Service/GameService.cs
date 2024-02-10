using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServiceLocator.Player;
using ServiceLocator.Map;
using ServiceLocator.Sound;
using ServiceLocator.Wave;
namespace ServiceLocator.Main 

{
    public class GameService : GenericSingleton<GameService>
    {
        public PlayerService playerService { get; private set; }
        public MapService mapService { get; private set; }
        public WaveService waveService { get; private set; }
        public SoundService soundService { get; private set; }

        [SerializeField]  private PlayerScriptableObject playerScriptableObj;
        [SerializeField]  private MapScriptableObject mapScriptableObj;
        [SerializeField]  private SoundScriptableObject soundScriptableObj;
        [SerializeField]  private WaveScriptableObject waveScriptableObj;


        [SerializeField] private AudioSource audioEffects;
        [SerializeField] private AudioSource backgroundMusic;



        void Start()
        {
            playerService = new PlayerService(playerScriptableObj);
            soundService = new SoundService(soundScriptableObj,audioEffects,backgroundMusic);

            mapService = new MapService(mapScriptableObj);
            waveService = new WaveService(waveScriptableObj);
        }

        private void Update()
        {
            playerService.Update();
        }


    }


}

