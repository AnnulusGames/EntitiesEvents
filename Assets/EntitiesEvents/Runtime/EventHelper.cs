using Unity.Collections;
using Unity.Entities;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    public static class EventHelper
    {
        public static EventWriter<T> GetEventWriter<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).events.GetWriter();
        }

        public static EventWriter<T> GetEventWriter<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetEventWriter<T>(ref systemBase.CheckedStateRef);
        }

        public static EventReader<T> GetEventReader<T>(this ref SystemState state)
            where T : unmanaged
        {
            return GetOrCreateSingleton<T>(ref state).events.GetReader();
        }

        public static EventReader<T> GetEventReader<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetEventReader<T>(ref systemBase.CheckedStateRef);
        }

        public unsafe static void EnsureBufferCapacity<T>(ref SystemState state, int capacity)
            where T : unmanaged
        {
            var events = GetOrCreateSingleton<T>(ref state).events;
            events.GetBuffer()->EnsureCapacity(capacity);
        }

        static EventSingleton<T> GetOrCreateSingleton<T>(ref SystemState state)
            where T : unmanaged
        {
            var query = state.GetEntityQuery(ComponentType.ReadWrite<EventSingleton<T>>());
            if (query.TryGetSingleton<EventSingleton<T>>(out var singleton)) return singleton;
            else
            {
                var events = new Events<T>(512, Allocator.Persistent);
                singleton = new EventSingleton<T> { events = events };
                state.EntityManager.CreateSingleton(singleton);
                return singleton;
            }
        }
    }
}