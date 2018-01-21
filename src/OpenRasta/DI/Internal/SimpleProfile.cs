﻿using System;

namespace OpenRasta.DI.Internal
{
  class SimpleProfile : ResolveProfile
  {
    readonly DependencyRegistration _dependency;
    readonly ResolveContext _ctx;

    public SimpleProfile(DependencyRegistration dependency, ResolveContext context)
    {
      _dependency = dependency;
      _ctx = context;
    }

    public override bool TryResolve(out object instance)
    {
      return _ctx.TryResolve(_dependency, out instance);
    }
    
  }
}