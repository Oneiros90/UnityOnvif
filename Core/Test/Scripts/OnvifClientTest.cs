using System.Linq;
using Onvif.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor;
#endif

public class OnvifClientTest : MonoBehaviour
{
    [SerializeField]
    private string host = "192.168.1.1:888";

    [SerializeField]
    private string username = "admin";

    [SerializeField]
    private string password = "";


    private Vector2 ptzPanSpeedLimits;
    private Vector2 ptzTiltSpeedLimits;
    private Vector2 ptzZoomSpeedLimits;


    private OnvifClient onvif;
    private string currentProfile;
    private PTZClient ptzClient;
    private MediaClient mediaClient;

    private async void Start()
    {
        onvif = new OnvifClient(host, username, password);
        var success = await onvif.Init();
        if (!success)
        {
            Debug.LogError($"Cannot connect to {host}");
            return;
        }

        ptzClient = await onvif.CreatePTZClient();
        mediaClient = await onvif.CreateMediaClient();

        var profiles = mediaClient.GetProfiles();
        currentProfile = profiles.First().token;

        var ptzConf = mediaClient.GetProfile(currentProfile).PTZConfiguration;
        Space2DDescription panTiltSpace = null;
        Space1DDescription zoomSpace = null;

        if (!string.IsNullOrWhiteSpace(ptzConf.NodeToken))
        {
            var node = ptzClient.GetNode(ptzConf.NodeToken);

            var cmPanTiltSpace = node.SupportedPTZSpaces.ContinuousPanTiltVelocitySpace.FirstOrDefault();
            if (cmPanTiltSpace != null)
                panTiltSpace = cmPanTiltSpace;

            var cmZoomSpace = node.SupportedPTZSpaces.ContinuousZoomVelocitySpace.FirstOrDefault();
            if (cmZoomSpace != null)
                zoomSpace = cmZoomSpace;
        }
        else if (ptzConf.PanTiltLimits != null && ptzConf.ZoomLimits != null)
        {
            panTiltSpace = ptzConf.PanTiltLimits.Range;
            zoomSpace = ptzConf.ZoomLimits.Range;
        }
        else
        {
            Debug.LogError($"Cannot find a PTZ configuration for device in {host}");
            return;
        }

        ptzPanSpeedLimits.y = panTiltSpace.XRange.Max;
        ptzPanSpeedLimits.x = panTiltSpace.XRange.Min;
        ptzTiltSpeedLimits.y = panTiltSpace.YRange.Max;
        ptzTiltSpeedLimits.x = panTiltSpace.YRange.Min;
        ptzZoomSpeedLimits.y = zoomSpace.XRange.Max;
        ptzZoomSpeedLimits.x = zoomSpace.XRange.Min;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(OnvifClientTest)), CanEditMultipleObjects]
    public class OnvifClientTestEditor : Editor
    {
        private OnvifClientTest Test => target as OnvifClientTest;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();

            // Draw the default inspector
            var defaultInspector = new IMGUIContainer(() =>
            {
                if (target != null)
                    DrawDefaultInspector();
            });
            inspector.Add(defaultInspector);

            // Create a label
            Label label = new Label("Control buttons");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 13;
            inspector.Add(label);

            // Buttons
            inspector.Add(controlButton("Left",     0.0f, 0.5f, 0.5f));
            inspector.Add(controlButton("Right",    1.0f, 0.5f, 0.5f));
            inspector.Add(controlButton("Down",     0.5f, 0.0f, 0.5f));
            inspector.Add(controlButton("Up",       0.5f, 1.0f, 0.5f));
            inspector.Add(controlButton("Zoom In",  0.5f, 0.5f, 0.0f));
            inspector.Add(controlButton("Zoom Out", 0.5f, 0.5f, 1.0f));

            Button controlButton(string name, float panSpeed, float tiltSpeed, float zoomSpeed)
            {
                Button controlButton = new Button() { text = name, };
                controlButton.clicked += () =>
                {
                    Test.ptzClient.ContinuousMove(
                        Test.currentProfile,
                        new PTZSpeed()
                        {
                            PanTilt = new Vector2D()
                            {
                                x = Mathf.Lerp(Test.ptzPanSpeedLimits.x, Test.ptzPanSpeedLimits.y, panSpeed),
                                y = Mathf.Lerp(Test.ptzTiltSpeedLimits.x, Test.ptzTiltSpeedLimits.y, tiltSpeed)
                            },
                            Zoom = new Vector1D()
                            {
                                x = Mathf.Lerp(Test.ptzZoomSpeedLimits.x, Test.ptzZoomSpeedLimits.y, zoomSpeed)
                            }
                        },
                        null
                    );
                };
                controlButton.SetEnabled(Application.isPlaying);
                return controlButton;
            }

            return inspector;
        }
    }
#endif
}
