namespace Bot.Data
{
    public class TokenData
    {
        private readonly string _name;
        private readonly string _value;

        public TokenData(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public string Name => _name;

        public string Value => _value;
    }
}