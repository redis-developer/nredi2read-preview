using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace dotnetredis
{
    public class Program
    {
        private const string SecretName = "CacheConnection";

        private static Lazy<ConnectionMultiplexer> _lazyConnection = CreateConnection();
        private static long _lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private static DateTimeOffset _firstErrorTime = DateTimeOffset.MinValue;
        private static DateTimeOffset _previousErrorTime = DateTimeOffset.MinValue;

        private static readonly object ReconnectLock = new();

        private static IConfigurationRoot Configuration { get; set; }

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return _lazyConnection.Value;
            }
        }

        public static TimeSpan ReconnectMinFrequency => TimeSpan.FromSeconds(60);

        public static TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);

        public static int RetryMaxAttempts => 5;

        public static void Main(string[] args)
        {
            InitializeConfiguration();
            IDatabase cache = GetDatabase();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }

        private static void InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>();

            Configuration = builder.Build();
        }

        public static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new(() =>
            {
                string cacheConnection = Configuration[SecretName];
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }

        private static void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null)
                return;

            try
            {
                oldConnection.Value.Close();
            }
            catch (Exception)
            {
                // Error
            }
        }

        private static void ForceReconnect()
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
        
        private static T BasicRetry<T>(Func<T> func)
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

        public static IDatabase GetDatabase()
        {
            return BasicRetry(() => Connection.GetDatabase());
        }

        public static EndPoint[] GetEndPoints()
        {
            return BasicRetry(() => Connection.GetEndPoints());
        }

        public static IServer GetServer(string host, int port)
        {
            return BasicRetry(() => Connection.GetServer(host, port));
        }

        public static HashEntry[] ToHashEntries(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties
                .Where(x => x.GetValue(obj) != null)
                .Select
                (
                    property =>
                    {
                        object propertyValue = property.GetValue(obj);
                        string hashValue;

                        if (propertyValue is IEnumerable<object>)
                        {
                            hashValue = JsonConvert.SerializeObject(propertyValue);
                        }
                        else
                        {
                            if (propertyValue != null)
                            {
                                hashValue = propertyValue.ToString();
                            }
                            else
                            {
                                return new HashEntry(property.Name, RedisValue.Null);
                            }
                        }

                        return new HashEntry(property.Name, hashValue);
                    }
                )
                .ToArray();
        }

        public static T ConvertFromRedis<T>(HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                if (property.PropertyType == typeof(String[]))
                {
                    String[] blah = {entry.Value.ToString()};
                    property.SetValue(obj, Convert.ChangeType(blah, property.PropertyType));
                }
                else 
                { 
                    property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
                }
            }
            return (T)obj;
        }
    }
}