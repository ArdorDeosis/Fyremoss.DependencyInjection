using System.Diagnostics.CodeAnalysis;

namespace Fyremoss.DependencyInjection;

/// <summary>
/// Resolves dependencies for the specified types.
/// </summary>
internal class DependencyResolver
{
  /// <summary>
  /// A set of collection types that are recognized and can be resolved if their item type has contracts.
  /// </summary>
  private static readonly HashSet<Type> SupportedCollectionTypes =
  [
    typeof(IEnumerable<>),
    typeof(IReadOnlyCollection<>),
    typeof(IReadOnlyList<>),
  ];

  private readonly ContractSet contracts;

  public DependencyResolver(ContractSet contracts)
  {
    this.contracts = contracts;
  }

  /// <summary>
  /// Resolves an instance of the specified type.
  /// </summary>
  /// <param name="type">The type to resolve.</param>
  /// <returns>An instance of the specified type.</returns>
  /// <exception cref="DependencyResolutionException">Thrown when the type cannot be resolved.</exception>
  public object Resolve(Type type)
  {
    if (TryResolveFirst(type, out var resolved))
      return resolved;
    if (TryResolveCollection(type, out var resolvedCollection))
      return resolvedCollection;
    throw new DependencyResolutionException($"Cannot resolve type {type.FullName}");
  }

  /// <summary>
  /// Tries to resolve the first contract of the specified type.
  /// </summary>
  /// <param name="type">The type to resolve.</param>
  /// <param name="instance">
  /// When this method returns, contains the resolved instance, if successful; otherwise, <c>null</c>.
  /// </param>
  /// <returns><c>true</c> if a contract exists and could be resolved; otherwise, <c>false</c>.</returns>
  private bool TryResolveFirst(Type type, [NotNullWhen(true)] out object? instance)
  {
    if (contracts.TryGetFirst(type, out var contract))
    {
      instance = contract.Resolve();
      return true;
    }

    instance = null;
    return false;
  }

  /// <summary>
  /// Tries to resolve all contracts of the specified item type of the specified collection type. This only succeeds if
  /// there is at least one contract for the item type. 
  /// </summary>
  /// <param name="collectionType">The collection type containing the type to resolve.</param>
  /// <param name="instances">
  /// When this method returns, contains an array of resolved instances, if successful; otherwise, <c>null</c>.
  /// </param>
  /// <returns><c>true</c> if a collection of instances is resolved; otherwise, <c>false</c>.</returns>
  private bool TryResolveCollection(Type collectionType, [NotNullWhen(true)] out Array? instances)
  {
    instances = null;
    return IsCollectionType(collectionType, out var itemType) && TryResolveAll(itemType, out instances);
  }

  /// <summary>
  /// Tries to resolve all contracts of the specified type. This only succeeds if there is at least one contract for the
  /// item type. 
  /// </summary>
  /// <param name="type">The type to resolve.</param>
  /// <param name="instances">
  /// When this method returns, contains an array of resolved instances, if successful; otherwise, <c>null</c>.
  /// </param>
  /// <returns><c>true</c> if all instances of the specified type are resolved; otherwise, <c>false</c>.</returns>
  private bool TryResolveAll(Type type, [NotNullWhen(true)] out Array? instances)
  {
    var contractList = contracts.Get(type);
    if (contractList.Any())
    {
      instances = Array.CreateInstance(type, contractList.Count);
      contractList
        .Select(contract => contract.Resolve())
        .ToArray().CopyTo(instances, 0);
      return true;
    }

    instances = null;
    return false;
  }

  /// <summary>
  /// Determines whether the specified type is a supported collection type.
  /// </summary>
  private static bool IsCollectionType(Type collectionType, [NotNullWhen(true)] out Type? itemType)
  {
    itemType = collectionType
      .GetInterfaces()
      .Append(collectionType) // include the type itself
      .FirstOrDefault(type => type.IsGenericType && SupportedCollectionTypes.Contains(type.GetGenericTypeDefinition()))?
      .GetGenericArguments()
      .FirstOrDefault();
    return itemType is not null;
  }
}