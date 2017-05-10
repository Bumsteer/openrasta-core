using System;
using System.Threading.Tasks;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.InMemory
{
  public class InMemoryHost : IHost, IDependencyResolverAccessor, IDisposable
  {
    readonly IConfigurationSource _configuration;
    bool _isDisposed;

    public InMemoryHost(IConfigurationSource configuration, IDependencyResolver dependencyResolver = null)
    {
      Resolver = dependencyResolver ?? new InternalDependencyResolver();
      _configuration = configuration;
      ApplicationVirtualPath = "/";
      HostManager = HostManager.RegisterHost(this);
      RaiseStart();
    }

    public event EventHandler<IncomingRequestProcessedEventArgs> IncomingRequestProcessed;
    public event EventHandler<IncomingRequestReceivedEventArgs> IncomingRequestReceived;

    public event EventHandler Start;
    public event EventHandler Stop;
    public string ApplicationVirtualPath { get; private set; }
    public HostManager HostManager { get; private set; }
    public IDependencyResolver Resolver { get; private set; }

    IDependencyResolverAccessor IHost.ResolverAccessor => this;

    public void Close()
    {
      RaiseStop();
      HostManager.UnregisterHost(this);
      _isDisposed = true;
    }

    [Obsolete("Please use the async version, this one may and will deadlock")]
    public IResponse ProcessRequest(IRequest request)
    {
      return ProcessRequestAsync(request).Result;
    }

    public async Task<IResponse> ProcessRequestAsync(IRequest request)
    {
      CheckNotDisposed();
      var ambientContext = new AmbientContext();
      var context = new InMemoryCommunicationContext
      {
        ApplicationBaseUri = new Uri("http://localhost"),
        Request = request,
        Response = new InMemoryResponse()
      };
      try
      {
        using (new ContextScope(ambientContext))
        {
          await RaiseIncomingRequestReceived(context);
        }
      }
      finally
      {
        using (new ContextScope(ambientContext))
        {
          RaiseIncomingRequestProcessed(context);
        }
      }
      return context.Response;

    }
    void IDisposable.Dispose()
    {
      Close();
    }

    bool IHost.ConfigureLeafDependencies(IDependencyResolver resolver)
    {
      CheckNotDisposed();
      return true;
    }

    bool IHost.ConfigureRootDependencies(IDependencyResolver resolver)
    {
      CheckNotDisposed();
      resolver.AddDependencyInstance<IContextStore>(new InMemoryContextStore());
      if (_configuration != null)
        Resolver.AddDependencyInstance<IConfigurationSource>(_configuration, DependencyLifetime.Singleton);
      return true;
    }

    protected virtual void RaiseIncomingRequestProcessed(ICommunicationContext context)
    {
      IncomingRequestProcessed.Raise(this, new IncomingRequestProcessedEventArgs(context));
    }

    protected virtual Task RaiseIncomingRequestReceived(ICommunicationContext context)
    {
      var incomingRequestReceivedEventArgs = new IncomingRequestReceivedEventArgs(context);
      IncomingRequestReceived.Raise(this, incomingRequestReceivedEventArgs);
      return incomingRequestReceivedEventArgs.RunTask;
    }

    protected virtual void RaiseStart()
    {
      Start.Raise(this, EventArgs.Empty);
    }

    protected virtual void RaiseStop()
    {
      Stop.Raise(this, EventArgs.Empty);
    }

    void CheckNotDisposed()
    {
      if (_isDisposed)
        throw new ObjectDisposedException("HttpListenerHost");
    }
  }
}