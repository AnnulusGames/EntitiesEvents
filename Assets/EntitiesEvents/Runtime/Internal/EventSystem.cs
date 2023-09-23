using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace EntitiesEvents.Internal
{
    [BurstCompile]
    [UpdateInGroup(typeof(EventSystemGroup))]
    public unsafe partial struct EventSystem<T> : ISystem
        where T : unmanaged
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var events = new Events<T>(256, Allocator.Persistent);
            var singleton = new EventSingleton<T> { events = events };

            state.EntityManager.AddComponentData(state.SystemHandle, singleton);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();
            SystemAPI.GetSingleton<EventSingleton<T>>().events.Update();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<EventSingleton<T>>();
            singleton.events.Dispose();
            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<EventSingleton<T>>());
        }

        public readonly EventWriter<T> GetEventWriter()
        {
            return SystemAPI.GetSingleton<EventSingleton<T>>().events.GetWriter();
        }

        public readonly EventReader<T> GetEventReader()
        {
            return SystemAPI.GetSingleton<EventSingleton<T>>().events.GetReader();
        }
    }
}