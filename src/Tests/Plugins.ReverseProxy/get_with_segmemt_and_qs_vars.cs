﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_segmemt_and_qs_vars : IDisposable
  {
    readonly (HttpResponseMessage response, string content, Action dispose) response;


    public get_with_segmemt_and_qs_vars()
    {
      response = new ProxyServer()
          .FromServer("/proxy/{first}/{second}/?q={third}")
          .ToServer("/proxied/{first}/{second}/?query={third}")
          .GetAsync("http://localhost/proxy/one/two/?q=three")
          .Result;
    }

    [Fact]
    public async Task response_status_body_is_proxied()
    {
      response.content.ShouldBe("http://localhost/proxied/one/two/?query=three");
    }

    public void Dispose()
    {
      response.dispose();
    }
  }
}