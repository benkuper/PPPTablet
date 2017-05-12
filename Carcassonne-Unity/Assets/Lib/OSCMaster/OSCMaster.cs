using UnityEngine;
using System.Collections;
using UnityOSC;


public class OSCMaster : MonoBehaviour {

    OSCThreadServer server;
    public int port = 6000;
    public bool debugMessage;

    OSCControllable[] controllables;
    

    
	// Use this for initialization
	void Awake () {
        server = new OSCThreadServer(port);
        server.PacketReceivedEvent += packetReceived;
        server.Connect();

        checkControllables();
	}

    void packetReceived(OSCPacket p)
    {
        if (debugMessage) Debug.Log("Message receive : " + p.Address);

        OSCMessage m = (OSCMessage)p;
        
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
}
