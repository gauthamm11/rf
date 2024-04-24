using ClientInfo.ClientData;
using Philips.PmsCT.Base.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UsageAnalyticsDataAccess;
using UsageAnalyticsService.Constants;
using UsageAnalyticsService.Interfaces;
using UsageAnalyticsCommon.UsageAnalyticsCommonTypes;
using UsageAnalyticsUtilities;

namespace UsageAnalyticsService.Providers
{
    /// <summary>
    /// Class ClientParameterInsert
    /// </summary>
    public class ClientParameterInformation : IClientParameterInformation, IDisposable
    {
        /// <summary>
        /// The dashboard entities
        /// </summary>
        UsageAnalyticsEntities DashboardEntities;
        List<GetMaintenanceSession_Result> values ;
        Dictionary<string, List<int>> forModelConstruction ;
        Dictionary<string, List<double>> finalModel ;
        List<int> maxUsers ;

        /// <summary>
        /// Inserts the client parameter meta data.
        /// </summary>
        /// <param name="clientParameters">The client parameters.</param>
        public void InsertClientParameterMetaData(IEnumerable<ClientParameterMetaData> clientParameters)
        {
            try
            {
                 DashboardEntities=new UsageAnalyticsEntities();
                 string strXML = XMLHandler.ToXml(clientParameters);
                 DashboardEntities.InsertClientParameters(strXML);
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "ClientParameterMetaDataInsert");
            }
        }

        /// <summary>
        /// Inserts the client parameter details.
        /// </summary>
        /// <param name="clientParameterDetails">The client parameter details.</param>
        public void InsertClientParameterDetails(IEnumerable<ClientParameterDetails> clientParameterDetails)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                DashboardEntities.InsertMachineDetails(clientParameterDetails.FirstOrDefault().ClientMachineName, "");
                DashboardEntities.InsertClientTypeDetails(clientParameterDetails.FirstOrDefault().ClientMachineName, clientParameterDetails.FirstOrDefault(p => p.ClientParameterID == ServiceConstants.ClientUserName).ClientParameterValue, clientParameterDetails.FirstOrDefault(p => p.ClientParameterID == ServiceConstants.ClientDomainName).ClientParameterValue);
                string strXML = XMLHandler.ToXml(clientParameterDetails);
                DashboardEntities.InsertClientDetails(strXML);
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "ClientParameterDetailsInsert");
            }            
        }

        /// <summary>
        /// Gets the client parameter meta data.
        /// </summary>
        /// <returns>IEnumerable<ClientParameterMetaData></returns>
        public IEnumerable<ClientParameterMetaData> GetClientParameterMetaData()
        {
            IEnumerable<ClientParameterMetaData> clientParameters = null;
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientParams= DashboardEntities.GetClientParameterMetaData();
                clientParameters = clientParams.AsEnumerable().Select(c => new ClientParameterMetaData
                {
                    ClientParameterID = c.ClientParameterID,
                    ClientParameterName = c.ClientParameterName
                }).ToList();
                
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetClientParameterMetaData");
            }
            return clientParameters;
        }



        /// <summary>
        /// Gets the off line users data.
        /// </summary>
        /// <returns> IEnumerable<OffLineUsers></returns>
        public IEnumerable<OffLineUsers> GetOffLineUsersData()
        {
            IEnumerable<OffLineUsers> clientParameters = null;
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientParams = DashboardEntities.GetOffLineUsers();
                if (clientParams != null)
                {
                    clientParameters = clientParams.AsEnumerable().Select(c => new OffLineUsers
                    {
                        UserName = c.UserName,
                        DomainName = c.DomainName,
                        MachineName = c.MachineName,
                        LastLogOut = c.EndTime,
                        SessionDuration = c.SessionDuration
                    }).ToList();
                }

            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetOffLineUsersData");
            }
            return clientParameters;
        }


        public IEnumerable<UsageAnalyticsDataAccess.MaintenanceMessage> GetMessageDetails()
        {
            IEnumerable<UsageAnalyticsDataAccess.MaintenanceMessage> MessageDetails = null;
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                MessageDetails = DashboardEntities.MaintenanceMessages.ToList();

            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetOffLineUsersData");
            }
            return MessageDetails;

        }


        /// <summary>
        /// Gets the machine parameter details.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <returns>IEnumerable<ClientParameterDetails></returns>
        public IEnumerable<ClientParameterDetails> GetMachineParameterDetails(string machineName)
        {
            UsageAnalyticsEntities DashboardEntities =null;
            List<ClientParameterDetails> clientParameterDetails = new List<ClientParameterDetails>();
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientdetails = DashboardEntities.GetMachineDetails(machineName);
                if (clientdetails != null)
                {
                    //Log.Write(LogClass.General, LogLevel.Event,clientdetails.Count().ToString(), "GetMachineParameterDetails");
                    foreach (var item in clientdetails)
                    {
                        ClientParameterDetails clientParam = new ClientParameterDetails(item.ClientParameterID, machineName, item.ParameterValue, item.LastUpdatedTime.ToString());
                        clientParameterDetails.Add(clientParam);
                    }
                    clientdetails = null;
                    //clientParameterDetails = clientdetails.AsEnumerable().Select(c => new ClientParameterDetails
                    //{
                    //    ClientMachineName = machineName,
                    //    ClientParameterID = c.ClientParameterID,
                    //    ClientValueDateTime = c.LastUpdatedTime.ToString(),
                    //    ClientParameterValue = c.ParameterValue
                    //}).ToList();
                }
                else
                {
                    Log.Write(LogClass.General, LogLevel.Error, "Client Detials is null", "GetMachineParameterDetails");
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetMachineParameterDetails");
                Log.Write(LogClass.General, LogLevel.Error, ex.InnerException.Message, "GetMachineParameterDetails");
            }
            finally
            {
                DashboardEntities = null;
            }
            return clientParameterDetails;
        }


        /// <summary>
        /// Gets the user session details.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>IEnumerable<ClientInfo.ClientData.ClientSession></returns>
        public IEnumerable<ClientInfo.ClientData.ClientSession> GetUserSessionDetails(string userName,string machineName,DateTime startTime,DateTime endTime)
        {
            IEnumerable<ClientInfo.ClientData.ClientSession> clientSessionDetails = null;
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientsessions = DashboardEntities.GetUserSession(userName,machineName,startTime,endTime);
                if (clientsessions != null)
                {
                    clientSessionDetails = clientsessions.AsEnumerable().Select(c => new ClientInfo.ClientData.ClientSession
                    {
                        ClientUserName = userName,
                        ClientMachineName = string.Empty,
                        ClientSessionEndTime = c.EndTime.ToString(),
                        ClientSessionDuration = c.SessionDuration
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetUserSessionDetails");
            }
            return clientSessionDetails;
        }
        
        /// <summary>
        /// Saves the maintenance message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SaveMaintenanceMessage(UsageAnalyticsCommon.UsageAnalyticsCommonTypes.MaintenanceMessage message)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                int PollingInterval = 60;
                if (DashboardEntities.MaintenanceMessages.FirstOrDefault() != null)
                    PollingInterval = (int)(DashboardEntities.MaintenanceMessages.FirstOrDefault().PollingFrequency == null ? 60 : DashboardEntities.MaintenanceMessages.FirstOrDefault().PollingFrequency);
                DashboardEntities.InsertMaintenanceMessage(message.StartTime, message.EndTime, message.MaintenanceMessageString, message.SystemLock, message.IsEnabled, message.UserName);

                if (DashboardEntities.MaintenanceMessages.FirstOrDefault() != null)
                {
                    UsageAnalyticsDataAccess.MaintenanceMessage msg = DashboardEntities.MaintenanceMessages.FirstOrDefault();
                    msg.PollingState = msg.IsEnabled== true?true:false;
                    msg.PollingFrequency = PollingInterval;
                    msg.LogTime = DateTime.Now;
                    DashboardEntities.SaveChanges();


                   if(msg.PollingState==false)
                   {
                       UsageAnalyticsServiceInstaller.PollingStatus(false, PollingInterval);
                   }
                }
            }
            catch (Exception ex)
            {
               Log.Write(LogClass.General, LogLevel.Error, ex.Message, "SaveMaintenanceMessage");
            }
        }
         
        /// <summary>
        /// Sets the client location.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <param name="location">The location.</param>
        public void SetClientLocation(string machineName, string location)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                DashboardEntities.SetMachineLocation(machineName, location);
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "SetClientLocation");
            }
        }

        /// <summary>
        /// Gets the client location.
        /// </summary>
        /// <param name="machineName">Name of the machine.</param>
        /// <returns>
        /// client location string
        /// </returns>
        public string GetClientLocation(string machineName)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var location=DashboardEntities.GetClientMachineLocation(machineName);
                return location.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetClientLocation");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the maintenance availability.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns>Dictionary<string, IEnumerable<double>>Day,Availability Percentage</returns>
        public Dictionary<string, List<double>> GetMaintenanceAvailability(DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                values = DashboardEntities.GetMaintenanceSession(startDateTime,endDateTime).ToList();
                forModelConstruction = new Dictionary<string, List<int>>();
                finalModel = new Dictionary<string, List<double>>();
                maxUsers = new List<int>();
                GetAverageSessionCount(startDateTime,endDateTime,"Monday");
                GetAverageSessionCount(startDateTime, endDateTime, "Tuesday");
                GetAverageSessionCount(startDateTime, endDateTime, "Wednesday");
                GetAverageSessionCount(startDateTime, endDateTime, "Thursday");
                GetAverageSessionCount(startDateTime, endDateTime, "Friday");
                GetAverageSessionCount(startDateTime, endDateTime, "Saturday");
                GetAverageSessionCount(startDateTime, endDateTime, "Sunday");
                int maximumUsers = maxUsers.Max();
                Log.Write(LogClass.General, LogLevel.Verbose, maximumUsers.ToString(), "Calculation");
                foreach (var item in forModelConstruction)
                {
                    List<int> average = item.Value;
                    List<double> percentage = new List<double>();
                    foreach (var value in average)
                    {
                        Log.Write(LogClass.General, LogLevel.Verbose, item.Key + value, "Calculation");
                        percentage.Add((value * 100) / maximumUsers);
                    }
                    finalModel.Add(item.Key, percentage);
                }
            }
            catch(Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetMaintenanceAvailability");
                return null;
            }
            return finalModel;
        }

        /// <summary>
        /// Gets the average session count.
        /// Assuming Session duration does not exceed one day
        /// </summary>
        /// <param name="dayString">The day string.</param>
        private void GetAverageSessionCount(DateTime startDate,DateTime endDate,string dayString)
        {
            List<int> timeSpanCount = new List<int>();
            //List<double> timeSpanAverage = new List<double>();
            int count = 0;
            //int dayCount = 0;
            // get unique day count. say, 3 Mondays for past one month.
            // Number of days shall be considered upon user login-logout
            //dayCount = GetDayCount(startDate,endDate,dayString);
            for (int timespanStart = 0; timespanStart <= 21; timespanStart = timespanStart + 3)
            {
                timeSpanCount.Add(values.Where(a => a.DayString == dayString && (a.EndHours >= new TimeSpan(timespanStart, 0, 0) && a.EndHours <= new TimeSpan(timespanStart + 2, 59, 59))).Count());
                //list of days  say, Mon/Tue between timespan
                var valueMondays = values.Where(a => a.DayString == dayString && (a.EndHours >= new TimeSpan(timespanStart, 0, 0) && a.EndHours <= new TimeSpan(timespanStart + 2, 59, 59))).ToList();
                foreach (var item in valueMondays)
                {
                    if (item.SessionDuration.Contains(':'))
                    {
                        string[] split = item.SessionDuration.Split(new char[] { '.', ':' });
                        string d = split[0];
                        string h = split[1];
                        string m = split[2];
                        string s = split[3];
                        //int day = Convert.ToInt32(d);
                        int hours = Convert.ToInt32(h);
                        //int minutes = Convert.ToInt32(m);
                        //int seconds = Convert.ToInt32(s);

                        if (hours >= 3 && hours < 6)
                        {
                            if (count - 1 >= 0)
                                timeSpanCount[count - 1]++;
                        }
                        else if (hours >= 6 && hours < 9)
                        {
                            if (count - 2 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                            }
                        }
                        else if (hours >= 9 && hours < 12)
                        {
                            if (count - 3 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                                timeSpanCount[count - 3]++;
                            }
                        }
                        else if (hours >= 12 && hours < 15)
                        {
                            if (count - 4 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                                timeSpanCount[count - 3]++;
                                timeSpanCount[count - 4]++;
                            }
                        }
                        else if (hours >= 15 && hours < 18)
                        {
                            if (count - 5 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                                timeSpanCount[count - 3]++;
                                timeSpanCount[count - 4]++;
                                timeSpanCount[count - 5]++;
                            }

                        }
                        else if (hours >= 18 && hours < 21)
                        {
                            if (count - 6 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                                timeSpanCount[count - 3]++;
                                timeSpanCount[count - 4]++;
                                timeSpanCount[count - 5]++;
                                timeSpanCount[count - 6]++;
                            }
                        }
                        else if (hours >= 21 && hours < 24)
                        {
                            if (count - 7 >= 0)
                            {
                                timeSpanCount[count - 1]++;
                                timeSpanCount[count - 2]++;
                                timeSpanCount[count - 3]++;
                                timeSpanCount[count - 4]++;
                                timeSpanCount[count - 5]++;
                                timeSpanCount[count - 6]++;
                                timeSpanCount[count - 7]++;
                            }
                        }
                    }
                }
                count++;
            }
            Log.Write(LogClass.General, LogLevel.Verbose, dayString + timeSpanCount.Max(), "Calculation");
            maxUsers.Add(timeSpanCount.Max());
            //if (dayCount != 0)
            //{
            //    foreach (var item in timeSpanCount)
            //    {
            //        Log.Write(LogClass.General,LogLevel.Event,dayString + item.ToString() + dayCount,"Calculation");
            //        timeSpanAverage.Add((double)item / dayCount);
            //    }
            //}
            //else
            //{
            //    foreach (var item in timeSpanCount)
            //    {
            //        timeSpanAverage.Add(0);
            //    }
            //}
            forModelConstruction.Add(dayString, timeSpanCount);

            Log.Write(LogClass.General, LogLevel.Verbose, "Average Mainteanace Calculation for " + dayString , "GetAverageSessionCount");
        }

        /// <summary>
        /// Gets the day count.
        /// </summary>
        /// <param name="dateTime1">The date time1.</param>
        /// <param name="dateTime2">The date time2.</param>
        /// <param name="dayString">The day string.</param>
        /// <returns>Number of given day count</returns>
        //private int GetDayCount(DateTime dateTime1, DateTime dateTime2, string dayString)
        //{
        //    Log.Write(LogClass.General, LogLevel.Event, dayString + dateTime1+dateTime2, "Day Count");
        //    int day = 0;
        //    for (DateTime i = dateTime1; i <= dateTime2; i = i.AddDays(1))
        //    {
        //        if (i.DayOfWeek.ToString() == dayString)
        //        {
        //            day++;
        //        }
        //    }
        //    Log.Write(LogClass.General, LogLevel.Event, dayString + dateTime1 + dateTime2 + day, "Day Count");
        //    return day;
        //}

        #region IDisposable Members
        private bool Disposed { get; set; }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                if (DashboardEntities != null)
                    DashboardEntities.Dispose();

            }

            Disposed = true;
        }

        #endregion IDisposable Members

    }
}