__Entwine / Buffers__

# PoolItem&lt;T&gt; struct

__Location:__ Entwine.Buffers<br/>
__Source:__ [PoolItem.cs](https://github.com/DavidKeszei/Entwine/blob/nightly/Entwine/Internals/Buffers/PoolItem.cs)

Wrapper structure for tracking pool items.
```cs
internal record struct PoolItem<T> where T: class, IClearable, new()
```

# Properties

#### PoolItem&lt;T&gt;.Data
Underlying instance of the PoolItem&lt;T&gt; struct.
```cs
public readonly PoolItem<T> Data { get; }
```
__Return:__ Return the underlying T instance.

<br/>

#### PoolItem&lt;T&gt;.IsUsed
Indicates the underlying data is used somewhere else.
```cs
public readonly PoolItem<T> Data { get; }
```
__Return:__ Indicates the T data is used somewhere else in the code.
