using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    [NativeContainer]
    [NativeContainerIsReadOnly]
    public unsafe struct EventReader<T>
        where T : unmanaged
    {
        public EventReader(in Events<T> events)
        {
            buffer = events.GetBuffer();
            eventCounter = buffer->prevEventCounter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(events.m_Safety);
            var ash = events.m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref ash);
            m_Safety = ash;
#endif
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;
        uint eventCounter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventsDataIterator<T> Read()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            var itr = new EventsDataIterator<T>(buffer, eventCounter);
            eventCounter = buffer->eventCounter;
            return itr;
        }
    }
}

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEventReader<T>
        where T : unmanaged
    {
        public UnsafeEventReader(in UnsafeEvents<T> events)
        {
            buffer = events.buffer;
            eventCounter = buffer->prevEventCounter;
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;
        uint eventCounter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventsDataIterator<T> Read()
        {
            var itr = new EventsDataIterator<T>(buffer, eventCounter);
            eventCounter = buffer->eventCounter;
            return itr;
        }
    }
}