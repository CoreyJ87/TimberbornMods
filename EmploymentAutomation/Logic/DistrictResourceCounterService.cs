using System;
using System.Collections.Generic;
using Bindito.Core;
using Timberborn.Common;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using UnityEngine;

namespace EmploymentAutomation.Logic
{
    public class DistrictResourceCounterService : ITickableSingleton, IPostLoadableSingleton
    {
        private const int TickInterval = 15;

        private readonly Dictionary<DistrictCenter, Dictionary<string, int[]>> stockCounter = new();
        private readonly List<GoodAmount> capacityCache = new();

        private EventBus eventBus;
        private DistrictCenterRegistry districtCenterRegistry;
        private int tickCounter;

        public void PostLoad()
        {
            eventBus.Register(this);
            UpdateResources();
        }

        public void Tick()
        {
            if (++tickCounter < TickInterval)
                return;
            tickCounter = 0;
            UpdateResources();
        }

        [Inject]
        public void InjectDependencies(DistrictCenterRegistry districtCenterRegistry, EventBus eventBus)
        {
            this.districtCenterRegistry = districtCenterRegistry;
            this.eventBus = eventBus;
        }

        [OnEvent]
        public void OnNewGameInitialized(NewGameInitializedEvent newGameInitializedEvent) => UpdateResources();

        public float GetFillRate(DistrictCenter districtCenter, string goodId)
        {
            float result;
            if (districtCenter && stockCounter.TryGetValue(districtCenter, out var districtStockCounter))
            {
                if (districtStockCounter.TryGetValue(goodId, out var goodStats))
                {
                    if (goodStats[0] == 0)
                        result = 0f;
                    else if (goodStats[1] == 0)
                        result = 1f;
                    else
                        result = Mathf.Clamp01((float)goodStats[0] / goodStats[1]);
                }
                else
                {
                    result = 0f;
                }
            }
            else
            {
                result = 0f;
            }

            return result;
        }

        private void UpdateResources()
        {
            ResetCounter();
            var districtCenters = districtCenterRegistry.FinishedDistrictCenters;
            foreach (var districtCenter in districtCenters)
            {
                var districtInventoryRegistry = districtCenter.GetComponent<DistrictInventoryRegistry>();
                foreach (var inventory in districtInventoryRegistry.Inventories)
                {
                    if (inventory.IsUnblocked && inventory.Enabled)
                    {
                        AddInventoryToCounter(inventory);
                    }
                }
            }
        }

        private void ResetCounter()
        {
            foreach (var districtGoods in stockCounter.Values)
            {
                foreach (var counts in districtGoods.Values)
                {
                    counts[0] = 0;
                    counts[1] = 0;
                }
            }
        }

        private void AddInventoryToCounter(Inventory inventory)
        {
            var districtCenter = inventory.GetComponent<DistrictBuilding>().InstantDistrict;
            if (!districtCenter)
            {
                return;
            }

            if (!stockCounter.ContainsKey(districtCenter))
                stockCounter.Add(districtCenter, new Dictionary<string, int[]>());

            capacityCache.Clear();
            inventory.GetCapacity(capacityCache);
            foreach (var good in capacityCache)
            {
                if (inventory.Gives(good.GoodId))
                    AddAmountToCounter(districtCenter, good.GoodId, good.Amount, 1);
            }

            foreach (var good in inventory.Stock)
            {
                if (inventory.Gives(good.GoodId))
                    AddAmountToCounter(districtCenter, good.GoodId, good.Amount, 0);
            }
        }

        private void AddAmountToCounter(DistrictCenter districtCenter, string goodId, int amount, int index)
        {
            if (!stockCounter.TryGetValue(districtCenter, out var districtStockCounter)) return;
            if (districtStockCounter.TryGetValue(goodId, out var goodStats))
            {
                goodStats[index] += amount;
            }
            else
            {
                goodStats = new int[2];
                goodStats[index] = amount;
                districtStockCounter.Add(goodId, goodStats);
            }
        }
    }
}