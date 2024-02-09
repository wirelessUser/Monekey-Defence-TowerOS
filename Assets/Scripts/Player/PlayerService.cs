using System.Collections.Generic;
using UnityEngine;
using ServiceLocator.Player.Projectile;
using ServiceLocator.Map;
using ServiceLocator.UI;
using ServiceLocator.Sound;

namespace ServiceLocator.Player
{
    public class PlayerService : GenericMonoBehaviourSingleton<PlayerService>
    {
        // Dependencies:
        
       
       [SerializeField] private PlayerScriptableObject playerScriptableObject;
        private ProjectilePool projectilePool;

        private List<MonkeyController> activeMonkeys;
        private MonkeyView selectedMonkeyView;
        private int health;
        public int Money { get; private set; }

      


        public void InitGameService(PlayerScriptableObject playerScriptableObject)
        {
            this.playerScriptableObject = playerScriptableObject;
            projectilePool = new ProjectilePool(playerScriptableObject.ProjectilePrefab, playerScriptableObject.ProjectileScriptableObjects);
        }

        public void Init( )
        {

           
            InitializeVariables();
        }

        private void InitializeVariables()
        {
            activeMonkeys = new List<MonkeyController>();
            health = playerScriptableObject.Health;
            Money = playerScriptableObject.Money;
            UIService.Instance.UpdateHealthUI(health);
            UIService.Instance.UpdateMoneyUI(Money);
        }

        public void Update()
        {
            if (activeMonkeys.Count != 0)
            {
                foreach (MonkeyController controller in activeMonkeys)
                {
                    controller.UpdateMonkey();
                }
            }
            foreach(MonkeyController monkey in activeMonkeys)
            {
                monkey?.UpdateMonkey();
            }

            if(Input.GetMouseButtonDown(0))
            {
                TrySelectingMonkey();
            }
        }

        private void TrySelectingMonkey()
        {
            RaycastHit2D[] hits = GetRaycastHitsAtMousePoition();

            foreach (RaycastHit2D hit in hits)
            {
                if(IsMonkeyCollider(hit.collider))
                {
                    SetSelectedMonkeyView(hit.collider.GetComponent<MonkeyView>());
                    return;
                }
            }

            selectedMonkeyView?.MakeRangeVisible(false);
        }

        private RaycastHit2D[] GetRaycastHitsAtMousePoition()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return Physics2D.RaycastAll(mousePosition, Vector2.zero);
        }

        private bool IsMonkeyCollider(Collider2D collider) => collider != null && !collider.isTrigger && collider.GetComponent<MonkeyView>() != null;

        private void SetSelectedMonkeyView(MonkeyView monkeyViewToBeSelected)
        {
            selectedMonkeyView?.MakeRangeVisible(false);
            selectedMonkeyView = monkeyViewToBeSelected;
            selectedMonkeyView.MakeRangeVisible(true);
        }

        public void ValidateSpawnPosition(int monkeyCost, Vector3 dropPosition)
        {
            if (monkeyCost > Money)
                return;

            MapService.Instance.ValidateSpawnPosition(dropPosition);
        }

        public void TrySpawningMonkey(MonkeyType monkeyType, int monkeyCost, Vector3 dropPosition)
        {
            if (monkeyCost > Money)
                return;

            if (MapService.Instance.TryGetMonkeySpawnPosition(dropPosition, out Vector3 spawnPosition))
            {
                SpawnMonkey(monkeyType, spawnPosition);
                SoundService.Instance.PlaySoundEffects(SoundType.SpawnMonkey);
            }
        }

        public void SpawnMonkey(MonkeyType monkeyType, Vector3 spawnPosition)
        {
            MonkeyScriptableObject monkeyScriptableObject = GetMonkeyScriptableObjectByType(monkeyType);
            MonkeyController monkey = new MonkeyController( monkeyScriptableObject, projectilePool);
            
            monkey.SetPosition(spawnPosition);
            activeMonkeys.Add(monkey);
            DeductMoney(monkeyScriptableObject.Cost);
        }

        private MonkeyScriptableObject GetMonkeyScriptableObjectByType(MonkeyType monkeyType) => playerScriptableObject.MonkeyScriptableObjects.Find(so => so.Type == monkeyType);

        public void ReturnProjectileToPool(ProjectileController projectileToReturn) => projectilePool.ReturnItem(projectileToReturn);
        
        public void TakeDamage(int damageToTake)
        {
            int reducedHealth = health - damageToTake;
            health = reducedHealth <= 0 ? 0 : health - damageToTake;

            UIService.Instance.UpdateHealthUI(health);
            if(health <= 0)
                PlayerDeath();
        }

        private void DeductMoney(int moneyToDedecut)
        {
            Money -= moneyToDedecut;
            UIService.Instance.UpdateMoneyUI(Money);
        }

        public void GetReward(int reward)
        {
            Money += reward;
            UIService.Instance?.UpdateMoneyUI(Money);
        }

        private void PlayerDeath() => UIService.Instance.UpdateGameEndUI(false);
    }
}