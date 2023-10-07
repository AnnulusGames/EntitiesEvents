using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.LowLevel.Unsafe;
using EntitiesEvents.Internal;
using Unity.Burst;

namespace EntitiesEvents
{
    [NativeContainer]
    public struct Events<T> : IDisposable
        where T : unmanaged
    {
        public Events(int initialCapacity, Allocator allocator)
        {
            container = new UnsafeEvents<T>(initialCapacity, allocator);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = CollectionHelper.CreateSafetyHandle(allocator);
            CollectionHelper.SetStaticSafetyId<Events<T>>(ref m_Safety, ref s_staticSafetyId.Data); 
            if (UnsafeUtility.IsNativeContainerType<T>()) AtomicSafetyHandle.SetNestedContainer(m_Safety, true);
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_Safety, true);
#endif
        }

        UnsafeEvents<T> container;

        internal readonly unsafe EventsData<T>* GetBuffer() => container.buffer;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
        internal static readonly SharedStatic<int> s_staticSafetyId = SharedStatic<int>.GetOrCreate<Events<T>>();
#endif

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return container.IsCreated;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            container.Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventWriter<T> GetWriter()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckExistsAndThrow(m_Safety);
#endif
            return new EventWriter<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EventReader<T> GetReader()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckExistsAndThrow(m_Safety);
#endif
            return new EventReader<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            CollectionHelper.DisposeSafetyHandle(ref m_Safety);
#endif
            container.Dispose();
        }
    }
}