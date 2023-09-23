using Unity.Entities;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    public static class SystemExtensions
    {
        public static EventWriter<T> GetEventWriter<T>(this ref SystemState state) where T : unmanaged
        {
            return EventHelper.GetWriter<T>(ref state);
        }

        public static EventWriter<T> GetEventWriter<T>(this SystemBase systemBase) where T : unmanaged
        {
            return EventHelper.GetWriter<T>(systemBase);
        }

        public static EventReader<T> GetEventReader<T>(this ref SystemState state) where T : unmanaged
        {
            return EventHelper.GetReader<T>(ref state);
        }

        public static EventReader<T> GetEventReader<T>(SystemBase systemBase) where T : unmanaged
        {
            return EventHelper.GetReader<T>(systemBase);
        }
    }
}