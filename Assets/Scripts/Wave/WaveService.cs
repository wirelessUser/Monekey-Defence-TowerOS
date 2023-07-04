using System.Collections.Generic;
using UnityEngine;
using ServiceLocator.Wave.Bloon;
using System.Threading.Tasks;
using ServiceLocator.UI;
using ServiceLocator.Map;
using ServiceLocator.Sound;
using ServiceLocator.Player;
using ServiceLocator.Events;

namespace ServiceLocator.Wave
{
    public class WaveService
    {
        // Dependencies:
        private UIService uiService;
        private MapService mapService;
        private PlayerService playerService;
        private SoundService soundService;
        private EventService eventService;

        private WaveScriptableObject waveScriptableObject;
        private BloonPool bloonPool;

        private int currentWaveId;
        private List<WaveData> waveDatas;
        private List<BloonController> activeBloons;

        public WaveService(WaveScriptableObject waveScriptableObject) => this.waveScriptableObject = waveScriptableObject;

        public void Init(UIService uiService, MapService mapService, PlayerService playerService, SoundService soundService, EventService eventService, Transform bloonContainer)
        {
            this.uiService = uiService;
            this.mapService = mapService;
            this.playerService = playerService;
            this.soundService = soundService;
            this.eventService = eventService;
            InitializeBloons(bloonContainer);
            SubscribeToEvents();
        }

        private void InitializeBloons(Transform bloonContainer)
        {
            // todo
            bloonPool = new BloonPool(playerService, 
                                        this, 
                                        soundService, 
                                        waveScriptableObject.BloonTypeDataMap.BloonPrefab, 
                                        waveScriptableObject.BloonTypeDataMap.BloonScriptableObjects, 
                                        bloonContainer);

            //bloonPool = new BloonPool(this, playerService, soundService, waveScriptableObject, bloonContainer);

            activeBloons = new List<BloonController>();
        }

        // todo
        private void SubscribeToEvents() => eventService.OnMapSelected.AddListener(LoadWaveDataForMap);


        private void LoadWaveDataForMap(int mapId)
        {
            currentWaveId = 0;
            waveDatas = waveScriptableObject.WaveConfigurations.Find(config => config.MapID == mapId).WaveDatas;
            uiService.UpdateWaveProgressUI(currentWaveId, waveDatas.Count);
        }

        public void StarNextWave()
        {
            currentWaveId++;
            var bloonsToSpawn = waveDatas.Find(waveData => waveData.WaveID == currentWaveId).ListOfBloons;
            var spawnPosition = mapService.GetBloonSpawnPositionForCurrentMap();
            SpawnBloons(bloonsToSpawn, spawnPosition, 0);
        }

        public async void SpawnBloons(List<BloonType> bloonsToSpawn, Vector3 spawnPosition, int startingWaypointIndex)
        {
            foreach(BloonType bloonType in bloonsToSpawn)
            {
                BloonController bloon = bloonPool.GetBloon(bloonType);
                bloon.SetPosition(spawnPosition);
                bloon.SetWayPoints(mapService.GetWayPointsForCurrentMap(), startingWaypointIndex);
                
                activeBloons.Add(bloon);
                await Task.Delay(Mathf.RoundToInt(waveScriptableObject.SpawnRate * 1000));
            }
        }

        public void RemoveBloon(BloonController bloon)
        {
            bloonPool.ReturnItem(bloon);
            activeBloons.Remove(bloon);
            if (HasCurrentWaveEnded())
            {
                soundService.PlaySoundEffects(Sound.SoundType.WaveComplete);
                uiService.UpdateWaveProgressUI(currentWaveId, waveDatas.Count);

                if(IsLevelWon())
                    uiService.UpdateGameEndUI(true);
                else
                    uiService.SetNextWaveButton(true);
            }
        }

        private bool HasCurrentWaveEnded() => activeBloons.Count == 0;

        private bool IsLevelWon() => currentWaveId >= waveDatas.Count;
    }
}