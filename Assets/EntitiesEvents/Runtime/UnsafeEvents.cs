using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using EntitiesEvents.Internal;

namespace EntitiesEvents.LowLevel.Unsafe
{
    public unsafe struct UnsafeEvents<T> : IDisposable
        where T : unmanaged
    {
        public UnsafeEvents(int initialCapacity, Allocator allocator)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (allocator <= Allocator.None) throw new ArgumentException("Allocator must be Temp, TempJob, Persistent or registered custom allcoator", "allocator");
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "InitialCapacity must be >= 0");
#endif

            var size = UnsafeUtility.SizeOf<EventsData<T>>();
            buffer = (EventsData<T>*)UnsafeUtility.MallocTracked(size, UnsafeUtility.AlignOf<EventsData<T>>(), allocator, 1);
            UnsafeUtility.MemClear(buffer, size);

            var data = new EventsData<T>(initialCapacity, allocator);
            UnsafeUtility.CopyStructureToPtr(ref data, buffer);

            this.allocator = allocator;
        }

        [NativeDisableUnsafePtrRestriction] internal EventsData<T>* buffer;
        readonly Allocator allocator;

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return buffer != null;
            }
        }

        public void Dispose()
        {
            CheckBuffer();
            buffer->Dispose();
            UnsafeUtility.FreeTracked(buffer, allocator);
            buffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            CheckBuffer();
            buffer->Update();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventWriter<T> GetWriter()
        {
            CheckBuffer();
            return new UnsafeEventWriter<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeEventReader<T> GetReader()
        {
            CheckBuffer();
            return new UnsafeEventReader<T>(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckBuffer()
        {
            if (buffer == null) throw new InvalidOperationException();
        }
    }
}