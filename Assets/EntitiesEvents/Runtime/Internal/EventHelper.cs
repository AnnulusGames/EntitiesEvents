using Unity.Entities;

namespace EntitiesEvents.Internal
{
    public static class EventHelper
    {
        public static EventWriter<T> GetWriter<T>(ref SystemState state)
            where T : unmanaged
        {
            var handle = state.WorldUnmanaged.GetExistingSystemState<EventSystem<T>>().SystemHandle;
            return state.EntityManager.GetComponentData<EventSingleton<T>>(handle).events.GetWriter();
        }

        public static EventWriter<T> GetWriter<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetWriter<T>(ref systemBase.CheckedStateRef);
        }

        public static EventReader<T> GetReader<T>(ref SystemState state)
            where T : unmanaged
        {
            var handle = state.WorldUnmanaged.GetExistingSystemState<EventSystem<T>>().SystemHandle;
            return state.EntityManager.GetComponentData<EventSingleton<T>>(handle).events.GetReader();
        }

        public static EventReader<T> GetReader<T>(SystemBase systemBase)
            where T : unmanaged
        {
            return GetReader<T>(ref systemBase.CheckedStateRef);
        }
    }
}