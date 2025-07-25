# YAMLyzer.Objects documentation
Provides classes to represent YAML objects & values in the .NET runtime.

# Content
- [YAMLBase](#yamlbase-class)
- YAMLValue
- YAMLCollection
- YAMLObject


## YAMLBase Class
<span id="#yamlbase-class">Provides common read & write mechanims for complex, mutable YAML types.</span>

__Assembly:__ YAMLyzer</br>
__Source:__ [YAMLBase.cs](https://github.com/DavidKeszei/YAMLyzer/blob/nightly/YAMLyzer/Objects/YAMLBase.cs)

__C# Code__</br>
```csharp
public abstract class YAMLBase: IEntity, IReadableEntity, IWriteableEntity
```

__Implements:__ IEntity, IReadableEntity, IWriteableEntity

### Fields
-----------------------------------------------------------------------------------------------------------
|  Name  | Description                                                                                    |
|:------:|------------------------------------------------------------------------------------------------|
| Key    | Key of the current YAMLBase instance.                                                          |
| TypeOf | Type of the current YAMLBase instance. This property must be overwritten by the child classes. |

__C# Code__</br>
```csharp
public string Key { get; }
public virtual YAMLType TypeOf { get; }
```

### Functions
----------------------------------------------------------------------------------------------------------
__Name__: YAMLBase.Read&lt;T&gt;(ReadOnlySpan&lt;string&gt;)<br/>
__Assembly__: YAMLyzer

Read a(n) T entry from the current YAMLBase instance.
```csharp
public T? Read<T>(ReadOnlySpan<string> route) where T: IEntity;
```

__Arguments__<br/>
`route` ReadOnlySpan&lt;string&gt;<br/>
Route of searched T instance.

__Returns__<br/>
Return a(n) T instance based on the `route` parameter.

__Exceptions__<br/>
`AccessViolationException` - If the searched item is not implement the `IReadableEntity` interface. (Only the `YAMLValue` class not implement this interface)<br/>
`ArgumentException` - If the `route` argument is invalid in the object context. (If key not exists or string value is not valid numeric value.)
