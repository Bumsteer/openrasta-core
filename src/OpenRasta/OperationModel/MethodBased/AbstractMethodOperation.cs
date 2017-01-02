﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.Collections;
using OpenRasta.DI;
using OpenRasta.TypeSystem;

namespace OpenRasta.OperationModel.MethodBased
{
  public abstract class AbstractMethodOperation
  {
    public IType OwnerType { get; }
    public IMethod Method { get; }

    protected AbstractMethodOperation(IMethod method, IObjectBinderLocator binderLocator)
    {
      binderLocator = binderLocator ?? new DefaultObjectBinderLocator();
      OwnerType = (IType) method.Owner;
      Method = method;
      Binders = method.InputMembers.ToDictionary(x => x, binderLocator.GetBinder);
      Inputs = Binders.Select(x => new InputMember(x.Key, x.Value, x.Key.IsOptional));
      ExtendedProperties = new NullBehaviorDictionary<object, object>();
    }

    public IDictionary ExtendedProperties { get; }
    public IEnumerable<InputMember> Inputs { get; }
    IDictionary<IParameter, IObjectBinder> Binders { get; }
    public string Name => Method.Name;
    public IDependencyResolver Resolver { protected get; set; }

    public IEnumerable<T> FindAttributes<T>()
      where T : class => OwnerType.FindAttributes<T>().Concat(Method.FindAttributes<T>());

    public T FindAttribute<T>() where T : class => Method.FindAttribute<T>() ?? OwnerType.FindAttribute<T>();
    public override string ToString() => Method.ToString();

    protected object[] GetParameters()
    {
      CheckInputs();

      var results = (
        from kv in Binders
        let param = kv.Key
        let binder = kv.Value
        select binder.IsEmpty
          ? BindingResult.Success(param.DefaultValue)
          : binder.BuildObject()
      ).ToList();

      if (results.Any(_ => _.Successful == false))
        throw new InvalidOperationException("A parameter wasn't successfully created.");
      return results.Select(r => r.Instance).ToArray();
    }

    /// <summary>
    /// Returns an instance of the type, optionally through the container if it is supported.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="resolver"></param>
    /// <returns></returns>
    protected static object CreateInstance(IType type, IDependencyResolver resolver)
    {
      var typeForResolver = type as IResolverAwareType;
      return resolver == null || typeForResolver == null
        ? type.CreateInstance()
        : typeForResolver.CreateInstance(resolver);
    }

    void CheckInputs()
    {
      if (Inputs.AllReady()) return;

      var notReady = Inputs.WhosNotReady();
      throw new InvalidOperationException(
        $"'{Method.Owner.Name}.{Method.Name} could not execute. " +
        $"These members have not been provided: {notReady.Select(x => x.Name).JoinString(", ")}");
    }
  }
}