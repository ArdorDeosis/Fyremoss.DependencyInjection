using System.Diagnostics.CodeAnalysis;

namespace Fyremoss.DependencyInjection;

/// <summary>
/// A fixed set of contracts.
/// </summary>
internal class ContractSet
{
  private readonly Dictionary<Type, IContract[]> contracts;

  internal ContractSet(Dictionary<Type, IContract[]> contracts)
  {
    this.contracts = contracts;
  }

  /// <summary>
  /// Gets the list of contracts associated with the specified type.
  /// </summary>
  /// <param name="type">The type whose contracts are to be retrieved.</param>
  /// <returns>A list of contracts associated with the specified type.</returns>
  public IReadOnlyList<IContract> Get(Type type) => contracts.TryGetValue(type, out var value) ? value : [];

  /// <summary>
  /// Tries to get the first contract associated with the specified type.
  /// </summary>
  /// <param name="type">The type whose first contract is to be retrieved.</param>
  /// <param name="contract">
  /// When this method returns, contains the first contract associated with the specified type, if found;
  /// otherwise, <c>null</c>.
  /// </param>
  /// <returns><c>true</c> if a contract is found; otherwise, <c>false</c>.</returns>
  public bool TryGetFirst(Type type, [NotNullWhen(true)] out IContract? contract)
  {
    contract = default!;
    return contracts.TryGetValue(type, out var value) && 
      (contract = value.FirstOrDefault()) is not null;
  }
}