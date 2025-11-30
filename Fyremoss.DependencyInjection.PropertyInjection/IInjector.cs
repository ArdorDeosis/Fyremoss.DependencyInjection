using System.Reflection;
using JetBrains.Annotations;

namespace Fyremoss.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IInjector"/> to support property injection.
/// </summary>
[PublicAPI]
public static class InjectorExtensions
{
  /// <summary>
  /// Injects all properties marked with the <see cref="InjectAttribute"/> in the given <paramref name="instance"/>.
  /// </summary>
  /// <param name="injector">The injector to resolve services from.</param>
  /// <param name="instance">The instance getting its properties set.</param>
  /// <exception cref="ArgumentNullException">
  /// Thrown when either <paramref name="injector"/> or <paramref name="instance"/> are <c>null</c>.
  /// </exception>
  /// <exception cref="DependencyResolutionException">
  /// Thrown when a property is marked with the <see cref="InjectAttribute"/> but has no setter.
  /// </exception>
  public static void InjectProperties(this IInjector injector, object instance)
  {
    if(injector is null)
      throw new ArgumentNullException(nameof(injector));
    if(instance is null)
      throw new ArgumentNullException(nameof(instance));

    var properties = instance.GetType()
      .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

    foreach (var property in properties.Where(property => property.GetCustomAttribute<InjectAttribute>() is not null))
    {
      if (property.SetMethod == null)
        throw new DependencyResolutionException($"Property '{property.Name}' on type '{instance.GetType().FullName}' is marked with [Inject] but does not have a setter.");
      property.SetValue(instance, injector.Resolve(property.PropertyType));
    }
  }
}