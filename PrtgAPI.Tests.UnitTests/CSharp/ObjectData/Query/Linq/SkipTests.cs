﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    [TestClass]
    public class SkipTests : BaseQueryTests
    {
        [TestMethod]
        public void Query_Skip_Standalone()
        {
            ExecuteSkip(
                q => q.Skip(1).ToList(),
                "count=2&start=1",
                s =>
                {
                    Assert.AreEqual(2, s.Count);
                    Assert.AreEqual("Volume IO _Total1", s[0].Name);
                    Assert.AreEqual("Volume IO _Total2", s[1].Name);
                },
                UrlFlag.Columns
            );
        }

        [TestMethod]
        public void Query_Skip_Twice()
        {
            ExecuteSkip(
                q => q.Skip(1).Skip(1).ToList(),
                "count=1&start=2",
                s => Assert.AreEqual(1, s.Count),
                UrlFlag.Columns
            );
        }

        [TestMethod]
        public void Query_Skip_ThreeTimes()
        {
            ExecuteClient(
                q => q.QueryDevices().Skip(1).Skip(1).Skip(1),
                new[] {
                    TestHelpers.RequestDeviceCount,
                    TestHelpers.RequestDevice("count=1&start=3", UrlFlag.Columns)
                },
                s => Assert.AreEqual(1, s.Count())
            );
        }

        [TestMethod]
        public void Query_Skip_Logs()
        {
            ExecuteClient(
                c => c.QueryLogs().Skip(1),
                new[] {
                    TestHelpers.RequestLogCount,
                    TestHelpers.RequestLog("count=4&start=2", UrlFlag.Columns)
                },
                s => Assert.AreEqual(4, s.Count())
            );
        }

        [TestMethod]
        public void Query_Skip_ToTake()
        {
            ExecuteSkip(
                q => q.Skip(1).Take(1).ToList(),
                "count=1&start=1",
                s => Assert.AreEqual("Volume IO _Total1", s.Single().Name),
                UrlFlag.Columns
            );
        }

        [TestMethod]
        public void Query_Skip_FromTake()
        {
            ExecuteNow(q => q.Take(2).Skip(1).ToList(), "count=2", s => Assert.AreEqual("Volume IO _Total1", s.Single().Name), UrlFlag.Columns);
        }

        [TestMethod]
        public void Query_Skip_ToWhere()
        {
            ExecuteSkip(
                q => q.Skip(1).Where(s => s.Name.Contains("Volume")).ToList(),
                "count=2&start=1",
                s => Assert.AreEqual(2, s.Count),
                UrlFlag.Columns
            );
        }

        [TestMethod]
        public void Query_Skip_FromWhere()
        {
            ExecuteClient(
                c => c.QuerySensors().Where(s => s.Name.Contains("Volume")).Skip(1).ToList(),
                new[] {
                    TestHelpers.RequestSensor("count=0&filter_name=@sub(Volume)", null),
                    TestHelpers.RequestSensor("count=2&filter_name=@sub(Volume)&start=1", UrlFlag.Columns)
                },
                s => Assert.AreEqual(2, s.Count)
            );
        }

        [TestMethod]
        public void Query_Skip_OnePage()
        {
            ExecuteSkip(
                q => q.Skip(501).ToList(),
                new[] {"count=500&start=501", "count=199&start=1001"},
                s =>
                {
                    Assert.AreEqual(699, s.Count);
                }, UrlFlag.Columns,
                1200
            );
        }

        [TestMethod]
        public void Query_Skip_TwoPages()
        {
            ExecuteSkip(
                q => q.Skip(1002).ToList(),
                new[] {"count=500&start=1002", "count=198&start=1502" },
                s =>
                {
                    Assert.AreEqual(698, s.Count);
                },
                UrlFlag.Columns,
                1700
            );
        }

        [TestMethod]
        public void Query_Skip_All()
        {
            ExecuteSkip(
                q => q.Skip(1200).ToList(),
                "count=0&start=1200",
                s =>
                {
                    Assert.AreEqual(0, s.Count);
                },
                UrlFlag.Columns,
                1200
            );
        }

        [TestMethod]
        public void Query_Skip_All_WithCount()
        {
            var parameters = new SensorParameters
            {
                Start = 3
            };

            ExecuteClient(
                c => c.StreamSensors(parameters, true),
                new[] {
                    TestHelpers.RequestSensorCount,
                    TestHelpers.RequestSensor("count=0&start=3", UrlFlag.Columns)
                },
                s => s.ToList()
            );
        }

        [TestMethod]
        public void Query_Skip_MoreThanAll()
        {
            ExecuteSkip(
                q => q.Skip(1300).ToList(),
                "count=0&start=1300",
                s =>
                {
                    Assert.AreEqual(0, s.Count);
                },
                UrlFlag.Columns,
                1200
            );
        }
    }
}
