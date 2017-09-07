using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRasta.Pipeline;

namespace OpenRasta.DI.Internal
{
  public static class ContextStoreExtensions
  {
    private const string CTX_INSTANCES_KEY_CONCURRENT = nameof(CTX_INSTANCES_KEY_CONCURRENT);
    
    public static ConcurrentDictionary<DependencyRegistration, object> GetConcurrentContextInstances(this IContextStore store)
    {
      lock (store)
      {
        return (ConcurrentDictionary<DependencyRegistration, object>)
        (store[CTX_INSTANCES_KEY_CONCURRENT] ??
         (store[CTX_INSTANCES_KEY_CONCURRENT] = new ConcurrentDictionary<DependencyRegistration, object>()));
      }
    }
  }
}