# AspNetCore.CacheOutput.LiteDB
Provider for caching using ASPNet OutputCache using LiteDatabase.
Use with a ASP.NET Core port of StratWeb.CacheOutput library (https://github.com/Iamcerba/AspNetCore.CacheOutput)

### Initial configuration:

1. Install ASP.NET Core CacheOutput package: **Install-Package AspNetCore.CacheOutput**

2. Install core package: **Install-Package AspNetCore.CacheOutput.LiteDB**

3. In "Startup" class "ConfigureServices" method:

   * Register cache key generator:
   
     ```csharp
     services.AddSingleton<ICacheKeyGenerator, DefaultCacheKeyGenerator>();
     ```
   
   * Register the provider for LiteDB using default database path:
   
     ```csharp
     services.AddSingleton<IApiOutputCache, InMemoryOutputCacheProvider>();
     ```
   
     OR define database path implicity
   
     ```csharp
     services.AddSingleton<IApiOutputCache, LiteDBOutputCacheProvider>(provider =>
     {
         return new LiteDBOutputCacheProvider("newFile.db");
     });
     ```

4. In "Startup" class "Configure" method **initialize cache output**:

   ```csharp
   app.UseCacheOutput();
   ```
   
5. Decorate any controller method with cache output filters: 

```csharp
[CacheOutput(ClientTimeSpan = 0, ServerTimeSpan = 3600, MustRevalidate = true, ExcludeQueryStringFromCacheKey = false)]
```

6. Read https://github.com/filipw/Strathweb.CacheOutput for more details about common filter usage