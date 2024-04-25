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
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace UsageAnalyticsService.Providers
{
    public class ClientParameterInformation : IClientParameterInformation, IDisposable
    {
        private UsageAnalyticsEntities DashboardEntities;
        private MemoryCache _cache;

        public ClientParameterInformation()
        {
            // Initialize MemoryCache for caching
            _cache = MemoryCache.Default;
        }

        public void InsertClientParameterMetaData(IEnumerable<ClientParameterMetaData> clientParameters)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                string strXML = XMLHandler.ToXml(clientParameters);
                DashboardEntities.InsertClientParameters(strXML);
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "ClientParameterMetaDataInsert");
            }
        }

        public IEnumerable<ClientParameterMetaData> GetClientParameterMetaData()
        {
            // Check if data is cached
            var cachedClientParameters = _cache.Get("ClientParameterMetaData") as IEnumerable<ClientParameterMetaData>;
            if (cachedClientParameters != null)
                return cachedClientParameters;

            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientParams = DashboardEntities.GetClientParameterMetaData();
                var clientParameters = clientParams.AsEnumerable().Select(c => new ClientParameterMetaData
                {
                    ClientParameterID = c.ClientParameterID,
                    ClientParameterName = c.ClientParameterName
                }).ToList();

                // Cache the result for future use
                _cache.Set("ClientParameterMetaData", clientParameters, DateTimeOffset.Now.AddMinutes(10)); // Example: Cache for 10 minutes

                return clientParameters;
            }
            catch (Exception ex)
            {
                Log.Write(LogClass.General, LogLevel.Error, ex.Message, "GetClientParameterMetaData");
                return Enumerable.Empty<ClientParameterMetaData>();
            }
        }

        public async Task<IEnumerable<ClientInfo.ClientData.ClientSession>> GetUserSessionDetailsAsync(string userName, string machineName, DateTime startTime, DateTime endTime)
        {
            try
            {
                DashboardEntities = new UsageAnalyticsEntities();
                var clientsessions = await DashboardEntities.GetUserSessionAsync(userName, machineName, startTime, endTime);
                if (clientsessions != null)
                {
                    return clientsessions.AsEnumerable().Select(c => new ClientInfo.ClientData.ClientSession
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
            return Enumerable.Empty<ClientInfo.ClientData.ClientSession>();
        }

        // Other methods...

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
