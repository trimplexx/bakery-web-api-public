using DotNetEnv;

namespace bakery_web_api;

public class Config
{
    public static ConnectionStrings ConnectionStrings { get; set; }
    public static OAuth OAuth { get; set; }
    public static MetaDev MetaDev { get; set; }
    public static SmtpSettingsJson SmtpSettings { get; set; }
    public static Jwt Jwt { get; set; }
    public static BlobContainerString BlobContainerString { get; set; }

    public static void BindValuesFromConfigFile(WebApplicationBuilder builder)
    {
        // Load environment variables
        Env.Load();
        var keyValuePairs = Env.Load();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(keyValuePairs)
            .Build();
        
        ConnectionStrings = new ConnectionStrings
        {
            BakeryDbCon = config["BAKERYDBCON"],
            BlobContainerCon = config["BLOBCONTAINERCON"],
            PageUrl = config["PAGEURL"]
        };

        OAuth = new OAuth
        {
            ClientId = config["CLIENTID"],
            ClientSecret = config["CLIENTSECRET"],
            RedirectUriGmail = config["REDIRECTURIGMAIL"]
        };

        MetaDev = new MetaDev
        {
            InstagramKey = config["INSTAGRAMKEY"],
            ID = config["ID"],
            AppSecret = config["APPSECRET"],
            RedirectUriFacebook = config["REDIRECTURIFACEBOOK"]
        };

        SmtpSettings = new SmtpSettingsJson
        {
            SmtpServer = config["SMTPSERVER"],
            Username = config["USERNAME"],
            Password = config["PASSWORD"]
        };

        Jwt = new Jwt
        {
            SecretKey = config["JWT_SECRETKEY"]
        };

        BlobContainerString = new BlobContainerString
        {
            BlobKey = config["BLOBKEY"]
        };
        
        config.GetSection("ConnectionStrings").Bind(ConnectionStrings);
        config.GetSection("OAuth").Bind(OAuth);
        config.GetSection("MetaDev").Bind(MetaDev);
        config.GetSection("SmtpSettings").Bind(SmtpSettings);
        config.GetSection("Jwt").Bind(Jwt);
        config.GetSection("BlobContainerString").Bind(BlobContainerString);
    }
}

public class ConnectionStrings
{
    public string BakeryDbCon { get; set; }
    public string BlobContainerCon { get; set; }
    public string PageUrl { get; set; }
}

public class OAuth
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUriGmail { get; set; }
}

public class MetaDev
{
    public string InstagramKey { get; set; }
    public string ID { get; set; }
    public string AppSecret { get; set; }
    public string RedirectUriFacebook { get; set; }
}

public class SmtpSettingsJson
{
    public string SmtpServer { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class Jwt
{
    public string SecretKey { get; set; }
}

public class BlobContainerString
{
    public string BlobKey { get; set; }
}
