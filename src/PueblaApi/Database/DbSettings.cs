namespace PueblaApi.Database
{
    public class DbSettings
    {
        public string Host { set; get; }
        public string Port { set; get; }
        public string Database { set; get; }
        public string Username { set; get; }
        public string Password { set; get; }

        public bool SeedData { set; get; }

        public string ConnectionString
        {
            get
            {
                return $"User Id={Username};Password={Password};Server={Host};Port={Port};Database={Database};";
            }
        }
    }
}
