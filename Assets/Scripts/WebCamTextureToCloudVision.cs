using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WebCamTextureToCloudVision : MonoBehaviour {

	public string url = "https://vision.googleapis.com/v1/images:annotate?key=";
	public string apiKey = "";
	public float captureIntervalSeconds = 2.0f;
	public int requestedWidth = 640;
	public int requestedHeight = 480;
	public FeatureType featureType = FeatureType.TEXT_DETECTION;
	public int maxResults = 10;
    public TextMeshPro question;
    
	WebCamTexture webcamTexture;
	Texture2D texture2D;
	Dictionary<string, string> headers;

	[System.Serializable]
	public class AnnotateImageRequests {
		public List<AnnotateImageRequest> requests;
	}

	[System.Serializable]
	public class AnnotateImageRequest {
		public Image image;
		public List<Feature> features;
	}

	[System.Serializable]
	public class Image {
		public string content;
	}

	[System.Serializable]
	public class Feature {
		public string type;
		public int maxResults;
	}

	[System.Serializable]
	public class ImageContext {
		public LatLongRect latLongRect;
		public List<string> languageHints;
	}

	[System.Serializable]
	public class LatLongRect {
		public LatLng minLatLng;
		public LatLng maxLatLng;
	}

	[System.Serializable]
	public class AnnotateImageResponses {
		public List<AnnotateImageResponse> responses;
	}

	[System.Serializable]
	public class AnnotateImageResponse {
		public List<FaceAnnotation> faceAnnotations;
		public List<EntityAnnotation> landmarkAnnotations;
		public List<EntityAnnotation> logoAnnotations;
		public List<EntityAnnotation> labelAnnotations;
		public List<EntityAnnotation> textAnnotations;
	}

	[System.Serializable]
	public class FaceAnnotation {
		public BoundingPoly boundingPoly;
		public BoundingPoly fdBoundingPoly;
		public List<Landmark> landmarks;
		public float rollAngle;
		public float panAngle;
		public float tiltAngle;
		public float detectionConfidence;
		public float landmarkingConfidence;
		public string joyLikelihood;
		public string sorrowLikelihood;
		public string angerLikelihood;
		public string surpriseLikelihood;
		public string underExposedLikelihood;
		public string blurredLikelihood;
		public string headwearLikelihood;
	}

	[System.Serializable]
	public class EntityAnnotation {
		public string mid;
		public string locale;
		public string description;
		public float score;
		public float confidence;
		public float topicality;
		public BoundingPoly boundingPoly;
		public List<LocationInfo> locations;
		public List<Property> properties;
	}

	[System.Serializable]
	public class BoundingPoly {
		public List<Vertex> vertices;
	}

	[System.Serializable]
	public class Landmark {
		public string type;
		public Position position;
	}

	[System.Serializable]
	public class Position {
		public float x;
		public float y;
		public float z;
	}

	[System.Serializable]
	public class Vertex {
		public float x;
		public float y;
	}

	[System.Serializable]
	public class LocationInfo {
		LatLng latLng;
	}

	[System.Serializable]
	public class LatLng {
		float latitude;
		float longitude;
	}

	[System.Serializable]
	public class Property {
		string name;
		string value;
	}

	public enum FeatureType {
		TYPE_UNSPECIFIED,
		FACE_DETECTION,
        LANDMARK_DETECTION,
        LOGO_DETECTION,
        LABEL_DETECTION,
        TEXT_DETECTION,
        SAFE_SEARCH_DETECTION,
		IMAGE_PROPERTIES
	}

	public enum LandmarkType {
		UNKNOWN_LANDMARK,
		LEFT_EYE,
		RIGHT_EYE,
		LEFT_OF_LEFT_EYEBROW,
		RIGHT_OF_LEFT_EYEBROW,
		LEFT_OF_RIGHT_EYEBROW,
		RIGHT_OF_RIGHT_EYEBROW,
		MIDPOINT_BETWEEN_EYES,
		NOSE_TIP,
		UPPER_LIP,
		LOWER_LIP,
		MOUTH_LEFT,
		MOUTH_RIGHT,
		MOUTH_CENTER,
		NOSE_BOTTOM_RIGHT,
		NOSE_BOTTOM_LEFT,
		NOSE_BOTTOM_CENTER,
		LEFT_EYE_TOP_BOUNDARY,
		LEFT_EYE_RIGHT_CORNER,
		LEFT_EYE_BOTTOM_BOUNDARY,
		LEFT_EYE_LEFT_CORNER,
		RIGHT_EYE_TOP_BOUNDARY,
		RIGHT_EYE_RIGHT_CORNER,
		RIGHT_EYE_BOTTOM_BOUNDARY,
		RIGHT_EYE_LEFT_CORNER,
		LEFT_EYEBROW_UPPER_MIDPOINT,
		RIGHT_EYEBROW_UPPER_MIDPOINT,
		LEFT_EAR_TRAGION,
		RIGHT_EAR_TRAGION,
		LEFT_EYE_PUPIL,
		RIGHT_EYE_PUPIL,
		FOREHEAD_GLABELLA,
		CHIN_GNATHION,
		CHIN_LEFT_GONION,
		CHIN_RIGHT_GONION
	};

	public enum Likelihood {
		UNKNOWN,
		VERY_UNLIKELY,
		UNLIKELY,
		POSSIBLE,
		LIKELY,
		VERY_LIKELY
	}

	// Use this for initialization
	void Start () {
        // header of www api request
		headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json; charset=UTF-8");

		if (apiKey == null || apiKey == "")
			Debug.LogError("No API key. Please set your API key into the \"Web Cam Texture To Cloud Vision(Script)\" component.");
		
		WebCamDevice[] devices = WebCamTexture.devices;
		for (var i = 0; i < devices.Length; i++) {
			Debug.Log (devices [i].name);
		}
		if (devices.Length > 0) {
			webcamTexture = new WebCamTexture(devices[0].name, requestedWidth, requestedHeight);
			Renderer r = GetComponent<Renderer> ();
			if (r != null) {
				Material m = r.material;
				if (m != null) {
					m.mainTexture = webcamTexture;
				}
			}
            //play webcam
			webcamTexture.Play();
			StartCoroutine("Capture");
		}	
	}
	
	private IEnumerator Capture()
    {
		while (true)
        {
			if (this.apiKey == null)
				yield return null;

			yield return new WaitForSeconds(captureIntervalSeconds);

			Color[] pixels = webcamTexture.GetPixels();
			if (pixels.Length == 0)
				yield return null;
			if (texture2D == null || webcamTexture.width != texture2D.width || webcamTexture.height != texture2D.height)
            {
                //create new texture captured form webcam
				texture2D = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
			}

			texture2D.SetPixels(pixels);

            //convert texture type to jpg
            byte[] jpg = texture2D.EncodeToJPG();
            
            // convert jpg image to string
            string base64 = System.Convert.ToBase64String(jpg);
            // configure json object
			AnnotateImageRequests requests = new AnnotateImageRequests();
			requests.requests = new List<AnnotateImageRequest>();

			AnnotateImageRequest request = new AnnotateImageRequest();
			request.image = new Image();
			request.image.content = base64;
			request.features = new List<Feature>();

			Feature feature = new Feature();
			feature.type = this.featureType.ToString();
			feature.maxResults = this.maxResults;

			request.features.Add(feature); 
		
			requests.requests.Add(request);
            
            //convert rquests object to json data
			string jsonData = JsonUtility.ToJson(requests, false);

            if (jsonData != string.Empty)
            {
                string url = this.url + this.apiKey;

                //convert json data string to bytes
                byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
                using (WWW www = new WWW(url, postData, headers))
                {
                    yield return www;

                    if (string.IsNullOrEmpty(www.error))
                    {
                        string textAnnotation = GetTextAnnotation(www.text);
                        if (textAnnotation != "")
                        {
                            question.text = GetDescription(textAnnotation);
                        }
                    }
                    else
                    {
                        Debug.Log("Error: " + www.error);
                    }
                }
            }
        }
	}

    // get TextAnnotation object as json string
    private string GetTextAnnotation (string json)
    {
        //create JSONObject
       JSONObject jSONObject = new JSONObject(json);

        //check if JSONObject has some Fields or not
        if (jSONObject.GetField("responses")[0].HasField("textAnnotations"))
        {
            JSONObject textAnnotation = jSONObject.GetField("responses")[0].GetField("textAnnotations")[0];
            return textAnnotation.ToString();
        }

        return "";
    }

    // get value of Description field from TextAnnotation object
    private string GetDescription(string textAnnotation)
    {
        Question q = JsonUtility.FromJson<Question>(textAnnotation);
        return q.description;
    }

    public class Question
    {
        public string locale;
        public string description;
        public string boundingPoly;
        public string[] vertices;
    }
}
