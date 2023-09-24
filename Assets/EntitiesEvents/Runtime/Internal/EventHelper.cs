using Unity.Collections;
using Unity.Entities;

namespace EntitiesEvents.Internal
{
    public static class EventHelper
    {
        public static EventWriter<T> GetWriter<T>(ref SystemState state)
            where T : unmanaged
        {
            var query = state.GetEntityQuery(ComponentType.ReadWrite<EventSingleton<T>>());
            if (query.TryGetSingleton<EventSingleton<T>>(out var value))
            {
                return value.events.GetWriter();
            }
            else
            {
                var events = new Events<T>(256, Allocator.Persistent);
                var singleton = new EventSingleton<T> { events = events };
                state.EntityManager.CreateSingleton(singleton);

                return events.GetWriter();
            }
        }

        public static EventWriter<T> GetWriter<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetWriter<T>(ref systemBase.CheckedStateRef);
        }

        public static EventReader<T> GetReader<T>(ref SystemState state)
            where T : unmanaged
        {
            var query = state.GetEntityQuery(ComponentType.ReadWrite<EventSingleton<T>>());
            if (query.TryGetSingleton<EventSingleton<T>>(out var value))
            {
                return value.events.GetReader();
            }
            else
            {
                var events = new Events<T>(256, Allocator.Persistent);
                var singleton = new EventSingleton<T> { events = events };
                state.EntityManager.CreateSingleton(singleton);

                return events.GetReader();
            }
        }

        public static EventReader<T> GetReader<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetReader<T>(ref systemBase.CheckedStateRef);
        }
    }
}