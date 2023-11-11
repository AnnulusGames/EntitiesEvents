using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents
{
    [NativeContainer]
    [NativeContainerIsAtomicWriteOnly]
    public unsafe struct EventWriter<T>
        where T : unmanaged
    {
        public EventWriter(in Events<T> events)
        {
            buffer = events.GetBuffer();

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(events.m_Safety);
            var ash = events.m_Safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref ash);
            m_Safety = ash;
#endif
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            buffer->WriteNoResize(value);
        }
    }
}

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEventWriter<T>
        where T : unmanaged
    {
        public UnsafeEventWriter(in UnsafeEvents<T> events)
        {
            buffer = events.buffer;
        }

        [NativeDisableUnsafePtrRestriction] EventsData<T>* buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(in T value)
        {
            buffer->Write(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNoResize(in T value)
        {
            buffer->WriteNoResize(value);
        }
    }
}