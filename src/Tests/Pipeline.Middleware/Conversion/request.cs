﻿using System.Linq;
using OpenRasta.Pipeline;
using Shouldly;
using Tests.Pipeline.Middleware.Infrastructrure;
using Xunit;

namespace Tests.Pipeline.Middleware.Conversion
{
  public class request
  {
    [Fact]
    public void convers_to_pre_exec_contrib()
    {
      var calls = new ContributorCall[]
      {
        new ContributorCall(new UriContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "uri"),
        new ContributorCall(new DoNothingContributor(), OpenRasta.Pipeline.Middleware.IdentitySingleTap, "stuff")
      };
      var middlewareChain = calls.ToMiddleware();
      middlewareChain.First().ShouldBeOfType<PreExecuteMiddleware>();
      middlewareChain.Skip(1).First().ShouldBeOfType<RequestMiddleware>();
    }
  }
}