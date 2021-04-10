# dotnetredis-preview

* As close as possible to redi2read
* StackExchange client for basic datatypes
* NRediSearch
* NReJSON

Do this once:
```bash
$ dotnet user-secrets init
$ dotnet user-secrets set CacheConnection "localhost,abortConnect=false,ssl=false,allowAdmin=false,password="
```
Start Docker
```bash
$ git submodule update --init --recursive
$ cd redismod-docker-compose
$ docker-compose up
```
Start the app (in separate shell)
```bash
$ dotnet run
```
