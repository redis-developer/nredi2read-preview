# NRedi2Read-Preview
## A Redis + .NET Coding Adventure

A collection of .NET REST services for a mythical online bookstore powered by Redis. It uses:

* [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)
* [RediSearch](https://oss.redislabs.com/redisearch/) via [NRediSearch](https://stackexchange.github.io/StackExchange.Redis/)
* [RedisJSON](https://oss.redislabs.com/redisjson/) via [NReJSON](https://github.com/tombatron/NReJSON)

**Prerequisites:**

* [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)
* [Docker](https://docs.docker.com/get-docker/)
* [Docker Compose](https://docs.docker.com/compose/install/)

## | [Getting Started](#getting-started) | [See Also](#see-also) | [Help](#help) | [License](#license) | [Credit](#credit) |

## Getting Started

### Clone the Repository w/ Submodules

To install this example application, run the following commands:
```bash
git clone git@github.com:redis-developer/nredi2read-preview.git --recurse-submodule
cd nredi2read-preview
```

You can also import the code straight into your IDE:
* [Visual Studio Code](https://code.visualstudio.com/docs/languages/csharp)
* [JetBrains Rider](https://www.jetbrains.com/help/rider/Creating_and_Opening_Projects_and_Solutions.html)

### Start Redis and the .NET Application

Set the environment secrets
```bash
dotnet user-secrets init
dotnet user-secrets set CacheConnection "localhost,abortConnect=false,ssl=false,allowAdmin=false,password="
```

Start the Docker Compose application:
 ```bash
 docker-compose up
 ```

Start the app (in separate shell)
```bash
dotnet run
```

## See Also

Quick Tutorial on Redis' Powerful Modules:

* [RedisJSON Tutorial](https://developer.redislabs.com/howtos/redisjson)
* [RediSearch Tutorial](https://developer.redislabs.com/howtos/redisearch)

## Help

Please post any questions and comments on the [Redis Discord Server](https://discord.gg/redis),
and remember to visit our [Redis Developer Page](https://developer.redislabs.com) for awesome tutorials,
project and tips.

## License

[MIT Licence](http://www.opensource.org/licenses/mit-license.html)

## Credit

- [DaShaun Carter](https://github.com/dashaun) @ [Redis Labs](https://redislabs.com)
- [Guy Royse](https://github.com/guyroyse) @ [Redis Labs](https://redislabs.com)