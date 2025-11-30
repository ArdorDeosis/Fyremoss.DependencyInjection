namespace Fyremoss.DependencyInjection;

/// <summary>
/// Manages the registration of contracts for dependency injection.
/// </summary>
internal class ContractRegistry
{
  private readonly Dictionary<Type, List<object>> contracts = new();

  /// <summary>
  /// Adds a new contract configuration for the specified type.
  /// </summary>
  /// <typeparam name="T">The type of the contract to add.</typeparam>
  /// <returns>The configuration for the specified contract for further configuration.</returns>
  public IContractConfiguration<T> Add<T>() where T : notnull
  {
    var contractBuilder = new ContractConfiguration<T>();
    if (!contracts.ContainsKey(typeof(T)))
      contracts[typeof(T)] = [];
    contracts[typeof(T)].Add(contractBuilder);
    return contractBuilder;
  }
  
  /// <summary>
  /// Creates a <see cref="ContractSet"/> from the registered contracts with the specified <see cref="IInjector"/>.
  /// </summary>
  /// <param name="injector">The injector used by the created contracts.</param>
  public ContractSet MakeContractSet(IInjector injector) =>
    new(contracts.ToDictionary(
      entry => entry.Key,
      entry => entry.Value
        .Select(contractConfiguration => ((dynamic)contractConfiguration).BuildContract(injector))
        .Cast<IContract>()
        .ToArray()));
}