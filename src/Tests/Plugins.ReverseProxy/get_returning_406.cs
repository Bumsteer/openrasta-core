﻿using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_returning_406 : IDisposable
  {
    readonly HttpResponseMessage response;
    readonly TestServer server;

    public get_returning_406()
    {
      server = ProxyTestServer.Create();

      response = server
          .CreateRequest("/proxy")
          .AddHeader("Accept", "application/vnd.example")
          .GetAsync()
          .Result;
    }

    [Fact]
    public void response_status_code_is_correct()
    {
      response.StatusCode.ShouldBe(HttpStatusCode.NotAcceptable);
    }

    public void Dispose()
    {
      server.Dispose();
    }
  }
}