using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class TGCConnectionController : MonoBehaviour {
	private TcpClient client; 
  	private Stream stream;
  	private byte[] buffer;
	
	public delegate void UpdateIntValueDelegate(int value);
	public delegate void UpdateFloatValueDelegate(float value);
	
	public event UpdateIntValueDelegate UpdatePoorSignalEvent;
	public event UpdateIntValueDelegate UpdateAttentionEvent;
	public event UpdateIntValueDelegate UpdateMeditationEvent;
	public event UpdateIntValueDelegate UpdateRawdataEvent;
	public event UpdateIntValueDelegate UpdateBlinkEvent;
	
	public event UpdateFloatValueDelegate UpdateDeltaEvent;
	public event UpdateFloatValueDelegate UpdateThetaEvent;
	public event UpdateFloatValueDelegate UpdateLowAlphaEvent;
	public event UpdateFloatValueDelegate UpdateHighAlphaEvent;
	public event UpdateFloatValueDelegate UpdateLowBetaEvent;
	public event UpdateFloatValueDelegate UpdateHighBetaEvent;
	public event UpdateFloatValueDelegate UpdateLowGammaEvent;
	public event UpdateFloatValueDelegate UpdateHighGammaEvent;

    public const float NEUROSKY_REPEAT_RATE = 0.1f;     //This is expressed in seconds
    public const float NEUROSKY_INITIAL_TIME = 0f;

    private bool successfulConnection;

    private MessageController feedbackMessage;

    private float currentTime;
	

	void Start () 
    {
        GameController gameController;
        successfulConnection = true;
        gameController = GameObject.Find(Names.GameController).GetComponent<GameController>();
        feedbackMessage = GameObject.Find(Names.FeedbackMessage).GetComponent<MessageController>();
        
        try
        {
            if(gameController.IsGameNeurosky)   
                Connect();
        }
        catch (SocketException e)
        {
            feedbackMessage.Show(Names.SocketConnectionMessage);
            successfulConnection = false;
            Debug.Log("Problem connecting: " + e.Message);
        }

        currentTime = 0;
	}
	
	public void Disconnect(){
		if(IsInvoking("ParseData")){
			CancelInvoke("ParseData");
			stream.Close();
		}
	}
	
	public void Connect(){
		if(!IsInvoking("ParseData")){
			
			client = new TcpClient("127.0.0.1", 13854);	
		    stream = client.GetStream();
		    buffer = new byte[1024];
		    byte[] myWriteBuffer = Encoding.ASCII.GetBytes(@"{""enableRawOutput"": true, ""format"": ""Json""}");
		    stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);
			
			//Repeating
            InvokeRepeating("ParseData", NEUROSKY_INITIAL_TIME, NEUROSKY_REPEAT_RATE);
		}
	}

    public bool ConnectionWasSuccessful()
    {
        return successfulConnection;
    }

    protected void TEST(int value)
    {
        Debug.Log("weeeeepa");
    }

	void ParseData()
    {
	    if(stream.CanRead){
	      try { 
	        int bytesRead = stream.Read(buffer, 0, buffer.Length);
	
	        string[] packets = Encoding.ASCII.GetString(buffer, 0, bytesRead).Split('\r');

            //Debug.Log(bytesRead);
	        foreach(string packet in packets){
	          if(packet.Length == 0)
	            continue;

              //Debug.Log(packet);
	          IDictionary primary = (IDictionary)JsonConvert.Import(typeof(IDictionary), packet);
	
	          if(primary.Contains("poorSignalLevel")){
						
				if(UpdatePoorSignalEvent != null){
				   UpdatePoorSignalEvent(int.Parse(primary["poorSignalLevel"].ToString()));
				   }
						
	            if(primary.Contains("eSense")){
	              IDictionary eSense = (IDictionary)primary["eSense"];
				  if(UpdateAttentionEvent != null){
                      //Debug.Log(int.Parse(eSense["attention"].ToString()));
					 UpdateAttentionEvent(int.Parse(eSense["attention"].ToString()));
				   }		
				  if(UpdateMeditationEvent != null){
					 UpdateMeditationEvent(int.Parse(eSense["meditation"].ToString()));
				   }
	            }
	
	            if(primary.Contains("eegPower")){
	              IDictionary eegPowers = (IDictionary)primary["eegPower"];
								
				  if(UpdateDeltaEvent != null){
					 UpdateDeltaEvent(float.Parse(eegPowers["delta"].ToString()));			
					}
				  if(UpdateThetaEvent != null){
					 UpdateThetaEvent(float.Parse(eegPowers["theta"].ToString()));			
					}
				  if(UpdateLowAlphaEvent != null){
					 UpdateLowAlphaEvent(float.Parse(eegPowers["lowAlpha"].ToString()));
					}
				  if(UpdateHighAlphaEvent != null){
					 UpdateHighAlphaEvent(float.Parse(eegPowers["highAlpha"].ToString()));
					}
				  if(UpdateLowBetaEvent != null){
					 UpdateLowBetaEvent(float.Parse(eegPowers["lowBeta"].ToString()));
					}
				  if(UpdateHighBetaEvent != null){
					 UpdateHighBetaEvent(float.Parse(eegPowers["highBeta"].ToString()));
					}
				  if(UpdateLowGammaEvent != null){
					 UpdateLowGammaEvent(float.Parse(eegPowers["lowGamma"].ToString()));			
					}
				  if(UpdateHighGammaEvent != null){
					 UpdateHighGammaEvent(float.Parse(eegPowers["highGamma"].ToString()));
					}
	            }
	          }
	          else if(primary.Contains("rawEeg") && UpdateRawdataEvent != null){
					  UpdateRawdataEvent(int.Parse(primary["rawEeg"].ToString()));
	          }
	          else if(primary.Contains("blinkStrength") && UpdateBlinkEvent != null){
					  UpdateBlinkEvent(int.Parse(primary["blinkStrength"].ToString()));
	          }
	        }
	      }
	      catch(IOException e)
          { 
              //Debug.Log("IOException " + e); 
          }
	      catch(System.Exception e)
          { 
              //Debug.Log("Exception " + e); 
          }
	    }

        currentTime += NEUROSKY_REPEAT_RATE;
		
	}// end ParseData

    public float GetCurrentTime()
    {
        return currentTime;
    }
	
	void OnApplicationQuit(){
		Disconnect();
	}
}
