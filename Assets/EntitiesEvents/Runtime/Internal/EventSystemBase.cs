using Unity.Burst;
using Unity.Entities;

namespace EntitiesEvents.Internal
{
    [BurstCompile]
    [UpdateInGroup(typeof(EventSystemGroup))]
    public unsafe abstract partial class EventSystemBase<T> : SystemBase
        where T : unmanaged
    {
        [BurstCompile]
        protected override void OnCreate()
        {
            RequireForUpdate<EventSingleton<T>>();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            CompleteDependency();
            SystemAPI.GetSingleton<EventSingleton<T>>().events.Update();
        }

        [BurstCompile]
        protected override void OnDestroy()
        {
            if (SystemAPI.TryGetSingleton<EventSingleton<T>>(out var singleton))
            {
                singleton.events.Dispose();
                EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<EventSingleton<T>>());
            }
        }

        public EventWriter<T> GetEventWriter()
        {
            return SystemAPI.GetSingleton<EventSingleton<T>>().events.GetWriter();
        }

        public EventReader<T> GetEventReader()
        {
            return SystemAPI.GetSingleton<EventSingleton<T>>().events.GetReader();
        }
    }
}