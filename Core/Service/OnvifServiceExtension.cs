namespace Onvif.Core
{
    public static class OnvifServiceExtension
    {

        public static string GetStreamUri(this MediaClient mediaClient, string profileToken, TransportProtocol protocol)
        {
            try
            {
                var setup = new StreamSetup()
                {
                    Transport = new Transport() { Protocol = protocol },
                    Stream = StreamType.RTPUnicast
                };
                return mediaClient.GetStreamUri(setup, profileToken).Uri;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}