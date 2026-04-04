using System;
using Bindito.Core;
using Timberborn.MechanicalSystem;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace EmploymentAutomation.Logic;

public class PowerComponent : TickableComponent, IPersistentEntity, IEmploymentBoundsProvider
{
    private static readonly ComponentKey EmploymentManagerComponentKey =
        new("EmploymentManagerPowerComponent");

    private static readonly PropertyKey<bool> PowerActiveKey = new("Active");
    private static readonly PropertyKey<float> PowerHighKey = new("High");
    private static readonly PropertyKey<float> PowerLowKey = new("Low");

    private static bool? smartPowerDetected;
    private bool permanentlyDisabled = false;

    public bool Available
    {
        get => available && !permanentlyDisabled;
        private set => available = value;
    }

    public bool Active { get; set; } = false;
    public float High { get; set; } = 0.75f;
    public float Low { get; set; } = 0.25f;

    public float Fillrate { get; private set; } = 0;
    public Vector2Int EmploymentBounds { get; private set; }
    private Manufactory manufactory;
    private MechanicalNode mechanicalNode;
    private Workplace workplace;
    private bool available;

    public void Save(IEntitySaver entitySaver)
    {
        var component = entitySaver.GetComponent(EmploymentManagerComponentKey);
        component.Set(PowerActiveKey, Active);
        component.Set(PowerLowKey, Low);
        component.Set(PowerHighKey, High);
    }

    public void Load(IEntityLoader entityLoader)
    {
        try
        {
            var component = entityLoader.GetComponent(EmploymentManagerComponentKey);
            Active = component.Get(PowerActiveKey);
            Low = component.Get(PowerLowKey);
            High = component.Get(PowerHighKey);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    [Inject]
    public void InjectDependencies(
        DistrictResourceCounterService districtResourceCounterService, EventBus eventBus)
    {
        eventBus.Register(this);
    }

    private static bool IsSmartPowerLoaded()
    {
        if (smartPowerDetected.HasValue)
            return smartPowerDetected.Value;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Namespace is "IgorZ.SmartPower.Core")
                {
                    smartPowerDetected = true;
                    return true;
                }
            }
        }
        smartPowerDetected = false;
        return false;
    }

    private void UpdateComponents()
    {
        workplace = GetComponent<Workplace>();
        manufactory = GetComponent<Manufactory>();
        mechanicalNode = GetComponent<MechanicalNode>();
        permanentlyDisabled = IsSmartPowerLoaded();
    }

    public override void StartTickable() => UpdateComponents();

    public override void Tick()
    {
        if (workplace == null)
        {
            Available = false;
            return;
        }

        Available = (mechanicalNode?.IsConsumer ?? false) && (manufactory?.HasCurrentRecipe ?? false);
        if (!Available)
            return;

        var batteries = mechanicalNode?.Graph?.Batteries;
        if (batteries == null)
        {
            Fillrate = mechanicalNode?.Graph?.PowerEfficiency ?? 0f;
            EmploymentBounds = GetEmploymentBoundsPower(Fillrate);
            return;
        }

        float totalCharge = 0f;
        float totalCapacity = 0f;
        foreach (var battery in batteries)
        {
            if (battery.ActiveAndPowered)
            {
                totalCharge += battery.NominalBatteryCharge;
                totalCapacity += battery.NominalBatteryCapacity;
            }
        }

        if (totalCapacity == 0f)
            Fillrate = mechanicalNode?.Graph?.PowerEfficiency ?? 0f;
        else
            Fillrate = totalCharge / totalCapacity;
        EmploymentBounds = GetEmploymentBoundsPower(Fillrate);
    }

    private Vector2Int GetEmploymentBoundsPower(float powerMeter)
    {
        return new Vector2Int(
            powerMeter < High ? 0 : workplace.MaxWorkers,
            powerMeter < Low ? 0 : workplace.MaxWorkers);
    }
}