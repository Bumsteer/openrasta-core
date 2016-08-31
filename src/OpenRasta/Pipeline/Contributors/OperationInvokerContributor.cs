﻿using System;
using System.Threading.Tasks;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.Pipeline.Contributors
{
  public class OperationInvokerContributor : KnownStages.IOperationExecution
  {
    readonly IDependencyResolver _resolver;

    public OperationInvokerContributor(IDependencyResolver resolver)
    {
      _resolver = resolver;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Use(ExecuteOperations).After<KnownStages.IRequestDecoding>();
    }

    async Task<PipelineContinuation> ExecuteOperations(ICommunicationContext context)
    {
      var executor = _resolver.Resolve<IOperationExecutor>();
      try
      {
        context.OperationResult = await executor.Execute(context.PipelineData.Operations);
      }
      catch (InterceptorException) when (context.OperationResult != null)
      {
        return PipelineContinuation.RenderNow;
      }
      return PipelineContinuation.Continue;
    }
  }
}
