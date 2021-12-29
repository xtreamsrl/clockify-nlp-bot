using System.Collections.Generic;
using System.Linq;

namespace Bot.Remind
{
    public interface ISpecificRemindServiceResolver
    {
        ISpecificRemindService Resolve(string name);
    }
    
    public class SpecificRemindServiceResolver: ISpecificRemindServiceResolver
    {
        private readonly IEnumerable<ISpecificRemindService> _remindServices;

        public SpecificRemindServiceResolver(IEnumerable<ISpecificRemindService> remindServices)
        {
            _remindServices = remindServices;
        }

        public ISpecificRemindService Resolve(string name)
        {
            return _remindServices.Single(p => p.GetType().ToString().Contains(name));
        }
    }
}