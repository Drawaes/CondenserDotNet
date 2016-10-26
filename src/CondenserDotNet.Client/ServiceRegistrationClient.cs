using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CondenserDotNet.Client.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CondenserDotNet.Client
{
    public class ServiceRegistrationClient : ClientBase
    {
        private string _serviceName;
        private string _serviceId;
        private string _address;
        private int _port = -1;
        private List<Version> _versions = new List<Version>();
        private List<string> _urls = new List<string>();
        private bool _started = false;
        private bool _hasCheck = false;
        private string _checkRelativePath;
        private int _checkTimespanSeconds;
        private int _checkTimeToLiveSeconds;
        private bool _hasSwagger = false;
        private Service _service = new Service();
        private string _leaderKey;
        private Leader _leader;
        
        public Leader Leader => _leader;

        #region Service Setup
        public ServiceRegistrationClient Config(string serviceName = null, string serviceId = null, int port = -1, string address = null)
        {
            if (_started)
            {
                throw new InvalidOperationException("The service has already started, you cannot change the settings you need to use a new service registration");
            }
            CheckDisposed();
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                _serviceName = serviceName;
            }
            if (!string.IsNullOrWhiteSpace(serviceId))
            {
                _serviceId = serviceId;
            }
            if (port != -1)
            {
                _port = port;
            }
            if (!string.IsNullOrWhiteSpace(address))
            {
                _address = address;
            }
            return this;
        }

        public ServiceRegistrationClient AddUrls(params string[] url)
        {
            if (_started)
            {
                throw new InvalidOperationException("The service has already started, you cannot change the settings you need to use a new service registration");
            }
            CheckDisposed();
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url), "You can't add null urls");
            }
            foreach (var u in url)
            {
                if (string.IsNullOrWhiteSpace(u))
                {
                    throw new ArgumentNullException(nameof(url), "You can't add empty urls");
                }
                if (!_urls.Contains(u))
                {
                    _urls.Add(u);
                }
            }
            return this;
        }

        public ServiceRegistrationClient AddSupportedVersions(params Version[] versions)
        {
            if (_started)
            {
                throw new InvalidOperationException("The service has already started, you cannot change the settings you need to use a new service registration");
            }
            CheckDisposed();
            if (versions == null)
            {
                throw new ArgumentNullException(nameof(versions), "You can't add null urls");
            }
            foreach (var v in versions)
            {
                if (!_versions.Contains(v))
                {
                    _versions.Add(v);
                }
            }
            return this;
        }

        public ServiceRegistrationClient AddHealthCheck(string checkRelativePath, int timespanSeconds, int timeToLiveSeconds)
        {
            if (_started)
            {
                throw new InvalidOperationException("The service has already started, you cannot change the settings you need to use a new service registration");
            }

            if (_hasCheck)
            {
                throw new InvalidOperationException("There is already a health check setup for this service");
            }
            _hasCheck = true;
            _checkRelativePath = checkRelativePath;
            _checkTimespanSeconds = timespanSeconds;
            _checkTimeToLiveSeconds = timeToLiveSeconds;

            return this;
        }

        public ServiceRegistrationClient AddSwagger()
        {
            _hasSwagger = true;
            return this;
        }

        public ServiceRegistrationClient AddLeaderElectionKey(string keyPath)
        {
            if(_started)
            {
                throw new InvalidOperationException("The service has already started, you cannot change the settings you need to use a new service registration");
            }
            _leaderKey = keyPath;

            return this;
        }
        #endregion
        
        public async Task RegisterServiceAsync()
        {
            if (string.IsNullOrWhiteSpace(_serviceName))
            {
                throw new ArgumentNullException(nameof(_serviceName), "You are required to have a service name to startup");
            }
            if (_port == -1)
            {
                throw new ArgumentNullException(nameof(_port), "You need to put a port in for the service");
            }
            if (string.IsNullOrWhiteSpace(_serviceId))
            {
                _serviceId = $"{_serviceName}{Dns.GetHostName()}";
            }
            if (string.IsNullOrWhiteSpace(_address))

            {
                _address = Dns.GetHostName();
            }

            _service.Address = _address;
            _service.EnableTagOverride = false;
            _service.ID = _serviceId;
            _service.Name = _serviceName;
            _service.Port = _port;
            _service.Tags = Enumerable.Concat(_versions.Select(v => $"version={v.ToString()}"), _urls.Select(u => $"url={u}")).Concat(Enumerable.Repeat($"swagger={_hasSwagger}",1)).ToArray();
            if (_hasCheck)
            {
                _service.Check = new HttpCheck()
                {
                    HTTP = $"http://{_address}:{_port}/{_checkRelativePath}",
                    Interval = $"{_checkTimespanSeconds}s",
                };
            }
            var content = new StringContent(JsonConvert.SerializeObject(_service, _jsonSettings), System.Text.Encoding.UTF8, "application/json");

            var registerResult = await _httpClient.PutAsync("/v1/agent/service/register", content);

            if (registerResult.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException("Unable to register the service with consul");
            }

            if (!string.IsNullOrEmpty(_leaderKey))
            {
                _leader = new Leader(_leaderKey,_serviceId);
            }

            _started = true;
        }
    }
}
