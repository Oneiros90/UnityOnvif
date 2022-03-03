using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Onvif.Security
{
    public class SoapSecurityHeaderBehavior : IEndpointBehavior
    {
        readonly string username;
        readonly string password;
        readonly TimeSpan time_shift;

        public SoapSecurityHeaderBehavior(string username, string password) : this(username, password, TimeSpan.FromMilliseconds(0))
        {
        }

        public SoapSecurityHeaderBehavior(string username, string password, TimeSpan timeShift)
        {
            this.username = username;
            this.password = password;
            time_shift = timeShift;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            var headerInspector = new SoapSecurityHeaderInspector(username, password, time_shift);
            try
            {
                clientRuntime.ClientMessageInspectors.Add(headerInspector);
            }
            catch (NotImplementedException)
            {
                var prop = clientRuntime.GetType().GetTypeInfo().GetDeclaredProperty("MessageInspectors");
                var obj = (ICollection<IClientMessageInspector>)prop.GetValue(clientRuntime);
                obj.Add(headerInspector);
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {

        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }
    }
}
