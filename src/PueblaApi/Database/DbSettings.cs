namespace PueblaApi.Database
{
    public class DbSettings
    {
        public string Host { set; get; }
        public string Port { set; get; }
        public string Database { set; get; }
        public string Username { set; get; }
        public string Password { set; get; }

        public string ConnectionString
        {
            get
            {
                return $"Server={Host}:{Port};User Id={Username};Password={Password};Database={Database};";
            }
        }
    }
}
