using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
    public class MethodBasedOperationCreator : IOperationCreator
    {
        readonly IObjectBinderLocator _binderLocator;
        readonly Func<IEnumerable<IMethod>, IEnumerable<IMethod>> _filterMethod;
        readonly IDependencyResolver _resolver;

        //// TODO: Remove when support for arrays is added to containers
        public MethodBasedOperationCreator(IDependencyResolver resolver, IObjectBinderLocator binderLocator)
            : this(resolver.ResolveAll<IMethodFilter>().ToArray(), resolver, binderLocator)
        {
        }

        public MethodBasedOperationCreator(IMethodFilter[] filters, IDependencyResolver resolver, IObjectBinderLocator binderLocator)
        {
            _resolver = resolver;
            _binderLocator = binderLocator;
            _filterMethod = FilterMethods(filters).Chain();
        }

        public IEnumerable<IOperation> CreateOperations(IEnumerable<IType> handlers)
        {
            return from handler in handlers
                   let sourceMethods = handler.GetMethods()
                   let filteredMethods = _filterMethod(sourceMethods)
                   from method in filteredMethods
                   select CreateOperation(handler, method) as IOperation;
        }

      IOperation CreateOperation(IType handler, IMethod method)
      {

        var output = method.OutputMembers.Single();
        if (output.StaticType == typeof(Task))
          return new MethodBasedOperation(_binderLocator, handler, method) { Resolver = _resolver };
        if (output.StaticType.IsGenericType &&
            output.StaticType.GetGenericTypeDefinition() == typeof(Task<>))
          return  new MethodBasedOperation(_binderLocator, handler, method) { Resolver = _resolver };
        return new SyncMethod(handler, method, _binderLocator) {Resolver = _resolver};
      }

      IEnumerable<Func<IEnumerable<IMethod>, IEnumerable<IMethod>>> FilterMethods(IMethodFilter[] filters)
        {
            if (filters == null)
            {
                yield return inMethods => inMethods;
                yield break;
            }
            foreach (var filter in filters)
                yield return filter.Filter;
        }
    }
}