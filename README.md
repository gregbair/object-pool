# Lagoon

[![.NET Core](https://github.com/gregbair/lagoon/workflows/.NET%20Core/badge.svg?branch=main)](https://github.com/gregbair/lagoon/actions?query=workflow%3A%22.NET+Core%22)

[![Nuget](https://img.shields.io/nuget/v/Lagoon?style=plastic)](https://www.nuget.org/packages/Lagoon/)

Lagoon is an object pool. It pools objects that are expensive to initialize. Think database connections or network connections in general.

## Installation

TBD. There will be a nuget package at some point.

## Usage

```c#
using Lagoon;

var factory = new MyFactory(); // this is a factory for creating and activating your objects, implements IObjectPoolFactory

var pool = new DefaultObjectPool(factory);

using (var obj = await GetObjectAsync()) { // This will either grab an instance from the pool or create a new one.
    // do something with the object
} // the object is not actually disposed, but returned to the pool.

pool.Dispose(); // the pool is disposed, also disposing all objects in the pool.
```

## Notes on Usage

- The objects to be pooled must implement `IDisposable`.
- The pool itself is meant to be used throughout your application.

This repo is a companion to a [blog series](https://www.gregbair.dev/tags/object-pool/) about object pool patterns.
