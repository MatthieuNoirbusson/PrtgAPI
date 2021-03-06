﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class MultiTypeResponse : IWebStreamResponse
    {
        private SensorType? newSensorType;

        public MultiTypeResponse()
        {
        }

        public MultiTypeResponse(Dictionary<Content, int> countOverride)
        {
            CountOverride = countOverride;
        }

        public MultiTypeResponse(Dictionary<Content, BaseItem[]> items)
        {
            ItemOverride = items;
        }

        public Dictionary<Content, int> CountOverride { get; set; }
        public Dictionary<Content, Action<BaseItem>> PropertyManipulator { get; set; }
        public Dictionary<Content, BaseItem[]> ItemOverride { get; set; }
        private Dictionary<string, int> hitCount = new Dictionary<string, int>();

        public int? FixedCountOverride { get; set; }

        public string GetResponseText(ref string address)
        {
            var function = GetFunction(address);

            if (hitCount.ContainsKey(function))
                hitCount[function]++;
            else
                hitCount.Add(function, 1);

            return GetResponse(ref address, function).GetResponseText(ref address);
        }

        public async Task<string> GetResponseTextStream(string address)
        {
            var function = GetFunction(address);

            if (hitCount.ContainsKey(function))
                hitCount[function]++;
            else
                hitCount.Add(function, 1);

            return await GetResponseStream(address, function).GetResponseTextStream(address);
        }

        protected virtual IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                case nameof(XmlFunction.HistoricData):
                    return GetTableResponse(address, function, false);
                case nameof(CommandFunction.Pause):
                case nameof(CommandFunction.PauseObjectFor):
                    return new BasicResponse("<a data-placement=\"bottom\" title=\"Resume\" href=\"#\" onclick=\"var self=this; _Prtg.objectTools.pauseObject.call(this,'1234',1);return false;\"><i class=\"icon-play icon-dark\"></i></a>");
                case nameof(HtmlFunction.ChannelEdit):
                    var components = UrlHelpers.CrackUrl(address);

                    if(components["channel"] != "99")
                        return new ChannelResponse(new ChannelItem());
                    return new BasicResponse(string.Empty);
                case nameof(CommandFunction.DuplicateObject):
                    address = "https://prtg.example.com/public/login.htm?loginurl=/object.htm?id=9999&errormsg=";
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditSettings):
                    return new BasicResponse(string.Empty);
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(JsonFunction.Triggers):
                    return new TriggerOverviewResponse();
                case nameof(JsonFunction.SensorTypes):
                    return new SensorTypeResponse();
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address);
                case nameof(XmlFunction.GetObjectProperty):
                case nameof(XmlFunction.GetObjectStatus):
                    return GetRawObjectProperty(address);
                case nameof(CommandFunction.AddSensor2):
                    newSensorType = UrlHelpers.CrackUrl(address)["sensortype"].ToEnum<SensorType>();
                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditNotification):
                    return new NotificationActionResponse(new NotificationActionItem());
                case nameof(JsonFunction.GetAddSensorProgress):
                    var progress = hitCount[function] % 2 == 0 ? 100 : 50;

                    return new BasicResponse($"{{\"progress\":\"{progress}\",\"targeturl\":\" /addsensor4.htm?id=4251&tmpid=119\"}}");
                case nameof(HtmlFunction.AddSensor4):
                    return GetSensorTargetResponse();
                case nameof(CommandFunction.AcknowledgeAlarm):
                case nameof(CommandFunction.AddSensor5):
                case nameof(CommandFunction.AddDevice2):
                case nameof(CommandFunction.AddGroup2):
                case nameof(CommandFunction.ClearCache):
                case nameof(CommandFunction.DeleteObject):
                case nameof(HtmlFunction.RemoveSubObject):
                case nameof(CommandFunction.DiscoverNow):
                case nameof(CommandFunction.LoadLookups):
                case nameof(CommandFunction.MoveObjectNow):
                case nameof(CommandFunction.RecalcCache):
                case nameof(CommandFunction.Rename):
                case nameof(CommandFunction.RestartServer):
                case nameof(CommandFunction.RestartProbes):
                case nameof(CommandFunction.ReloadFileLists):
                case nameof(CommandFunction.SaveNow):
                case nameof(CommandFunction.ScanNow):
                case nameof(CommandFunction.SetPosition):
                case nameof(CommandFunction.Simulate):
                case nameof(CommandFunction.SortSubObjects):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        protected virtual IWebStreamResponse GetResponseStream(string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return (IWebStreamResponse)GetTableResponse(address, function, true);
                default:
                    throw GetUnknownFunctionException(function, true);
            }
        }

        private IWebResponse GetTableResponse(string address, string function, bool async)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content? content;

            try
            {
                content = components["content"].DescriptionToEnum<Content>();
            }
            catch
            {
                content = null;
            }

            var count = GetCount(components, content);

            //Hack to make test "forces streaming with a date filter and returns no results" work
            if (content == Content.Logs && count == 0 && components["columns"] == "objid,name")
            {
                count = 501;
                address = address.Replace("count=1", "count=501");
            }

            if (function == nameof(XmlFunction.HistoricData))
                return new SensorHistoryResponse(GetItems(i => new SensorHistoryItem(), count));

            var columns = components["columns"]?.Split(',');

            switch (content)
            {
                case Content.Sensors: return Sensors(CreateSensor, count, columns, address, async);
                case Content.Devices: return Devices(CreateDevice, count, columns, address, async);
                case Content.Groups:  return Groups(CreateGroup,   count, columns, address, async);
                case Content.Probes: return Probes(CreateProbe,    count, columns, address, async);
                case Content.Logs:
                    if (IsGetTotalLogs(address))
                        return TotalLogsResponse();

                    return Messages(i => new MessageItem($"WMI Remote Ping{i}"), count);
                case Content.History: return new ModificationHistoryResponse(new ModificationHistoryItem());
                case Content.Notifications: return Notifications(CreateNotification, count);
                case Content.Schedules: return Schedules(CreateSchedule, count);
                case Content.Channels: return new ChannelResponse(GetItems(Content.Channels, i => new ChannelItem(), 1));
                case Content.Objects:
                    return Objects(address, function, components);
                case Content.SysInfo:
                    return new SystemInfoResponse(
                        SystemInfoItem.SystemItem(), SystemInfoItem.HardwareItem(), SystemInfoItem.SoftwareItem(),
                        SystemInfoItem.ProcessItem(), SystemInfoItem.ServiceItem(), SystemInfoItem.UserItem()
                    );
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebStreamResponse Sensors(Func<int, SensorItem> func, int count, string[] columns, string address, bool async) => FilterColumns<Sensor>(new SensorResponse(GetItems(Content.Sensors, func, count)), columns, address, async);
        private IWebStreamResponse Devices(Func<int, DeviceItem> func, int count, string[] columns, string address, bool async) => FilterColumns<Device>(new DeviceResponse(GetItems(Content.Devices, func, count)), columns, address, async);
        private IWebStreamResponse Groups(Func<int, GroupItem> func, int count, string[] columns, string address, bool async) => FilterColumns<Group>(new GroupResponse(GetItems(Content.Groups, func, count)), columns, address, async);
        private IWebStreamResponse Probes(Func<int, ProbeItem> func, int count, string[] columns, string address, bool async) => FilterColumns<Probe>(new ProbeResponse(GetItems(Content.Probes, func, count)), columns, address, async);
        private IWebResponse Messages(Func<int, MessageItem> func, int count) => new MessageResponse(GetItems(func, count));
        private IWebResponse Notifications(Func<int, NotificationActionItem> func, int count) => new NotificationActionResponse(GetItems(func, count));
        private IWebResponse Schedules(Func<int, ScheduleItem> func, int count) => new ScheduleResponse(GetItems(func, count));

        private bool IsGetTotalLogs(string address)
        {
            if (address.Contains("content=messages&count=1&columns=objid,name"))
                return true;

            return false;
        }

        private IWebResponse TotalLogsResponse()
        {
            int count;

            if (!(CountOverride != null && CountOverride.TryGetValue(Content.Logs, out count)))
                count = 1000000;

            return new BasicResponse(new XElement("messages",
                new XAttribute("listend", 1),
                new XAttribute("totalcount", count),
                new XElement("prtg-version", "1.2.3.4"),
                null
            ).ToString());
        }

        private SensorItem CreateSensor(int i)
        {
            var item = new SensorItem(
                name: $"Volume IO _Total{i}",
                typeRaw: "aggregation",
                objid: (4000 + i).ToString(),
                downtimeTimeRaw: (1220 + i).ToString(),
                messageRaw: "OK1" + i,
                lastUpRaw: new DateTime(2000, 10, 1, 4, 2, 1, DateTimeKind.Utc).AddDays(i).ToUniversalTime().ToOADate().ToString(CultureInfo.InvariantCulture)
            );

            return AdjustProperties(item, Content.Sensors);
        }

        private DeviceItem CreateDevice(int i)
        {
            var item = new DeviceItem(
                name: $"Probe Device{i}",
                objid: (3000 + i).ToString(),
                messageRaw: "OK1" + i
            );

            return AdjustProperties(item, Content.Devices);
        }

        private GroupItem CreateGroup(int i)
        {
            var item = new GroupItem(
                name: $"Windows Infrastructure{i}",
                totalsens: "2",
                groupnum: "0",
                objid: (2000 + i).ToString(),
                messageRaw: "OK1" + i
            );

            return AdjustProperties(item, Content.Groups);
        }

        private ProbeItem CreateProbe(int i)
        {
            var item = new ProbeItem(
                name: $"127.0.0.1{i}",
                objid: (1000 + i).ToString(),
                messageRaw: "OK" + i
            );

            return AdjustProperties(item, Content.Probes);
        }

        private NotificationActionItem CreateNotification(int i)
        {
            var item = new NotificationActionItem();

            return AdjustProperties(item, Content.Notifications);
        }

        private ScheduleItem CreateSchedule(int i)
        {
            var item = new ScheduleItem();

            return AdjustProperties(item, Content.Schedules);
        }

        private TItem AdjustProperties<TItem>(TItem item, Content content) where TItem : BaseItem
        {
            if (PropertyManipulator != null)
            {
                Action<BaseItem> action;

                if (PropertyManipulator.TryGetValue(content, out action))
                    action(item);
            }

            return item;
        }

        private IWebResponse Objects(string address, string function, NameValueCollection components)
        {
            var idStr = components["filter_objid"];

            if (idStr != null)
            {
                var ids = idStr.Split(',').Select(v => Convert.ToInt32(v));

                var items = ids.SelectMany(id =>
                {
                    if (id < 400)
                        return GetObject("notifications", address, function);
                    if (id < 700)
                        return GetObject("schedules", address, function);
                    if (id < 2000)
                        return GetObject("probenode", address, function);
                    if (id < 3000)
                        return GetObject("groups", address, function);
                    if (id < 4000)
                        return GetObject("devices", address, function);
                    if (id < 5000)
                        return GetObject("sensors", address, function);

                    var text = new ObjectResponse(new SensorItem()).GetResponseText(ref address);

                    return XDocument.Parse(text).Descendants("item").ToList();
                }).ToArray();

                var xml = new XElement("objects",
                    new XAttribute("listend", 1),
                    new XAttribute("totalcount", items.Length),
                    new XElement("prtg-version", "1.2.3.4"),
                    items
                );

                return new BasicResponse(xml.ToString());
            }

            return new ObjectResponse(
                new SensorItem(),
                new DeviceItem(),
                new GroupItem(),
                new ProbeItem(),
                new ScheduleItem(),
                new NotificationActionItem()
            );
        }

        private List<XElement> GetObject(string newContent, string address, string function)
        {
            var r = GetTableResponse(address.Replace("content=objects", $"content={newContent}").Replace("count=*", "count=1"), function, false);

            var text = r.GetResponseText(ref address);

            return XDocument.Parse(text).Descendants("item").ToList();
        }

        private T[] GetItems<T>(Content content, Func<int, T> func, int count) where T : BaseItem
        {
            BaseItem[] items;
            T[] typedItems;

            if (ItemOverride != null && ItemOverride.TryGetValue(content, out items))
                typedItems = items.Cast<T>().ToArray();
            else
                typedItems = GetItems(func, count);

            return typedItems;
        }

        private IWebStreamResponse FilterColumns<T>(IWebStreamResponse response, string[] columns, string address, bool async) where T : PrtgObject
        {
            if (columns != null)
            {
                var defaultProperties = ContentParameters<T>.GetDefaultProperties();

                var defaultPropertiesStr = defaultProperties.Select(p => p.GetDescription().ToLower()).ToList();

                var missing = defaultPropertiesStr.Where(p => !columns.Contains(p)).ToList();

                if (missing.Count > 0)
                {
                    string responseStr;

                    if (async)
                        responseStr = response.GetResponseTextStream(address).Result;
                    else
                        responseStr = response.GetResponseText(ref address);

                    var xDoc = XDocument.Parse(responseStr);

                    var toRemove = xDoc.Descendants("item").Descendants().Where(
                        e =>
                        {
                            var str = e.Name.ToString();

                            if (str.EndsWith("_raw"))
                                str = str.Substring(0, str.Length - "_raw".Length);

                            return missing.Contains(str);
                        }).ToList();

                    foreach (var elm in toRemove)
                    {
                        elm.Remove();
                    }

                    return new BasicResponse(xDoc.ToString());
                }
            }

            return response;
        }

        private int GetCount(NameValueCollection components, Content? content)
        {
            var count = 2;

            if (FixedCountOverride != null)
                return FixedCountOverride.Value;

            if (CountOverride == null) //question: will this cause issues with streaming cos the second page have the wrong count
            {
                var countStr = components["count"];

                if (countStr != null && countStr != "0" && countStr != "500" && countStr != "*")
                    count = Convert.ToInt32(countStr);

                if (components["filter_objid"] != null)
                {
                    count = 1;

                    var values = components.GetValues("filter_objid");

                    if (values?.Length > 1)
                        count = 2;
                    else
                    {

                        if (values?.First() == "-2")
                        {
                            if (content != Content.Devices)
                                count = 0;
                        }
                        if (values?.First() == "-3")
                        {
                            if (content != Content.Groups)
                                count = 0;
                        }
                        else if (values?.First() == "-4")
                            count = 0;
                    }
                }
            }
            else
            {
                if (content != null && CountOverride.ContainsKey(content.Value))
                    count = CountOverride[content.Value];
            }

            return count;
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            var objectType = components["objecttype"].ToEnum<ObjectType>();

            switch (objectType)
            {
                case ObjectType.Sensor:
                    return new SensorSettingsResponse();
                case ObjectType.Device:
                    return new DeviceSettingsResponse();
                case ObjectType.Notification:
                    return new NotificationActionResponse(new NotificationActionItem());
                case ObjectType.Schedule:
                    return new ScheduleResponse();
                default:
                    throw new NotImplementedException($"Unknown object type '{objectType}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebResponse GetRawObjectProperty(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            if (components["name"] == "name")
                return new RawPropertyResponse("testName");

            if (components["name"] == "active")
                return new RawPropertyResponse("1");

            if (components["name"] == "tags")
                return new RawPropertyResponse("tag1 tag2");

            if (components["name"] == "accessgroup")
                return new RawPropertyResponse("1");

            if (components["name"] == "intervalgroup")
                return new RawPropertyResponse(null);

            if (components["name"] == "comments")
                return new RawPropertyResponse("Do not turn off!");

            if (components["name"] == "banana")
                return new RawPropertyResponse("(Property not found)");

            components.Remove("username");
            components.Remove("passhash");
            components.Remove("id");

            throw new NotImplementedException($"Unknown raw object property '{components[0]}' passed to {GetType().Name}");
        }

        private IWebResponse GetSensorTargetResponse()
        {
            switch (newSensorType)
            {
                case SensorType.ExeXml:
                    return new ExeFileTargetResponse();
                case SensorType.WmiService:
                    return new WmiServiceTargetResponse();
                case SensorType.Http:
                    return new HttpTargetResponse();
                default:
                    throw new NotSupportedException($"Sensor type {newSensorType} not supported");
            }
        }

        public static Content GetContent(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            return content;
        }

        protected T[] GetItems<T>(Func<int, T> func, int count)
        {
            return Enumerable.Range(0, count).Select(func).ToArray();
        }

        public static string GetFunction(string address)
        {
            var page = GetPage(address);

            XmlFunction xmlFunc;
            if (TryParseEnumDescription(page, out xmlFunc))
                return xmlFunc.ToString();

            CommandFunction cmdFunc;
            if (TryParseEnumDescription(page, out cmdFunc))
                return cmdFunc.ToString();

            JsonFunction jsonFunc;
            if (TryParseEnumDescription(page, out jsonFunc))
                return jsonFunc.ToString();

            HtmlFunction htmlFunc;
            if (TryParseEnumDescription(page, out htmlFunc))
                return htmlFunc.ToString();

            throw new NotImplementedException($"Don't know what the type of function '{page}' is");
        }

        private static string GetPage(string address)
        {
            var queries = UrlHelpers.ParseQueryString(address);

            var items = queries.AllKeys.SelectMany(queries.GetValues, (k, v) => new { Key = k, Value = v });

            var first = items.First().Key;

            var query = first.LastIndexOf('?');

            var end = query > 0 ? query : first.Length - 1;
            var start = first.IndexOf('/', 9) + 1;

            var page = first.Substring(start, end - start);

            if (page.StartsWith("api/"))
                page = page.Substring(4);

            return page;
        }

        private static bool TryParseEnumDescription<TEnum>(string description, out TEnum result)
        {
            result = default(TEnum);

            foreach (var field in typeof(TEnum).GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                if (attribute != null)
                {
                    if (attribute.Description.ToLower() == description.ToLower())
                    {
                        result = (TEnum)field.GetValue(null);

                        return true;
                    }
                }
            }

            return false;
        }

        protected Exception GetUnknownFunctionException(string function, bool async = false)
        {
            return new NotImplementedException($"Unknown {(async ? "async " : "")}function '{function}' passed to {GetType().Name}");
        }
    }
}
