﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.Binding;
using OpenRasta.Codecs;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Hydrators;
using OpenRasta.OperationModel.Hydrators.Diagnostics;
using OpenRasta.OperationModel.MethodBased;
using OpenRasta.Testing;
using OpenRasta.Testing.Contexts;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

namespace OpenRasta.Tests.Unit.OperationModel.Hydrators
{
  public abstract class request_entity_reader_context : operation_context<EntityReaderHandler>
  {
    protected IEnumerable<IOperation> Operations { get; set; }

    protected void given_filter()
    {
      Filter = new RequestEntityReaderHydrator(Resolver, Request)
      {
        ErrorCollector = Errors,
        Log = new TraceSourceLogger<CodecLogSource>()
      };
    }

    protected void given_operations()
    {
      Operations = new MethodBasedOperationCreator(
        new[] {new TypeExclusionMethodFilter<object>()},
        Resolver,
        new DefaultObjectBinderLocator()).CreateOperations(new[] {TypeSystem.FromClr<EntityReaderHandler>()}).ToList();
    }

    protected RequestEntityReaderHydrator Filter { get; set; }

    protected void given_operation_has_codec_match<TCodec>(string name, MediaType mediaType, float codecScore)
    {
      Operations.First(x=>x.Name == name).SetRequestCodec(new CodecMatch(new CodecRegistration(typeof(TCodec),Guid.NewGuid(),mediaType), codecScore, 1));

    }

    protected void when_filtering_operations()
    {
      try
      {
        ResultOperation = Filter.Read(Operations).GetAwaiter().GetResult();
      }
      catch (Exception e)
      {
        Error = e;
      }
    }

    public Exception Error { get; set; }

    protected void when_entity_is_read()
    {
      when_filtering_operations();
    }

    protected void given_operation_value(string methodName, string parameterName, object parameterValue)
    {
      Operations.First(x => x.Name == methodName)
        .Inputs.Required()
        .First(x => x.Member.Name == parameterName)
        .Binder.SetInstance(parameterValue)
        .ShouldBeTrue();
    }

    protected IOperation ResultOperation { get; private set; }
  }
}
