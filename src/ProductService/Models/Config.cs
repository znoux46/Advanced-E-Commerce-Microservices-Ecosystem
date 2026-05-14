namespace ProductService.Models;

public class Config
{
    public DatabaseConfig Database { get; set; } = new();
    public RedisConfig Redis { get; set; } = new();
    public KeyCloakConfig KeyCloak { get; set; } = new();
    public ApplicationConfig Application { get; set; } = new();
    public ApiConfig Api { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = "sqlserver";
    public bool AutoMigration { get; set; } = true;
}

public class RedisConfig
{
    public string ConnectionString { get; set; } = "redis:6379";
    public int DefaultExpiryMinutes { get; set; } = 30;
}

public class KeyCloakConfig
{
    public string Url { get; set; } = "http://keycloak:8080";
    public string Realm { get; set; } = "ecommerce-realm";
    public string ClientId { get; set; } = "product-service";
    public string ClientSecret { get; set; } = string.Empty;
}

public class ApplicationConfig
{
    public string Name { get; set; } = "Product Service";
    public string Version { get; set; } = "1.0.0";
    public int Port { get; set; } = 5000;
    public string Environment { get; set; } = "Development";
}

public class ApiConfig
{
    public bool EnableSwagger { get; set; } = true;
    public string SwaggerTitle { get; set; } = "Product Service API";
    public string ApiPrefix { get; set; } = "api";
}

public class LoggingConfig
{
    public string LogLevel { get; set; } = "Information";
    public bool ConsoleLogging { get; set; } = true;
    public bool FileLogging { get; set; } = false;
    public string LogFilePath { get; set; } = "logs/product-service.log";
}
