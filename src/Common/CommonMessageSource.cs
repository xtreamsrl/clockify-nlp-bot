using Microsoft.Extensions.Localization;

namespace Bot.Common
{
    public class CommonMessageSource: ICommonMessageSource
    {
        private readonly IStringLocalizer<CommonMessageSource> _localizer;

        public CommonMessageSource(IStringLocalizer<CommonMessageSource> localizer)
        {
            _localizer = localizer;
        }

        public string ThanksAnswer => GetString(nameof(ThanksAnswer));
        public string InsultAnswer => GetString(nameof(InsultAnswer));
        public string HelpIntro => GetString(nameof(HelpIntro));
        public string HelpDescription => GetString(nameof(HelpDescription));
        public string HelpLanguage => GetString(nameof(HelpLanguage));
        public string HelpSecurityInfo => GetString(nameof(HelpSecurityInfo));
        public string MessageUnhandled => GetString(nameof(MessageUnhandled));

        private string GetString(string name) => _localizer[name];
    }
}