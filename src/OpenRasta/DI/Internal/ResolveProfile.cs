﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRasta.DI.Internal
{
  abstract class ResolveProfile
  {
    static ResolveProfile Simple(ResolveContext ctx, Type serviceType)
    {
      var registration = ctx.Registrations.DefaultRegistrationFor(serviceType);
      return registration != null ? new SimpleProfile(registration, ctx) : null;
    }

    static ResolveProfile Enumerable(ResolveContext ctx, Type serviceType)
    {
      if (serviceType.IsConstructedGenericType == false ||
          serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
        return null;

      
      var innerServiceType = serviceType.GenericTypeArguments[0];
      var registrations = ctx.Registrations[innerServiceType];
      return (ResolveProfile) Activator
        .CreateInstance(
          typeof(EnumerableProfile<>)
          .MakeGenericType(innerServiceType),
          registrations, ctx);
    }

    static ResolveProfile Func(ResolveContext ctx, Type serviceType)
    {
      if (serviceType.IsGenericType == false
          || serviceType.GetGenericTypeDefinition() != typeof(Func<>))
        return null;
      
      var innerType = serviceType.GetGenericArguments()[0];
      var innerProfile = FindProfile(innerType, ctx);
      return innerProfile == null
      ? null
      : (ResolveProfile)Activator.CreateInstance(typeof(FuncProfile<>).MakeGenericType(innerType), innerProfile);
    }

    public abstract bool TryResolve(out object instance);

    public static ResolveProfile FindProfile(Type serviceType, ResolveContext resolveContext)
    {
      return Simple(resolveContext, serviceType)
             ?? Enumerable(resolveContext, serviceType)
             ?? Func(resolveContext, serviceType);
    }
  }
}