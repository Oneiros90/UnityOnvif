using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Onvif.Security;

namespace Onvif.Core
{

    public class OnvifClient
    {
        private DeviceClient device;
        private Binding binding;
        private SoapSecurityHeaderBehavior securityHeader;

        public Uri OnvifUri { get; private set; }
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public Capabilities Capabilities { get; private set; }

        public OnvifClient(string host, string username, string password)
        {
            Host = host;
            OnvifUri = new Uri($"http://{host}/onvif/device_service");
            Username = username;
            Password = password;
        }

        public async Task<bool> Init()
        {
            try
            {
                binding = CreateBinding();
                var endpoint = new EndpointAddress(OnvifUri);
                device = new DeviceClient(binding, endpoint);
                var timeShift = await GetDeviceTimeShift(device);
                securityHeader = new SoapSecurityHeaderBehavior(Username, Password, timeShift);

                device = new DeviceClient(binding, endpoint);
                device.ChannelFactory.Endpoint.EndpointBehaviors.Clear();
                device.ChannelFactory.Endpoint.EndpointBehaviors.Add(securityHeader);
                device.Open();

                Capabilities = device.GetCapabilities(new CapabilityCategory[] { CapabilityCategory.All });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Ping()
        {
            device.GetCapabilities(new CapabilityCategory[] { CapabilityCategory.All });
        }

        public void Close()
        {
            if (device != null)
                device.Close();
        }

        public async Task<DeviceClient> GetDeviceClient()
        {
            if (device == null)
                await Init();

            return device;
        }

        public async Task<MediaClient> CreateMediaClient()
        {
            if (device == null)
                await Init();

            var media = new MediaClient(binding, new EndpointAddress(new Uri(Capabilities.Media.XAddr)));
            media.ChannelFactory.Endpoint.EndpointBehaviors.Clear();
            media.ChannelFactory.Endpoint.EndpointBehaviors.Add(securityHeader);

            return media;
        }

        public async Task<PTZClient> CreatePTZClient()
        {
            if (device == null)
                await Init();

            var ptz = new PTZClient(binding, new EndpointAddress(new Uri(Capabilities.PTZ.XAddr)));
            ptz.ChannelFactory.Endpoint.EndpointBehaviors.Clear();
            ptz.ChannelFactory.Endpoint.EndpointBehaviors.Add(securityHeader);

            return ptz;
        }

        public async Task<ImagingPortClient> CreateImagingClient()
        {
            if (device == null)
                await Init();

            var imaging = new ImagingPortClient(binding, new EndpointAddress(new Uri(Capabilities.Imaging.XAddr)));
            imaging.ChannelFactory.Endpoint.EndpointBehaviors.Clear();
            imaging.ChannelFactory.Endpoint.EndpointBehaviors.Add(securityHeader);

            return imaging;
        }

        private static Binding CreateBinding()
        {
            var binding = new CustomBinding();
            var textBindingElement = new TextMessageEncodingBindingElement
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.None)
            };
            var httpBindingElement = new HttpTransportBindingElement
            {
                AllowCookies = true,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue
            };

            binding.Elements.Add(textBindingElement);
            binding.Elements.Add(httpBindingElement);

            return binding;
        }

        private static Task<TimeSpan> GetDeviceTimeShift(DeviceClient device)
        {
            return Task.Run(() =>
            {
                var utc = device.GetSystemDateAndTime().UTCDateTime;
                var dt = new System.DateTime(utc.Date.Year, utc.Date.Month, utc.Date.Day,
                                  utc.Time.Hour, utc.Time.Minute, utc.Time.Second);
                return dt - System.DateTime.UtcNow;
            });
        }
    }
}
