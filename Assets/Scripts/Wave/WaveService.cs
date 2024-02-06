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
    public class WaveService :MonoBehaviour
    {
        // Dependencies:
       
        private EventService eventService;

        private WaveScriptableObject waveScriptableObject;
        private BloonPool bloonPool;

        private int currentWaveId;
        private List<WaveData> waveDatas;
        private List<BloonController> activeBloons;

        private static WaveService instance;
        public static WaveService Instance { get { return instance; } }

        private void Awake()
        {
            MakeInstance();
        }
        private void MakeInstance()
        {
            if (instance == null) instance = this;
            else { Destroy(gameObject); }
        }
        public void  InitWaveFromGameService(WaveScriptableObject waveScriptableObject) => this.waveScriptableObject = waveScriptableObject;

        public void Init( EventService eventService)
        {
            
            this.eventService = eventService;
            InitializeBloons();
            SubscribeToEvents();
        }

        private void InitializeBloons()
        {
            bloonPool = new BloonPool( waveScriptableObject);
            activeBloons = new List<BloonController>();
        }

        private void SubscribeToEvents() => eventService.OnMapSelected.AddListener(LoadWaveDataForMap);

        private void LoadWaveDataForMap(int mapId)
        {
            currentWaveId = 0;
            waveDatas = waveScriptableObject.WaveConfigurations.Find(config => config.MapID == mapId).WaveDatas;
            UIService.Instance.UpdateWaveProgressUI(currentWaveId, waveDatas.Count);
        }

        public void StarNextWave()
        {
            currentWaveId++;
            var bloonsToSpawn = GetBloonsForCurrentWave();
            var spawnPosition = MapService.Instance.GetBloonSpawnPositionForCurrentMap();
            SpawnBloons(bloonsToSpawn, spawnPosition, 0, waveScriptableObject.SpawnRate);
        }

        public async void SpawnBloons(List<BloonType> bloonsToSpawn, Vector3 spawnPosition, int startingWaypointIndex, float spawnRate)
        {
            foreach(BloonType bloonType in bloonsToSpawn)
            {
                BloonController bloon = bloonPool.GetBloon(bloonType);
                bloon.SetPosition(spawnPosition);
                bloon.SetWayPoints(MapService.Instance.GetWayPointsForCurrentMap(), startingWaypointIndex);

                AddBloon(bloon);
                await Task.Delay(Mathf.RoundToInt(spawnRate * 1000));
            }
        }

        private void AddBloon(BloonController bloonToAdd)
        {
            activeBloons.Add(bloonToAdd);
            bloonToAdd.SetOrderInLayer(-activeBloons.Count);
        }

        public void RemoveBloon(BloonController bloon)
        {
            bloonPool.ReturnItem(bloon);
            activeBloons.Remove(bloon);
            if (HasCurrentWaveEnded())
            {
                SoundService.Instance.PlaySoundEffects(Sound.SoundType.WaveComplete);
                UIService.Instance.UpdateWaveProgressUI(currentWaveId, waveDatas.Count);

                if(IsLevelWon())
                    UIService.Instance.UpdateGameEndUI(true);
                else
                    UIService.Instance.SetNextWaveButton(true);
            }
        }

        private List<BloonType> GetBloonsForCurrentWave() => waveDatas.Find(waveData => waveData.WaveID == currentWaveId).ListOfBloons;

        private bool HasCurrentWaveEnded() => activeBloons.Count == 0;

        private bool IsLevelWon() => currentWaveId >= waveDatas.Count;
    }
}