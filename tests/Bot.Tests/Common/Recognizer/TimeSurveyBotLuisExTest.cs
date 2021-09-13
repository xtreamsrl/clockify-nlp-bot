using System;
using Bot.Common.Recognizer;
using FluentAssertions;
using Microsoft.Bot.Builder.AI.Luis;
using Xunit;

namespace Bot.Tests.Common.Recognizer
{
    public class TimeSurveyBotLuisExTest
    {
        [Fact]
        public void TimePeriod_ValidEntitiesInstance_ReturnsFirstDateTimeText()
        {
            const string timePeriod = "from 01 July to 10 July";
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "from 01 July to 10 July",
                        Type = "builtin.datetimeV2.daterange"
                    },
                    new InstanceData
                    {
                        Text = "this week",
                        Type = "builtin.datetimeV2.daterange"
                    }
                }
            };

            var luisResult = new TimeSurveyBotLuis
            {
                Entities = new TimeSurveyBotLuis._Entities
                {
                    _instance = instances
                }
            };
            
            luisResult.TimePeriod().Should().Be(timePeriod);
        }
        
        [Fact]
        public void TimePeriod_NullOrEmptyDateTimeInstance_ThrowsException()
        {
            var emptyDateTimeTextEntities = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = null,
                        Type = null
                    }
                }
            };
            var lsEmptyDateTimeTextInstance = new TimeSurveyBotLuis
            {
                Entities = new TimeSurveyBotLuis._Entities
                {
                    _instance = emptyDateTimeTextEntities
                }
            };
            
            var nullDateTimeInstance = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = null
            };
            var lsNullDateTimeInstance = new TimeSurveyBotLuis
            {
                Entities = new TimeSurveyBotLuis._Entities
                {
                    _instance = nullDateTimeInstance
                }
            };
            var lsEmptyInstance = new TimeSurveyBotLuis
            {
                Entities = new TimeSurveyBotLuis._Entities
                {
                    _instance = new TimeSurveyBotLuis._Entities._Instance()
                }
            };
            
            Func<string> getDateTimeWithNullDateTimeEntities = () =>  lsEmptyDateTimeTextInstance.TimePeriod();
            getDateTimeWithNullDateTimeEntities.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("No worked period has been recognized");
            
            Func<string> getDateTimeWithEmptyEntities = () =>  lsEmptyInstance.TimePeriod();
            getDateTimeWithEmptyEntities.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("No worked period has been recognized");

            Func<string> getDateTimeWithNullDateTimeText = () => lsNullDateTimeInstance.TimePeriod();
            getDateTimeWithNullDateTimeText.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("No worked period has been recognized");
        }

        [Fact]
        public void TimePeriodInMinutes_EightHoursPeriod_ReturnsMinutes()
        {
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "8 hours",
                        Type = "builtin.datetimeV2.duration"
                    }
                }
            };
            var luisResult = new TimeSurveyBotLuis
            {
                Entities = new TimeSurveyBotLuis._Entities
                {
                    _instance = instances
                }
            };

            const double expectedMinutes = 480.00;
            
            luisResult.TimePeriodInMinutes().Should().Be(expectedMinutes);
        }
        
    }
}