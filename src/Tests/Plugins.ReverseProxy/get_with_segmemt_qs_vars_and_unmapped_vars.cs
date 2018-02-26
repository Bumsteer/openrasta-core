﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRasta.Plugins.ReverseProxy;
using Shouldly;
using Tests.Plugins.ReverseProxy.Implementation;
using Xunit;

namespace Tests.Plugins.ReverseProxy
{
  public class get_with_segmemt_qs_vars_and_unmapped_vars 
  {
    [Fact]
    public async Task response_status_body_is_proxied()
    {
      var response = await new ProxyServer()
          .FromServer("/proxy/{first}/{second}/?q={third}")
          .ToServer("/proxied/{first}/{second}/?query={third}")
          .GetAsync("http://localhost/proxy/one/two/?q=three&another=fourth");
      response.content.ShouldBe("http://destination.example/proxied/one/two/?query=three&another=fourth");
      response.dispose();
    }
  }
}