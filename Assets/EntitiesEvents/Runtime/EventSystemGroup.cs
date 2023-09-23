using Unity.Entities;

namespace EntitiesEvents
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [CreateBefore(typeof(SimulationSystemGroup))]
    public sealed partial class EventSystemGroup : ComponentSystemGroup { }
}