# ObjectPool&lt;T&gt; class

__Location:__ YAMLyzer.Buffers<br/>
__Source:__ [ObjectPool.cs](https://github.com/DavidKeszei/YAMLyzer/blob/nightly/YAMLyzer/Internals/Buffers/ObjectPool/ObjectPool.cs)

Represent a pool of reusable instances.
```cs
internal class ObjectPool<T> where T: class, IClearable, new()
```

# Properties
#### ObjectPool&lt;T&gt;.Shared
Tread-safe, shared instance of the ObjectPool&lt;T&gt; class.
```cs
public static ObjectPool<T> Shared { get; }
```
##### Return
Return a ObjectPool&lt;T&gt; instance.

# Functions
### ObjectPool&lt;T&gt;.Rent()
Rent a(n) __T__ instance from the pool.

```cs
public T Rent();
```
#### Return
Return a(n) __T__ instance.

-------

### ObjectPool&lt;T&gt;.Return()
Return a(n) __T__ instance to the pool.

```cs
public void Return(T @object);
```

> [!NOTE]
> If you return an object, which not belong to the pool, then the method does nothing.
