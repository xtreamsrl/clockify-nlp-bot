using System;

namespace Bot.Common
{
    public interface ICommonMessageSource
    {
        string ThanksAnswer { get; }
        string InsultAnswer { get; }
        string HelpIntro { get; }
        string HelpDescription { get; }
        string HelpLanguage { get; }
        string HelpSecurityInfo { get; }
        string MessageUnhandled { get; }
        string GenericError { get; }
    }
}