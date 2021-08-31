namespace Bot.Data
{
    public class TokenData
    {
        private readonly string _id;
        private readonly string _value;

        public TokenData(string id, string value)
        {
            _id = id;
            _value = value;
        }

        public string Id => _id;

        public string Value => _value;
    }
}