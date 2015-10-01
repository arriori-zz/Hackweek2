using HackTestWPF;
using Server.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Server.HackTestWPF
{
    public class HackExchangeContext
    {
        public string Endpoint { get; set; }
        public ICredentials Credentials { get; set; }
    }

    public class HackExchangeService
    {
        public string Login(NetworkCredential credentials, out HackExchangeContext context)
        {
            var postBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
                "<Autodiscover xmlns=\"http://schemas.microsoft.com/exchange/autodiscover/outlook/requestschema/2006\"> " +
                    "<Request> " +
                        "<EMailAddress>" + credentials.UserName + "</EMailAddress> " +
                        "<AcceptableResponseSchema>http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a</AcceptableResponseSchema> " +
                    "</Request> " +
                "</Autodiscover>";

            var url = "https://autodiscover-s.outlook.com/autodiscover/autodiscover.xml";
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                //var xmlDoc = new XmlDocument();
                //xmlDoc.Load(new StringReader(responseBody));

                //var nodeList = xmlDoc.GetElementsByTagName("DisplayName");
                //if (nodeList.Count > 0)
                //{
                //    var element = nodeList[0] as XmlElement;
                //    if (element != null)
                //    {
                //        Console.WriteLine("DisplayName: " + element.InnerText);
                //    }
                //}
                //nodeList = xmlDoc.GetElementsByTagName("ASUrl");
                //if (nodeList.Count > 0)
                //{
                //    var element = nodeList[0] as XmlElement;
                //    if (element != null)
                //    {
                //        Console.WriteLine("ASUrl: " + element.InnerText);
                //    }
                //}
                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("r", "http://schemas.microsoft.com/exchange/autodiscover/outlook/responseschema/2006a");
                var value = nav.SelectSingleNode("//r:DisplayName", nsManager);
                string displayName = null;
                if (value != null)
                {
                    Console.WriteLine("DisplayName: " + value.Value);
                    displayName = value.Value;
                }
                var urlNode = nav.SelectSingleNode("//r:ASUrl", nsManager);
                string asUrl = null;
                if (urlNode != null)
                {
                    Console.WriteLine("ASUrl: " + urlNode.Value);
                    asUrl = urlNode.Value;
                }
                Console.WriteLine("Status: " + response.StatusCode);

                context = new HackExchangeContext()
                {
                    Credentials = credentials,
                    Endpoint = asUrl
                };

                return displayName;
            }
        }

        public IList<Room> GetRooms(HackExchangeContext context)
        {
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBody = LoadXml("GetRooms");
            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                IList<Room> result = new List<Room>();
                var roomIterator = nav.Select("//t:Persona[t:PersonaType='Room']", nsManager);
                foreach (XPathNavigator roomNav in roomIterator)
                {
                    var displayName = EvaluateXPath(roomNav, nsManager, "./t:DisplayName");
                    var roomId = EvaluateXPath(roomNav, nsManager, "./t:PersonaId/@Id");
                    var location = EvaluateXPath(roomNav, nsManager, "./t:OfficeLocations/t:StringAttributedValue/t:Value");
                    var emailAddress = EvaluateXPath(roomNav, nsManager, "./t:EmailAddress/t:EmailAddress");
                    Room r = new Room()
                    {
                        Name = displayName,
                        Location = location,
                        Id = roomId,
                        EmailAddress = emailAddress
                    };
                    result.Add(r);
                }
                return result;
            }
        }

        public CalendarItem CreateAppointment(HackExchangeContext context, string subject, string description, DateTime startTime, DateTime endTime, Room room, List<string> requiredAttendees)
        {
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBodyTemplate = LoadXml("CreateAppointment");
            var attendeeTemplate = LoadXml("AttendeeTemplate");

            var roomAttendeeBody = string.Format(attendeeTemplate, room.EmailAddress);
            string requiredAttendeesBody = "";
            if (requiredAttendees != null)
            {
                var attendeesBuilder = new StringBuilder();
                foreach (var attendee in requiredAttendees)
                {
                    attendeesBuilder.AppendFormat(attendeeTemplate, attendee);
                    attendeesBuilder.AppendLine();
                }
                requiredAttendeesBody = attendeesBuilder.Length > 0 ? attendeesBuilder.ToString() : "";
            }

            var startTimeBody = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", startTime.ToUniversalTime());
            var endTimeBody = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", endTime.ToUniversalTime());

            var postBody = string.Format(postBodyTemplate, subject, description, startTimeBody, endTimeBody, room.Name, requiredAttendeesBody, roomAttendeeBody);

            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                CalendarItem calendarItem = null;
                var calendarItemNode = nav.SelectSingleNode("//t:CalendarItem/t:ItemId", nsManager);
                if (calendarItemNode != null)
                {
                    var id = EvaluateXPath(calendarItemNode, nsManager, "@Id");
                    var changeKey = EvaluateXPath(calendarItemNode, nsManager, "@ChangeKey");
                    if (id != null && changeKey != null)
                    {
                        calendarItem = new CalendarItem()
                        {
                            Id = id,
                            ChangeKey = changeKey
                        };
                    }
                }
                return calendarItem;
            }
        }

        public bool CancelAppointment(HackExchangeContext context, CalendarItem appointment)
        {
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBodyTemplate = LoadXml("CancelAppointment");
            var postBody = string.Format(postBodyTemplate, appointment.Id, appointment.ChangeKey);
            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                var responseClass = EvaluateXPath(nav, nsManager, "//m:DeleteItemResponseMessage/@ResponseClass");
                return responseClass == "Success";
            }
        }

        public IList<CalendarItem> GetAppointments(HackExchangeContext context)
        {
            var calendarFolder = GetCalendarFolder(context);
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBodyTemplate = LoadXml("GetAppointments");

            var now = DateTime.Now;
            var startTimeBody = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", now.Date.ToUniversalTime());
            var endTimeBody = string.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", now.Date.AddDays(1).ToUniversalTime());
            var postBody = string.Format(postBodyTemplate, startTimeBody, endTimeBody, calendarFolder.Id, calendarFolder.ChangeKey);
            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                var calendarItems = new List<CalendarItem>();
                var calendarItemsIterator = nav.Select("//t:CalendarItem", nsManager);
                foreach (XPathNavigator calendarItemNav in calendarItemsIterator)
                {
                    var id = EvaluateXPath(calendarItemNav, nsManager, "t:ItemId/@Id");
                    var changeKey = EvaluateXPath(calendarItemNav, nsManager, "t:ItemId/@ChangeKey");
                    var subject = EvaluateXPath(calendarItemNav, nsManager, "t:Subject");
                    var location = EvaluateXPath(calendarItemNav, nsManager, "t:Location");

                    if (id != null && changeKey != null)
                    {
                        var calendarItem = new CalendarItem()
                        {
                            Id = id,
                            ChangeKey = changeKey,
                            Subject = subject,
                            Location = location
                        };
                        calendarItems.Add(calendarItem);
                    }
                }
                return calendarItems;
            }
        }

        public CalendarItem GetCalendarFolder(HackExchangeContext context)
        {
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBody = LoadXml("GetCalendarFolder");

            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                CalendarItem calendarFolder = null;
                var folderIdNode = nav.SelectSingleNode("//t:FolderId", nsManager);
                if (folderIdNode != null)
                {
                    var id = EvaluateXPath(folderIdNode, nsManager, "@Id");
                    var changeKey = EvaluateXPath(folderIdNode, nsManager, "@ChangeKey");
                    if (id != null && changeKey != null)
                    {
                        calendarFolder = new CalendarItem()
                        {
                            Id = id,
                            ChangeKey = changeKey
                        };
                    }
                }
                return calendarFolder;
            }
        }

        public RoomAvailability GetRoomAvailability(HackExchangeContext context, string emailAddress, List<string> attendees, int meetingDuration, bool includeSuggestions)
        {
            var url = context.Endpoint;
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var postBodyTemplate = LoadXml("GetRoomAvailability");
            var mailboxTemplate = LoadXml("MailboxDataTemplate");
            var mailboxDataBuilder = new StringBuilder();
            mailboxDataBuilder.AppendFormat(mailboxTemplate, emailAddress);
            if (attendees != null)
            {
                foreach (var attendee in attendees)
                {
                    mailboxDataBuilder.AppendLine();
                    mailboxDataBuilder.AppendFormat(mailboxTemplate, attendee);
                }
            }
            var mailboxData = mailboxDataBuilder.ToString();
            var now = DateTime.Now;
            var startTime = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", now.Date);
            var endTime = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", now.AddDays(1).Date);

            var postBody = string.Format(postBodyTemplate, mailboxData, startTime, endTime, meetingDuration);
            request.Content = new StringContent(postBody, Encoding.UTF8, "text/xml");

            var clientHandler = new HttpClientHandler()
            {
                Credentials = context.Credentials
            };
            using (var client = new HttpClient(clientHandler))
            {
                var response = client.SendAsync(request).Result;
                var responseBody = response.Content.ReadAsStringAsync().Result;

                var doc = new XPathDocument(new StringReader(responseBody));
                var nav = doc.CreateNavigator();
                var nsManager = new XmlNamespaceManager(nav.NameTable);
                nsManager.AddNamespace("m", "http://schemas.microsoft.com/exchange/services/2006/messages");
                nsManager.AddNamespace("t", "http://schemas.microsoft.com/exchange/services/2006/types");

                var roomBusyPeriods = new List<BusyTimeWindow>();
                var attendeesBusyPeriods = new Dictionary<string, IList<BusyTimeWindow>>();
                var suggestedTimes = includeSuggestions ? new List<DateTime>() : null;
                var pos = 0;
                var attendeeCalendarIterator = nav.Select("//m:FreeBusyView//t:CalendarEventArray", nsManager);
                foreach (XPathNavigator attendeeCalendarNav in attendeeCalendarIterator)
                {
                    //pos == 0 means the availability is for the room
                    bool roomAvailability = pos == 0;
                    IList<BusyTimeWindow> busyPeriods;
                    if (roomAvailability)
                    {
                        busyPeriods = roomBusyPeriods;
                    }
                    else
                    {
                        busyPeriods = new List<BusyTimeWindow>();
                        attendeesBusyPeriods[attendees[pos - 1]] = busyPeriods;
                    }
                    var eventIterator = attendeeCalendarNav.Select("./t:CalendarEvent[t:BusyType='Busy']", nsManager);
                    foreach (XPathNavigator eventNav in eventIterator)
                    {
                        var eventStartTimeStr = EvaluateXPath(eventNav, nsManager, "./t:StartTime");
                        var eventEndTimeStr = EvaluateXPath(eventNav, nsManager, "./t:EndTime");
                        if (eventStartTimeStr == null || eventEndTimeStr == null)
                        {
                            continue;
                        }
                        var eventStartTime = DateTime.ParseExact(eventStartTimeStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                        var eventEndTime = DateTime.ParseExact(eventEndTimeStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                        var owner = EvaluateXPath(eventNav, nsManager, "./t:CalendarEventDetails/t:Subject");
                        var location = EvaluateXPath(eventNav, nsManager, "./t:CalendarEventDetails/t:Location");
                        BusyTimeWindow busyWindow = new BusyTimeWindow()
                        {
                            StartTime = eventStartTime,
                            EndTime = eventEndTime,
                            Owner = owner,
                            Location = location
                        };
                        busyPeriods.Add(busyWindow);
                    }
                    pos++;
                }

                if (suggestedTimes != null)
                {
                    var suggestedTimesIterator = nav.Select("//m:SuggestionsResponse//t:Suggestion", nsManager);
                    foreach (XPathNavigator suggestedTimeNav in suggestedTimesIterator)
                    {
                        var meetingTimeStr = EvaluateXPath(suggestedTimeNav, nsManager, "./t:MeetingTime");
                        if (meetingTimeStr != null)
                        {
                            var meetingTime = DateTime.ParseExact(meetingTimeStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                            suggestedTimes.Add(meetingTime);
                        }
                    }
                }

                return new RoomAvailability()
                {
                    BusyPeriods = roomBusyPeriods,
                    AttendeesBusyPeriods = attendeesBusyPeriods,
                    SuggestedTimes = suggestedTimes
                };
            }
        }

        private static string LoadXml(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Server.HackTestWPF.Xmls." + name + ".xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string EvaluateXPath(XPathNavigator nav, XmlNamespaceManager nsManager, string xPathExpression)
        {
            var node = nav.SelectSingleNode(xPathExpression, nsManager);
            return node != null ? node.Value : null;
        }
    }
}
