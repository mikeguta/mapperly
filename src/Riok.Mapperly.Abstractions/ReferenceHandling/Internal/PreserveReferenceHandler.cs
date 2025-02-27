using System.Diagnostics.CodeAnalysis;

namespace Riok.Mapperly.Abstractions.ReferenceHandling.Internal;

/// <summary>
/// A <see cref="IReferenceHandler"/> implementation
/// which returns the same target object instance if encountered the same source object instance.
/// Do not use directly. Should only be used by Mapperly generated code.
/// API surface is not subject to semantic releases and may break in any release.
/// </summary>
public sealed class PreserveReferenceHandler : IReferenceHandler
{
    private readonly Dictionary<(Type, Type), ReferenceHolder> _referenceHolders = new();

    /// <inheritdoc cref="IReferenceHandler.TryGetReference{TSource,TTarget}"/>
    public bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
        where TSource : notnull
        where TTarget : notnull
    {
        var refHolder = GetReferenceHolder<TSource, TTarget>();
        return refHolder.TryGetRef(source, out target);
    }

    /// <inheritdoc cref="IReferenceHandler.SetReference{TSource,TTarget}"/>
    public void SetReference<TSource, TTarget>(TSource source, TTarget target)
        where TSource : notnull
        where TTarget : notnull => GetReferenceHolder<TSource, TTarget>().SetRef(source, target);

    private ReferenceHolder GetReferenceHolder<TSource, TTarget>()
    {
        var mapping = (typeof(TSource), typeof(TTarget));
        if (_referenceHolders.TryGetValue(mapping, out var refHolder))
            return refHolder;

        return _referenceHolders[mapping] = new();
    }

    private class ReferenceHolder
    {
        private readonly Dictionary<object, object> _references = new(ReferenceEqualityComparer.Instance);

        public bool TryGetRef<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
            where TSource : notnull
            where TTarget : notnull
        {
            if (_references.TryGetValue(source, out var targetObj))
            {
                target = (TTarget)targetObj;
                return true;
            }

            target = default;
            return false;
        }

        public void SetRef<TSource, TTarget>(TSource source, TTarget target)
            where TSource : notnull
            where TTarget : notnull
        {
            _references[source] = target;
        }
    }
}
