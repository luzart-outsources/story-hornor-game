using System;
using System.Collections.Generic;

namespace UIFramework.Communication
{
    /// <summary>
    /// Message broker pattern for request-response communication
    /// Alternative to EventBus for query scenarios
    /// </summary>
    public class MessageBroker
    {
        private static MessageBroker instance;
        public static MessageBroker Instance => instance ?? (instance = new MessageBroker());
        
        private readonly Dictionary<Type, object> requestHandlers = new Dictionary<Type, object>();
        
        /// <summary>
        /// Register a request handler
        /// </summary>
        public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse>
        {
            var requestType = typeof(TRequest);
            
            if (requestHandlers.ContainsKey(requestType))
            {
                UnityEngine.Debug.LogWarning($"[MessageBroker] Handler for {requestType.Name} already registered, replacing");
            }
            
            requestHandlers[requestType] = handler;
        }
        
        /// <summary>
        /// Send a request and get response
        /// </summary>
        public TResponse Send<TRequest, TResponse>(TRequest request)
            where TRequest : IRequest<TResponse>
        {
            var requestType = typeof(TRequest);
            
            if (requestHandlers.TryGetValue(requestType, out var handler))
            {
                var typedHandler = handler as IRequestHandler<TRequest, TResponse>;
                if (typedHandler != null)
                {
                    try
                    {
                        return typedHandler.Handle(request);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[MessageBroker] Error handling request {requestType.Name}: {ex.Message}");
                        return default;
                    }
                }
            }
            
            UnityEngine.Debug.LogError($"[MessageBroker] No handler registered for {requestType.Name}");
            return default;
        }
        
        /// <summary>
        /// Unregister a handler
        /// </summary>
        public void UnregisterHandler<TRequest, TResponse>()
            where TRequest : IRequest<TResponse>
        {
            var requestType = typeof(TRequest);
            requestHandlers.Remove(requestType);
        }
        
        /// <summary>
        /// Clear all handlers
        /// </summary>
        public void Clear()
        {
            requestHandlers.Clear();
        }
    }
    
    /// <summary>
    /// Marker interface for requests
    /// </summary>
    public interface IRequest<TResponse> { }
    
    /// <summary>
    /// Interface for request handlers
    /// </summary>
    public interface IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        TResponse Handle(TRequest request);
    }
    
    #region Example Usage
    
    // Example Request
    public class GetPlayerDataRequest : IRequest<PlayerDataResponse>
    {
        public int PlayerId { get; set; }
    }
    
    // Example Response
    public class PlayerDataResponse
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
    }
    
    // Example Handler
    public class GetPlayerDataHandler : IRequestHandler<GetPlayerDataRequest, PlayerDataResponse>
    {
        public PlayerDataResponse Handle(GetPlayerDataRequest request)
        {
            // Query domain layer
            // var player = PlayerService.GetPlayer(request.PlayerId);
            
            return new PlayerDataResponse
            {
                Name = "Player1",
                Level = 5,
                Health = 100
            };
        }
    }
    
    // Usage in Controller
    // MessageBroker.Instance.RegisterHandler(new GetPlayerDataHandler());
    // var response = MessageBroker.Instance.Send<GetPlayerDataRequest, PlayerDataResponse>(
    //     new GetPlayerDataRequest { PlayerId = 123 }
    // );
    
    #endregion
}
