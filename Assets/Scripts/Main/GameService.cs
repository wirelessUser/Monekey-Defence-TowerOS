using UnityEngine;
using ServiceLocator.Utilities;
using ServiceLocator.Events;
using ServiceLocator.Map;
using ServiceLocator.Wave;
using ServiceLocator.Sound;
using ServiceLocator.Player;
using ServiceLocator.UI;

namespace ServiceLocator.Main
{
    public class GameService : MonoBehaviour
    {
        // Services:
        public EventService EventService   ;
        public MapService MapService       ;
        public WaveService WaveService     ;
        public SoundService SoundService   ;
        public PlayerService PlayerService ;
      //  public MapButton mapButton { get; private set; }

        [SerializeField] private UIService uiService;
        public UIService UIService => uiService;


        // Scriptable Objects:
        [SerializeField] private MapScriptableObject mapScriptableObject;
        [SerializeField] private WaveScriptableObject waveScriptableObject;
        [SerializeField] private SoundScriptableObject soundScriptableObject;
        [SerializeField] private PlayerScriptableObject playerScriptableObject;

        // Scene Referneces:
        [SerializeField] private AudioSource SFXSource;
        [SerializeField] private AudioSource BGSource;

        private void Start()
        {
            EventService = new EventService();
          //  UIService.SubscribeToEvents();
            MapService = new MapService(mapScriptableObject);
            WaveService = new WaveService(waveScriptableObject);
            SoundService = new SoundService(soundScriptableObject, SFXSource, BGSource);
            PlayerService = new PlayerService(playerScriptableObject);
            DependencyInjection();
         //   mapButton = new MapButton();
        }


        private void DependencyInjection()
        {

            PlayerService.Init(MapService, SoundService, UIService);
            MapService.Init(EventService);
            uiService.Init(WaveService, EventService, PlayerService);
            WaveService.Init(MapService, UIService, EventService, SoundService, WaveService, PlayerService);
           // mapButton.Init(EventService);
        }
        private void Update()
        {
            PlayerService.Update();
        }
    }
}