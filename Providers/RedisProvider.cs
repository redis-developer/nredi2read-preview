using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace dotnetredis.Providers
{
     public class RedisProvider
    {
        private readonly IConfiguration _configuration;
        private const string SecretName = "CacheConnection";
        
        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        private static long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private static DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private static DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;
        private static readonly object ReconnectLock = new();

        public RedisProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _lazyConnection = CreateConnection();
        }

        private static TimeSpan ReconnectMinFrequency => TimeSpan.FromSeconds(60);
        private static TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);
        private static int RetryMaxAttempts => 5;
        
        private ConnectionMultiplexer Connection => _lazyConnection.Value;

        private Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new(() => ConnectionMultiplexer.Connect(_configuration[SecretName]));
        }
        
        private static void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null)
            {
                return;
            }

            try
            {
                oldConnection.Value.Close();
            }
            catch (Exception)
            {
                // Error
            }
        }

        private void ForceReconnect()
                {
                    var utcNow = DateTimeOffset.UtcNow;
                    var previousTicks = Interlocked.Read(ref _lastReconnectTicks);
                    var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
                    var elapsedSinceLastReconnect = utcNow - previousReconnectTime;
        
                    if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                        return;
        
                    lock (ReconnectLock)
                    {
                        utcNow = DateTimeOffset.UtcNow;
                        elapsedSinceLastReconnect = utcNow - previousReconnectTime;
        
                        if (_firstErrorTime == DateTimeOffset.MinValue)
                        {
                            _firstErrorTime = utcNow;
                            _previousErrorTime = utcNow;
                            return;
                        }
        
                        if (elapsedSinceLastReconnect < ReconnectMinFrequency){
                            return;
                        }
        
                        var elapsedSinceFirstError = utcNow - _firstErrorTime;
                        var elapsedSinceMostRecentError = utcNow - _previousErrorTime;
        
                        var shouldReconnect =
                            elapsedSinceFirstError >=
                            ReconnectErrorThreshold
                            && elapsedSinceMostRecentError <=
                            ReconnectErrorThreshold; 
        
                        _previousErrorTime = utcNow;
        
                        if (!shouldReconnect)
                            return;
        
                        _firstErrorTime = DateTimeOffset.MinValue;
                        _previousErrorTime = DateTimeOffset.MinValue;
        
                        var oldConnection = _lazyConnection;
                        CloseConnection(oldConnection);
                        _lazyConnection = CreateConnection();
                        Interlocked.Exchange(ref _lastReconnectTicks, utcNow.UtcTicks);
                    }
                }
        
        private T BasicRetry<T>(Func<T> func)
        {
            var reconnectRetry = 0;
            var disposedRetry = 0;

            while (true)
                try
                {
                    return func();
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        throw;
                    ForceReconnect();
                }
                catch (ObjectDisposedException)
                {
                    disposedRetry++;
                    if (disposedRetry > RetryMaxAttempts)
                        throw;
                }
        }

        public IDatabase Database()
        {
            return BasicRetry(() => Connection.GetDatabase());
        }

        public EndPoint[] GetEndPoints()
        {
            return BasicRetry(() => Connection.GetEndPoints());
        }

        public IServer GetServer(string host, int port)
        {
            return BasicRetry(() => Connection.GetServer(host, port));
        }
    }
}