using System;
using Onvif.Discovery.Models;
using Onvif.Discovery;
using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

public class DiscoveryComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Task.Run(DiscoveryTask);
	}

	static async Task DiscoveryTask()
	{
		Debug.Log("Starting Discover ONVIF cameras for 10 seconds, press Ctrl+C to abort\n");

		var cts = new CancellationTokenSource();
		var discovery = new Discovery();
		await discovery.Discover(10, OnNewDevice, cts.Token);
		Debug.Log("ONVIF Discovery finished");
	}

	private static void OnNewDevice(DiscoveryDevice device)
	{
		// Multiple events could be received at the same time.
		// The lock is here to avoid messing the console.
		lock (Debug.unityLogger)
		{
			Debug.Log($"Device model {device.Model} from manufacturer {device.Mfr} has address {device.Address}");
			Debug.Log($"Urls to device: ");
			foreach (var address in device.XAdresses)
			{
				Debug.Log($"{address}, ");
			}

			Debug.Log("\n");
		}
	}
}
