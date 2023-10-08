# Entities Events
Provides inter-system messaging functionality to Unity ECS

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[日本語版READMEはこちら](README_JP.md)

## Overview

Entities Events is a library that adds event functionality to Unity's Entity Component System (ECS). It allows for easy implementation of messaging between systems using EventWriter/EventReader.

## Features

* Natural inter-system messaging using EventWriter/EventReader
* Creating a custom event system using Events<T>

### Requirements

* Unity 2022.1 or higher
* Entities 1.0.0 or higher

### Installation

1. Open the Package Manager from Window > Package Manager.
2. Click the "+" button and select "Add package from git URL."
3. Enter the following URL:

```
https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents
```

Alternatively, open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
        "com.annulusgames.entities-events": "https://github.com/AnnulusGames/EntitiesEvents.git?path=Assets/EntitiesEvents"
    }
}
```

## Basic Usage

In Entities Events, you perform event writing/reading based on the type of event. First, define a structure to be used for events. The structure used for events cannot contain reference types and must be an unmanaged type.

```cs
public struct MyEvent { }
```

The type of event you want to use must be registered in advance using the `RegisterEvent` attribute. Adding this attribute generates code that includes the necessary System and assembly attributes during compilation.

```cs
using EntitiesEvents;

// Add RegisterEvent attribute to the assembly
[assembly: RegisterEvent(typeof(MyEvent))]
```

In the sending System, use `EventWriter` to publish events.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct WriteEventSystem : ISystem
{
    // Cache the obtained EventWriter within the System
    EventWriter<MyEvent> eventWriter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Obtain the EventWriter with GetEventWriter
        eventWriter = state.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Publish the event using Write
        eventWriter.Write(new MyEvent());
    }
}
```

In the receiving System, use `EventReader` to read the published events.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial struct ReadEventSystem : ISystem
{
    // Cache the obtained EventReader within the System
    EventReader<MyEvent> eventReader;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Obtain the EventReader with GetEventReader
        eventReader = state.GetEventReader<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Read unread events with eventReader.Read()
        foreach (var eventData in eventReader.Read())
        {
            Debug.Log("received!");
        }
    }
}
```

If your System inherits from a class that extends SystemBase, you can obtain EventWriter/EventReader using `this.GetEventWriter<MyEvent>()` or `this.GetEventWriter<MyEvent>()`.

```cs
using Unity.Burst;
using Unity.Entities;
using EntitiesEvents;

[BurstCompile]
public partial class WriteEventSystemClass : SystemBase
{
    EventWriter<MyEvent> eventWriter;

    [BurstCompile]
    protected override OnCreate()
    {
        // Obtain the EventWriter with this.GetEventWriter
        eventWriter = this.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        eventWriter.Write(new MyEvent());
    }
}
```

> **Warning**
> Always obtain EventWriter/EventReader and cache it in OnCreate. In particular, EventReader records the count of unread events for each reader, so calling `state.GetEventReader()` each time you read can lead to duplicated event reads.

## Event Mechanism

Entities Events generates a singleton Entity and a System for each type registered with the `RegisterEvent` attribute to hold event buffers and update the buffers. The generated EventSystem is executed within `EventSystemGroup` and clears the event buffers every frame.

However, events are held for one additional frame after being sent. This means that even if the receiving System is executed before the sending System, there will be a one-frame delay. To prevent this, you can explicitly specify the execution order between Systems using the `UpdateBefore` and `UpdateAfter` attributes.

Also, events have a lifespan of two frames, so if you do not read events every frame, there is a risk of events being lost. If you want to manually update the buffer, you can create your own EventSystem using `Events<T>` as described below.

## Events<T>

A custom NativeContainer `Events<T>` is provided as a collection to store event information.

```cs
using Unity.Collections;
using EntitiesEvents;

// Create a new Events
var events = new Events<MyEvent>(32, Allocator.Temp);
```

You can call `Update` to update the container, which swaps the internal buffer and removes the oldest buffer to prevent memory consumption due to event accumulation. It is recommended to perform this update every frame.

```cs
// Call Update to clear and swap the buffer
events.Update();
```

Writing and reading are done through `EventWriter/EventReader`, which can be obtained using `GetWriter/GetReader`.

```cs
// Obtain EventWriter and write
var eventWriter = events.GetWriter();
eventWriter.Write(new MyEvent());

// Obtain EventReader and read
var eventReader = events.GetReader();
```

After use, like other NativeContainers, you must release the memory using `Dispose`. Forgetting to do this can lead to memory leaks.

```cs
// Dispose to release the container and free memory
events.Dispose();
```

## License

[MIT License](LICENSE)