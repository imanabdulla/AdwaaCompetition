using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WWWAPI : MonoBehaviour
{

    public TextMesh textMesh;

    private QuestionClass question;
    #region Singlton
    private static WWWAPI _api;

    public static WWWAPI API
    {
        get
        {
            return _api;
        }
    }

    private void Awake()
    {
        if (_api == null)
        {
            _api = this;
            DontDestroyOnLoad(API.gameObject);
        }
    }
    #endregion

    private void Start()
    {
        
        ShowQuestion();
    }


    #region WWWClassWithJason
    /* define a delegate named callback*/
    public IEnumerator CreateJsonObject(System.Action<QuestionClass> CallBack, int subjectId, QuestionClass question)
    {
        // add token value to url (header)
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Authorization", "Bearer " + "NDMwMTJmMjItMzRjNi00ZjVkLTg5YWYtNTU3N2M5YTdlZTdi");

        // make api request
        //using (WWW www = new WWW("http://portalapi.aladwaa.com/searchQuestionView.aspx?used=aladwaaapp&text=&userId=&lessonId=-1&unityId=-1&subjectId=" + subjectId, null, headers))
        using (WWW www = new WWW("https://www.aladwaa.com/searchQuestionView.aspx?used=aladwaaapp&text=%D9%85%D8%B5%D8%B1&userId=711555&lessonId=-1&unitId=-1&subjectId=39", null, headers))
        {
            yield return www;
            if (www == null)
            {
                print("Failed to create API Request");
        
                //fire callback event
                CallBack(null);
            }
            else
            {

                textMesh.text = www.text;
                print(www.text);
                //convert www.text string to JSON Object
                //JSONObject JSONObj = new JSONObject(www.text);

                ///*
                // * ha5od el json object da w a search gwah 3la field esmo data
                // * w data da hal2e el value bt3to mn no3 object
                // * w ha5od el object da w a7welo l string
                // */
                //string json = JSONObj.GetField("Data").ToString();

                ////then, cast string to QuestionClass object
                //question = JsonUtility.FromJson<QuestionClass>(json);
                
                //fire callback event
                CallBack(question);
            }
        }
    }

    void AcessData(JSONObject JSONObj)
    {
        switch (JSONObj.type)
        {
            case JSONObject.Type.OBJECT:
                for (int i = 0; i < JSONObj.list.Count; i++)
                {
                    string key = (string)JSONObj.keys[i];
                    JSONObject j = (JSONObject)JSONObj.list[i];
                    Debug.Log(key);
                    AcessData(j);
                }
                break;
            case JSONObject.Type.ARRAY:
                foreach (JSONObject j in JSONObj.list)
                {
                    AcessData(j);
                }
                break;
            case JSONObject.Type.STRING:
                Debug.Log(JSONObj.str);
                break;
            case JSONObject.Type.NUMBER:
                Debug.Log(JSONObj.n);
                break;
            case JSONObject.Type.BOOL:
                Debug.Log(JSONObj.b);
                break;
            case JSONObject.Type.NULL:
                Debug.Log("NULL");
                break;
        }
    }
    #endregion


    private void ShowQuestion()
    {
        StartCoroutine(CreateJsonObject(
            callBack => {
                if (callBack != null)
                {
                    print("Json Object is Created");
                    //listen to event
                    question = callBack;

                }
            }
            , 39, question));


    }

}