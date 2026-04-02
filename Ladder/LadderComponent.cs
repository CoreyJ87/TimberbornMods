namespace Ladder;

public class LadderComponent : BaseComponent
{
    BlockService _blockService = null!;

    [Inject]
    public void InjectDependencies(BlockService blockService)
    {
        _blockService = blockService;
    }
}
