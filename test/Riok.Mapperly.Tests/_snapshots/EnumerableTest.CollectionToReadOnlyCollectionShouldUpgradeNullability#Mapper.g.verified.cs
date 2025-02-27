﻿//HintName: Mapper.g.cs
// <auto-generated />
#nullable enable
public partial class Mapper
{
    private partial global::B? Map(global::A? source)
    {
        if (source == null)
            return default;
        var target = new global::B();
        if (source.Value != null)
        {
            target.Value = MapToIReadOnlyCollection(source.Value);
        }

        return target;
    }

    private global::System.Collections.Generic.IReadOnlyCollection<string?> MapToIReadOnlyCollection(global::System.Collections.Generic.ICollection<int> source)
    {
        var target = new string?[source.Count];
        var i = 0;
        foreach (var item in source)
        {
            target[i] = item.ToString();
            i++;
        }

        return target;
    }
}