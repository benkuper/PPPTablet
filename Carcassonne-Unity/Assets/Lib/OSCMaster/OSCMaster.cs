using UnityEngine;
using System.Collections;
using UnityOSC;
using System.Net;

public class OSCMaster : MonoBehaviour {

    public static OSCMaster instance;

    OSCThreadServer server;
    OSCClient client;
    OSCClient broadcastClient;
    OSCClient scoreClient;

    public int port = 6000;
    public int remotePort = 6001;
    public int scorePort = 6950;

    public bool debugMessage;

    OSCControllable[] controllables;
    
    
	// Use this for initialization
	void Awake () {
        instance = this;

        server = new OSCThreadServer(port);
        server.PacketReceivedEvent += packetReceived;
        server.Connect();

        if (MainConfig.instance != null) setupClient();

        checkControllables();
	}

    public void setupClient()
    {
        if (client != null)
        {
            client.Close();
        }

        if(OSCMaster.instance != null)
        {
            IPAddress ip = IPAddress.Loopback;

#if !UNITY_EDITOR
            ip = IPAddress.Parse(MainConfig.getRemoteIP());    
#endif

            client = new OSCClient(ip, remotePort);
            broadcastClient = new OSCClient(ip, port);
            scoreClient = new OSCClient(ip, scorePort);

            client.Connect();
            broadcastClient.Connect();
            scoreClient.Connect();

            Debug.Log("OSCMaster is now sending to : " + client.ClientIPAddress + ":" + client.Port);

        }
    }

    void packetReceived(OSCPacket p)
    {
        if (debugMessage) Debug.Log("Message receive : " + p.Address);

        OSCMessage m = (OSCMessage)p;

        if (TabletIDManager.instance.isAdmin)
        {
            if (debugMessage) Debug.Log("Tablet is admin, not parsing message");
            return;
        }

        string[] addSplit = m.Address.Split(new char[] { '/' });

        if (addSplit.Length < 4) return;

        string tabIDString = addSplit[1].ToLower();


        if (!tabIDString.Contains("all"))
        {
            if (tabIDString.StartsWith("tab"))
            {
                tabIDString = tabIDString.Substring(3); //remove "tab"
                int tabID = -1;
                int.TryParse(tabIDString, out tabID);

                Debug.Log("Target tablet id " + tabID);

                if (tabID != TabletIDManager.getTabletID())
                {
                    if (debugMessage) Debug.Log("Not same id (local) " + TabletIDManager.getTabletID() + " <> (target) " + tabID);
                    return;
                }
            }
        }

        string target = addSplit[2];
        string property = addSplit[3];

        if(debugMessage) Debug.Log("Message received for Target : " + target + ", property = " + property);

        OSCControllable c = getControllableForID(target);

        if (c == null)
        {
            if(debugMessage) Debug.Log("Controllable is null, refresh controllables");
           //If not found, update once controllable list, if not found then stop
           checkControllables();
           c = getControllableForID(target);
            if (c == null)
            {
                if (debugMessage) Debug.Log("Controllable still null, stopping.");
                return;
            }
        }

        c.setProp(property, m.Data);
    }

    OSCControllable getControllableForID(string id)
    {
        foreach(OSCControllable c in controllables)
        {
            if (c.oscName == id) return c;
        }
        return null;
    }

    void checkControllables()
    {
        controllables = FindObjectsOfType<OSCControllable>();
        foreach (OSCControllable c in controllables)
        {
            Debug.Log("Add controllable : " + c.oscName);
        }
    }
	
	// Update is called once per frame
	void Update () {
        server.Update();
	}


    void OnDestroy()
    {
        server.Close();
    }

    public static void sendMessage(OSCMessage m)
    {
        instance.client.Send(m);
    }

    public static void sendMessageToOtherTablets(OSCMessage m)
    {
        instance.broadcastClient.Send(m);
    }

    public static void sendScoreMessage(OSCMessage m)
    {
        instance.scoreClient.Send(m);
    }

    public static string getBaseAddress()
    {
        return "/tab" + TabletIDManager.getTabletID()+"/";
    }
}
