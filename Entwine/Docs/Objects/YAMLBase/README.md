__Entwine / Objects__

# YAMLBase class

__Location:__ Entwine.Objects
__Source:__ [YAMLBase.cs](https://github.com/DavidKeszei/Entwine/blob/nightly/Entwine/Objects/YAMLBase.cs)

Base-class of all complex YAML objects.

```cs
public abstract class YAMLBase: IEntity, IWriteableEntity, IReadableEntity
```

# Properties

#### YAMLBase.TypeOf

Indicates type of the YAML object.

```cs
public YAMLType TypeOf { get; }
```

__Return:__ Return a value from the YAMLType enumeration.



#### YAMLBase.TypeOf

Key of the object inside the YAML document.

```cs
public string Key { get; }
```

__Return:__ Return the key of the object as string.



# Functions

#### YAMLBase.Write&lt;T&gt;(ReadOnlySpan&lt;string&gt; route, T entity) where T: IEntity

Write an IEntity instance to the object based on the route parameter.

```cs
public T Rent();
```

__Return:__ Return a(n) __T__ instance.



### ObjectPool&lt;T&gt;.Return()

Return a(n) __T__ instance to the pool.

```cs
public void Return(T @object);
```

> [!NOTE]
> If you return an object, which not belong to the pool, then the method does nothing.
