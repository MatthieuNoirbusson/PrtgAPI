﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    class SensorTotalsResponse : IWebResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        protected SensorTotalsItem item;

        internal SensorTotalsResponse(SensorTotalsItem item)
        {
            this.item = item;
        }

        public string GetResponseText(string address)
        {
            var xml = new XElement("data",
                new XElement("prtg-version", item.PrtgVersion),
                new XElement("summary", item.Summary),
                new XElement("upsens", item.UpSens),
                new XElement("downsens", item.DownSens),
                new XElement("warnsens", item.WarnSens),
                new XElement("downacksens", item.DownAckSens),
                new XElement("partialdownsens", item.PartialDownSens),
                new XElement("unusualsens", item.UnusualSens),
                new XElement("pausedsens", item.PausedSens),
                new XElement("undefinedsens", item.UndefinedSens),
                new XElement("totalsens", item.TotalSens),
                new XElement("servertime", item.ServerTime)
            );

            return xml.ToString();
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }
    }
}