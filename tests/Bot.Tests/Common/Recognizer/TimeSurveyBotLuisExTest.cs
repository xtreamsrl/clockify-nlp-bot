using System;
using Bot.Common;
using Bot.Common.Recognizer;
using FluentAssertions;
using Microsoft.Bot.Builder.AI.Luis;
using Moq;
using Xunit;

namespace Bot.Tests.Common.Recognizer
{
    public class TimeSurveyBotLuisExTest
    {
        [Fact]
        public void WorkedDuration_ValidEntitiesInstance_ReturnsFirstDateTimeText()
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
        public void WorkedDuration_NullOrEmptyDateTimeInstance_ThrowsException()
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
        public void WorkedDurationInMinutes_EightHoursPeriod_ReturnsMinutes()
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
        
        [Fact]
        public void WorkedPeriod_DateWithoutTime_ReturnsWorkedPeriodFromNineAm()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 15, 0, 0);
            var lastFridayStart = new DateTime(2021, 10, 29, 9, 0, 0);
            var lastFridayEnd = new DateTime(2021, 10, 29, 11, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "last friday",
                        Type = "builtin.datetimeV2.datetime"
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(lastFridayStart);
            end.Should().Be(lastFridayEnd);
        }
        
        [Fact]
        public void WorkedPeriod_PeriodStartingFromDateTime_ReturnsWorkedPeriod()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 15, 0, 0);
            var lastFridayStart = new DateTime(2021, 10, 29, 16, 0, 0);
            var lastFridayEnd = new DateTime(2021, 10, 29, 18, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "last friday at 4pm",
                        Type = "builtin.datetimeV2.datetime"
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(lastFridayStart);
            end.Should().Be(lastFridayEnd);
        }
        
        [Fact]
        public void WorkedPeriod_TillSelectedHour_ReturnsWorkedPeriod()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);
            var expectedEnd = new DateTime(2021, 11, 1, 18, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "till 6 pm",
                        Type = "builtin.datetimeV2.datetime"
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(mondayFirstNovember);
            end.Should().Be(expectedEnd);
        }
        
        [Fact]
        public void WorkedPeriod_FromSelectedHour_ReturnsWorkedPeriod()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);
            var expectedEnd = new DateTime(2021, 11, 1, 18, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "from 4 pm",
                        Type = "builtin.datetimeV2.datetime"
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(mondayFirstNovember);
            end.Should().Be(expectedEnd);
        }
        
        [Fact]
        public void WorkedPeriod_FromToHoursRange_ReturnsWorkedPeriod()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);
            var expectedStart = new DateTime(2021, 11, 1, 9, 0, 0);
            var expectedEnd = new DateTime(2021, 11, 1, 11, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "from 9 am to 11 am",
                        Type = "builtin.datetimeV2.datetime"
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(expectedStart);
            end.Should().Be(expectedEnd);
        }
        
        [Fact]
        public void WorkedPeriod_HoursRangeMismatchWithDuration_ThrowsInvalidWorkedPeriodException()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);

            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "1 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "from 9 am to 11 am",
                        Type = "builtin.datetimeV2.datetime"
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

            Func<(DateTime, DateTime)> action = () => luisResult.WorkedPeriod(mockDateTimeProvider.Object, 60);

            action.Should().ThrowExactly<InvalidWorkedPeriodException>()
                .WithMessage("Worked period time span differs from the duration provided. Expected 60 but got 120");
        }
        
        [Fact]
        public void WorkedPeriod_DateTimeIsDuration_ThrowsInvalidWorkedPeriodException()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);

            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "1 hours",
                        Type = "builtin.datetimeV2.duration"
                    },
                    new InstanceData
                    {
                        Text = "for 60 minutes",
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

            Func<(DateTime, DateTime)> action = () => luisResult.WorkedPeriod(mockDateTimeProvider.Object, 60);

            action.Should().ThrowExactly<InvalidWorkedPeriodException>()
                .WithMessage("Date time type duration is not allowed");
        }
        
        [Fact]
        public void WorkedPeriod_NoHoursRange_ReturnsWorkedPeriodStartingFromNineAm()
        {
            var mondayFirstNovember = new DateTime(2021, 11, 1, 16, 0, 0);
            var expectedStart = new DateTime(2021, 11, 1, 9, 0, 0);
            var expectedEnd = new DateTime(2021, 11, 1, 11, 0, 0);
            
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayFirstNovember);
            
            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "2 hours",
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

            var (start, end) = luisResult.WorkedPeriod(mockDateTimeProvider.Object, 120);

            start.Should().Be(expectedStart);
            end.Should().Be(expectedEnd);
        }
    }
}