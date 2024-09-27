using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServiceLocator.Player;
using ServiceLocator.Main;

namespace ServiceLocator.UI
{
    public class MonkeyCellController
    {
        private MonkeyCellView monkeyCellView;
        private MonkeyCellScriptableObject monkeyCellSO;
        private PlayerService playerService;
        public MonkeyCellController(Transform cellContainer, MonkeyCellView monkeyCellPrefab, MonkeyCellScriptableObject monkeyCellScriptableObject, PlayerService playerService)
        {
            this.playerService = playerService;
            this.monkeyCellSO = monkeyCellScriptableObject;
            monkeyCellView = Object.Instantiate(monkeyCellPrefab, cellContainer);
            monkeyCellView.SetController(this);
           
            monkeyCellView.ConfigureCellUI(monkeyCellSO.Sprite, monkeyCellSO.Name, monkeyCellSO.Cost);
        }

        public void MonkeyDraggedAt(Vector3 dragPosition)
        {
          // GameService.Instance.PlayerService.ValidateSpawnPosition(monkeyCellSO.Cost, dragPosition);
           playerService.ValidateSpawnPosition(monkeyCellSO.Cost, dragPosition);
        }

        public void MonkeyDroppedAt(Vector3 dropPosition)
        {
            playerService.TrySpawningMonkey(monkeyCellSO.Type, monkeyCellSO.Cost, dropPosition);
        }
    }
}