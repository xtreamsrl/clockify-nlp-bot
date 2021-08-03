using System.Collections.Generic;
using System.Linq;

namespace Bot.Remind
{
    public interface IRemindServiceResolver
    {
        IRemindService Resolve(string name);
    }
    
    public class RemindServiceResolver: IRemindServiceResolver
    {
        private readonly IEnumerable<IRemindService> _remindServices;

        public RemindServiceResolver(IEnumerable<IRemindService> remindServices)
        {
            _remindServices = remindServices;
        }

        public IRemindService Resolve(string name)
        {
            return _remindServices.Single(p => p.GetType().ToString().Contains(name));
        }
    }
}