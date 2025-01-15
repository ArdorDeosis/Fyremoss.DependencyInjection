﻿using JetBrains.Annotations;

namespace Ember.DependencyInjection;

/// <summary>
/// Marks the constructor as the one to be used for constructor injection.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Constructor)]
public class InjectedConstructorAttribute : Attribute;