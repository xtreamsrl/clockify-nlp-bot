﻿using System;
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
            
            luisResult.WorkedDuration().Should().Be(timePeriod);
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
            
            Func<string> getDateTimeWithNullDateTimeEntities = () =>  lsEmptyDateTimeTextInstance.WorkedDuration();
            getDateTimeWithNullDateTimeEntities.Should().ThrowExactly<InvalidWorkedDurationException>()
                .WithMessage("No worked duration has been recognized");
            
            Func<string> getDateTimeWithEmptyEntities = () =>  lsEmptyInstance.WorkedDuration();
            getDateTimeWithEmptyEntities.Should().ThrowExactly<InvalidWorkedDurationException>()
                .WithMessage("No worked duration has been recognized");

            Func<string> getDateTimeWithNullDateTimeText = () => lsNullDateTimeInstance.WorkedDuration();
            getDateTimeWithNullDateTimeText.Should().ThrowExactly<InvalidWorkedDurationException>()
                .WithMessage("No worked duration has been recognized");
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
            
            luisResult.WorkedDurationInMinutes().Should().Be(expectedMinutes);
        }
        
    }
}