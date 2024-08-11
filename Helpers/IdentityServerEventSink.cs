namespace Login.Helpers
{
    using Duende.IdentityServer.Events;
    using Duende.IdentityServer.Services;
    using Microsoft.Extensions.Logging;

    public class IdentityServerEventSink : IEventSink
    {
        private readonly ILogger<IdentityServerEventSink> _logger;

        public IdentityServerEventSink(ILogger<IdentityServerEventSink> logger)
        {
            _logger = logger;
        }

        public Task PersistAsync(Event evt)
        {
            switch (evt.EventType)
            {
                case EventTypes.Success:
                    _logger.LogInformation("IdentityServer Success Event: {@event}", evt);
                    break;
                case EventTypes.Failure:
                    _logger.LogWarning("IdentityServer Failure Event: {@event}", evt);
                    break;
                case EventTypes.Error:
                    _logger.LogError("IdentityServer Error Event: {@event}", evt);
                    break;
                case EventTypes.Information:
                    _logger.LogInformation("IdentityServer Information Event: {@event}", evt);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}