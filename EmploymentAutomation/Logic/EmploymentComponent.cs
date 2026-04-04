using Bindito.Core;
using Timberborn.Buildings;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.WorkSystem;
using UnityEngine;

namespace EmploymentAutomation.Logic;

public class EmploymentComponent : TickableComponent
{
    private Workplace workplace;
    private PowerComponent powerComponent;
    private ProductComponent productComponent;
    private IngredientComponent ingredientComponent;
    private PausableBuilding pausableBuilding;

    [Inject]
    public void InjectDependencies(
        EventBus eventBus)
    {
        eventBus.Register(this);
    }

    private void UpdateComponents()
    {
        workplace = GetComponent<Workplace>();
        powerComponent = GetComponent<PowerComponent>();
        productComponent = GetComponent<ProductComponent>();
        ingredientComponent = GetComponent<IngredientComponent>();
        pausableBuilding = GetComponent<PausableBuilding>();
    }

    public override void StartTickable() => UpdateComponents();

    public override void Tick()
    {
        var ingredientsEnabled = ingredientComponent is { Available: true, Active: true };
        var productEnabled = productComponent is { Available: true, Active: true };
        var powerEnabled = powerComponent is { Available: true, Active: true };
        var hasToTick = workplace is not null && pausableBuilding is not null &&
                        (ingredientsEnabled || productEnabled || powerEnabled);
        if (!hasToTick) return;
        // employment trigger bounds
        var bounds = new Vector2Int(workplace.MaxWorkers, workplace.MaxWorkers);
        if (powerEnabled) bounds = Vector2Int.Min(bounds, powerComponent.EmploymentBounds);
        if (ingredientsEnabled) bounds = Vector2Int.Min(bounds, ingredientComponent.EmploymentBounds);
        if (productEnabled) bounds = Vector2Int.Min(bounds, productComponent.EmploymentBounds);

        // perform employment
        var currentDesiredWorkers = GetDesiredWorkers();
        if (currentDesiredWorkers < bounds.x)
            IncreaseDesiredWorkers();
        else if (currentDesiredWorkers > bounds.y)
            DecreaseDesiredWorkers();
    }

    private int GetDesiredWorkers() => pausableBuilding.Paused ? 0 : workplace.DesiredWorkers;


    private void IncreaseDesiredWorkers()
    {
        if (pausableBuilding.Paused)
        {
            pausableBuilding.Resume();
        }
        else
        {
            workplace.IncreaseDesiredWorkers();
        }
    }

    private void DecreaseDesiredWorkers()
    {
        if (workplace.DesiredWorkers > 1)
        {
            workplace.DecreaseDesiredWorkers();
        }
        else
        {
            pausableBuilding.Pause();
        }
    }
}