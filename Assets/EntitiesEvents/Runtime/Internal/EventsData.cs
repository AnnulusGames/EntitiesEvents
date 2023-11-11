using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace EntitiesEvents.Internal
{
    public readonly struct EventInstance<T> where T : unmanaged
    {
        public readonly T value;
        public readonly uint id;

        public EventInstance(in T value, uint id)
        {
            this.value = value;
            this.id = id;
        }
    }

    public struct EventsData<T> : IDisposable
        where T : unmanaged
    {
        internal UnsafeList<EventInstance<T>> buffer1;
        internal UnsafeList<EventInstance<T>> buffer2;
        internal uint eventCounter;
        internal uint prevEventCounter;
        bool state;

        internal UnsafeList<EventInstance<T>> GetWriteBuffer() => state ? buffer2 : buffer1;
        internal UnsafeList<EventInstance<T>> GetReadBuffer() => state ? buffer1 : buffer2;

        public EventsData(int capacity, Allocator allocator)
        {
            buffer1 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            buffer2 = new UnsafeList<EventInstance<T>>(capacity, allocator);
            eventCounter = 0;
            prevEventCounter = 0;
            state = false;
        }

        public void Update()
        {
            state = !state;
            if (state) buffer2.Clear();
            else buffer1.Clear();

            prevEventCounter = eventCounter;
        }

        public void Write(in T value)
        {
            if (state) buffer2.Add(new EventInstance<T>(value, eventCounter));
            else buffer1.Add(new EventInstance<T>(value, eventCounter));
            eventCounter++;
        }

        public void WriteNoResize(in T value)
        {
            if (state) buffer2.AsParallelWriter().AddNoResize(new EventInstance<T>(value, eventCounter));
            else buffer1.AsParallelWriter().AddNoResize(new EventInstance<T>(value, eventCounter));
            eventCounter++;
        }

        public void Dispose()
        {
            buffer1.Dispose();
            buffer2.Dispose();
        }

        public void EnsureCapacity(int capacity)
        {
            while (buffer1.Length < capacity && buffer2.Length < capacity)
            {
                buffer1.Resize(buffer1.Length * 2);
                buffer2.Resize(buffer2.Length * 2);
            }
        }
    }

    public readonly unsafe ref struct EventsDataIterator<T> where T : unmanaged
    {
        public EventsDataIterator(EventsData<T>* buffer, uint eventCounter)
        {
            this.buffer = buffer;
            this.eventCounter = eventCounter;
        }
        readonly EventsData<T>* buffer;
        readonly uint eventCounter;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(buffer->GetReadBuffer(), buffer->GetWriteBuffer(), eventCounter);
        }

        public struct Enumerator : IEnumerator<T>
        {
            public Enumerator(UnsafeList<EventInstance<T>> buffer1, UnsafeList<EventInstance<T>> buffer2, uint eventCounter)
            {
                reader1 = buffer1.AsParallelReader();
                reader2 = buffer2.AsParallelReader();
                this.eventCounter = eventCounter;
                current = default;
                offset = default;
                readFirstReader = default;
            }

            readonly UnsafeList<EventInstance<T>>.ParallelReader reader1;
            readonly UnsafeList<EventInstance<T>>.ParallelReader reader2;
            readonly uint eventCounter;
            T current;
            int offset;
            bool readFirstReader;

            public T Current => current;
            object IEnumerator.Current => current;

            public void Dispose() { }

            public bool MoveNext()
            {
                var reader = readFirstReader ? reader2 : reader1;
                if (reader.Ptr != null && reader.Length > offset)
                {
                    ref var instance = ref *(reader.Ptr + offset);
                    offset++;

                    if (instance.id < eventCounter) return MoveNext();
                    current = instance.value;
                    return true;
                }
                else if (!readFirstReader)
                {
                    readFirstReader = true;
                    offset = 0;
                    return MoveNext();
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}