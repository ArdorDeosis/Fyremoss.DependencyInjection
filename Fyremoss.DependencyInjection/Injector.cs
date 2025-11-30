using System.Reflection;

namespace Fyremoss.DependencyInjection;

/// <inheritdoc />
internal class Injector : IInjector
{
  private readonly DependencyResolver resolver;
  private readonly CachedConstructorSelector constructorSelector;
  private readonly HashSet<CreationHook> creationHooks;

  internal Injector(ContractRegistry registry, ConstructorSelector constructorSelector, 
    HashSet<CreationHook> creationHooks)
  {
    this.creationHooks = creationHooks;
    this.constructorSelector = new CachedConstructorSelector(constructorSelector);
    registry.Add<IInjector>().ToInstance(this);
    resolver = new DependencyResolver(registry.MakeContractSet(this));
  }

  /// <inheritdoc />
  public T CreateInstance<T>()
  {
    if (constructorSelector.GetConstructor(typeof(T)) is not {} constructor)
      throw new ArgumentException($"Cannot create an instance of type {typeof(T).FullName}; the type has no public constructor.");

    try
    {
      var parameters = ResolveParameterList(constructor);
      var instance = (T)constructor.Invoke(parameters);
      foreach (var hook in creationHooks)
        hook.Invoke(this, instance!);
      return instance;
    }
    catch (Exception exception) when (exception is not DependencyResolutionException)
    {
      throw new DependencyResolutionException(
        $"Failed to create instance of type {typeof(T).FullName}. See inner exception for more information", exception);
    }
  }

  /// <inheritdoc />
  public T ExecuteMethod<T>(Delegate factoryMethod)
  {
    if (factoryMethod is null)
      throw new ArgumentNullException(nameof(factoryMethod));
    if (factoryMethod.Method.ReturnType != typeof(T))
      throw new ArgumentException($"Delegate has wrong return type. Expected return type {typeof(T).FullName}.");

    try
    {
      var parameters = ResolveParameterList(factoryMethod.Method);
      return (T)factoryMethod.DynamicInvoke(parameters)!;
    }
    catch (Exception exception) when (exception is not DependencyResolutionException)
    {
      throw new DependencyResolutionException(
        $"Failed to create instance of type {typeof(T).FullName}. See inner exception for more information", exception);
    }
  }

  /// <inheritdoc />
  public T Resolve<T>() => (T)Resolve(typeof(T));

  /// <inheritdoc />
  public object Resolve(Type type)
  {
    if(type is null)
      throw new ArgumentNullException(nameof(type));
    try
    {
      var instance = resolver.Resolve(type);
      if (type.IsInstanceOfType(instance))
        return instance;
      throw new DependencyResolutionException(
        $"Resolved instance is not assignable to resolved type {type.FullName}");
    }
    catch (Exception exception) when (exception is not DependencyResolutionException)
    {
      throw new DependencyResolutionException(
        $"Failed to create instance of type {type.FullName}. See inner exception for more information", exception);
    }
  }

  private object[] ResolveParameterList(MethodBase method) =>
    method.GetParameters().Select(parameter => resolver.Resolve(parameter.ParameterType)).ToArray();
}