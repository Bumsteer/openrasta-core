﻿using System;
using OpenRasta.Configuration;
using OpenRasta.Plugins.ReverseProxy;

namespace Tests.Plugins.ReverseProxy.Implementation
{
  public class ReverseProxyApi : IConfigurationSource
  {
    readonly ReverseProxyOptions _options;

    public ReverseProxyApi(ReverseProxyOptions options)
    {
      _options = options;
    }

    public void Configure()
    {
      ResourceSpace.Has
          .ResourcesNamed("proxied")
          .AtUri("/proxied")
          .HandledBy<ProxiedHandler>()
          .TranscodedBy<ProxiedCodec>()
          .ForMediaType("text/plain");
      
      ResourceSpace.Has
        .ResourcesNamed("proxy")
        .AtUri("/proxy")
        .ReverseProxyFor(new Uri("http://localhost/proxied"));

      ResourceSpace.Uses.ReverseProxy(_options);
    }
  }
}