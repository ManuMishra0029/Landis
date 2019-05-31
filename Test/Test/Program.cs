using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("The purpose of Application : Only Automating creation of code merge task in TFS");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Please enter 1 for Bug Details or 2 for creation of Code merge Task");
            string _tfsOption = null;
            _tfsOption = Console.ReadLine();
            if (_tfsOption == "1")
                TFSWorkitem.GetWorkItemByID();
            if (_tfsOption == "2")
                TFSWorkitem.CreateCodeTaskInParentTFS();
            if ((_tfsOption == null) || _tfsOption != "1" || _tfsOption != "2")
            {
                Console.WriteLine("------------------------------------------------------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please Enter the only 1 or 2");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Please enter 1 for Bug Details or 2 for creation of Code merge Task");
                _tfsOption = Console.ReadLine();
                if (_tfsOption == "1")
                    TFSWorkitem.GetWorkItemByID();
                if (_tfsOption == "2")
                    TFSWorkitem.CreateCodeTaskInParentTFS();

            }
            Console.ReadKey();

        }


        static class TFSWorkitem
        {
            public static void GetWorkItemByID()
            {
                string _personalAccessToken = (System.Configuration.ConfigurationManager.AppSettings["_personalAccessToken"]);
                string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));
                Console.WriteLine("Please Enter the TFS Defect Number");
                string _workItemsNum = Console.ReadLine();
                string _targetRelease = null;
                string _workItemIDFromRes = null;
                var _result = "";
                //use the httpclient
                using (var client = new HttpClient())
                {
                    //set our headers
                    client.BaseAddress = new Uri("https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Please wait for TFS Bug detail={0} ", _workItemsNum);
                    Console.WriteLine("------------------------------------------------------------------------------------------------------------");
                    //send the request and content
                    HttpResponseMessage response = client.GetAsync(string.Format(client.BaseAddress + "_apis/wit/workitems?ids={0}&api-version=1.0", _workItemsNum)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        _result = response.Content.ReadAsStringAsync().Result;
                        JObject _jObject = JObject.Parse(_result);
                        _workItemIDFromRes = (string)_jObject.SelectToken("value[0].id");
                        JObject _fieldsJson = (JObject)_jObject.SelectToken("value[0].fields");
                        _targetRelease = (string)_fieldsJson["LandisGyr.TargetRelease"];
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format("TFS Bug Number is={0} and Target Release is={1}", _workItemIDFromRes, _targetRelease));
                    }
                    Console.WriteLine("------------------------------------------------------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Do you want to see other details in Json Format for this TFS Bug ? So Please enter Y or N");
                    string _flag = Console.ReadLine();
                    Console.WriteLine("------------------------------------------------------------------------------------------------------------");
                    if (_flag.ToUpper() == "Y")
                    {
                        dynamic parsedJson = JsonConvert.DeserializeObject(_result);
                        var _jsonFormattedData = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(_jsonFormattedData);
                    }

                }
                Console.ReadKey();

            }
            public static string GetTargetReleaseByTFSID(string _workItemId)
            {
                string _personalAccessToken = (System.Configuration.ConfigurationManager.AppSettings["_personalAccessToken"]);
                string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));
                string _workItemsNum = _workItemId;
                string _targetRelease = null;
                var _result = "";
                //use the httpclient
                using (var client = new HttpClient())
                {
                    //set our headers
                    client.BaseAddress = new Uri("https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
                    //send the request and content
                    HttpResponseMessage response = client.GetAsync(string.Format(client.BaseAddress + "_apis/wit/workitems?ids={0}&api-version=1.0", _workItemsNum)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        _result = response.Content.ReadAsStringAsync().Result;
                        JObject _jObject = JObject.Parse(_result);
                        JObject _fieldsJson = (JObject)_jObject.SelectToken("value[0].fields");
                        _targetRelease = (string)_fieldsJson["LandisGyr.TargetRelease"];

                    }

                }

                return _targetRelease;

            }
            public static void CreateCodeTaskInParentTFS()
            {
                string _personalAccessToken = (System.Configuration.ConfigurationManager.AppSettings["_personalAccessToken"]);
                string _credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Please enter the TFS ID for code merge task");
                string _parentWorkItemId = null;
                _parentWorkItemId = Console.ReadLine();
                string _GetTargetReleaseFromTFS = TFSWorkitem.GetTargetReleaseByTFSID(_parentWorkItemId);
                List<string> _createCodeTask = new List<string>();

                //  By default code merge in Master branch
                _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Master Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");


                // If  issue in CC 7.1 need to code merge in Integ-0.71.0,Integ-0.72.0,Integ-0.73.0 ,Integ-0.74.0 and Integ-0.75.0 branch.
                if (_GetTargetReleaseFromTFS == "CC 7.1" || _GetTargetReleaseFromTFS == "CC 7.1 MR1" || _GetTargetReleaseFromTFS == "CC 7.1 MR2" || _GetTargetReleaseFromTFS == "CC 7.1 MR3" || _GetTargetReleaseFromTFS == "CC 7.1 MR4")
                {
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge " + _GetTargetReleaseFromTFS + " Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.71.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.72.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.73.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.74.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.75.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                }

                // If  issue in CC 7.2 need to code merge in Integ-0.72.0,Integ-0.73.0 ,Integ-0.74.0 and Integ-0.75.0 branch.
                if (_GetTargetReleaseFromTFS == "CC 7.2" || _GetTargetReleaseFromTFS == "CC 7.2 MR0" || _GetTargetReleaseFromTFS == "CC 7.2 MR1" || _GetTargetReleaseFromTFS == "CC 7.2 MR2" || _GetTargetReleaseFromTFS == "CC 7.2 MR3")
                {
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge " + _GetTargetReleaseFromTFS + " Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.72.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.73.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.74.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.75.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                }

                // If  issue in CC 7.3 need to code merge in Integ-0.73.0 ,Integ-0.74.0 and Integ-0.75.0 branch.
                if (_GetTargetReleaseFromTFS == "CC 7.3" || _GetTargetReleaseFromTFS == "CC 7.3 MR0" || _GetTargetReleaseFromTFS == "CC 7.3 MR1" || _GetTargetReleaseFromTFS == "CC 7.3 MR2" || _GetTargetReleaseFromTFS == "CC 7.3 MR3")
                {
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge " + _GetTargetReleaseFromTFS + " Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.73.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.74.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.75.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                }

                // If  issue in CC 7.4 need to code merge Integ-0.74.0 and Integ-0.75.0 branch.
                if (_GetTargetReleaseFromTFS == "CC 7.4" || _GetTargetReleaseFromTFS == "CC 7.4 MR1" || _GetTargetReleaseFromTFS == "CC 7.4 MR2" || _GetTargetReleaseFromTFS == "CC 7.4 MR3")
                {
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge " + _GetTargetReleaseFromTFS + " Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.74.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                    _createCodeTask.Add("[  {    \"op\": \"add\",    \"path\": \"/fields/System.Title\",    \"from\": null,    \"value\": \"Code Merge Integ-0.75.0 Branch-TFS# " + _parentWorkItemId + "\"},    {    \"op\": \"add\",    \"path\": \"/relations/-\",    \"value\": {      \"rel\": \"System.LinkTypes.Hierarchy-Reverse\",      \"url\": \"https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/" + _parentWorkItemId + "\"} }]");
                }

                List<string> _createdNewWorkItemId = new List<string>();
                //use the httpclient
                using (var client = new HttpClient())
                {
                    //set our headers
                    client.BaseAddress = new Uri("https://am.tfs.landisgyr.net/tfs/DefaultCollection/Command Center/_apis/wit/workitems/$Task?api-version=1.0");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
                    for (int i = 0; i <= _createCodeTask.Count - 1; i++)
                    {
                        HttpContent _httpContent = null;
                        _httpContent = new StringContent(_createCodeTask[i], Encoding.UTF8, "application/json-patch+json");
                        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------");
                        //send the request and content
                        HttpResponseMessage _response = client.PostAsync(client.BaseAddress, _httpContent).Result;
                        if (_response.IsSuccessStatusCode)
                        {
                            var _result = _response.Content.ReadAsStringAsync().Result;
                            JObject _jObject = JObject.Parse(_result);
                            _createdNewWorkItemId.Add((string)_jObject.SelectToken("id"));
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(_result);
                        }
                    }
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------------------");

                    Console.WriteLine(string.Format("Code Merge task has been created in TFS for given Task #{0}.", _parentWorkItemId));
                    Console.WriteLine();
                    foreach (string createNewTaskID in _createdNewWorkItemId)
                    {
                        Console.WriteLine(string.Format("Created New Code Merge Task TFS ID #{0}", createNewTaskID));
                    }

                }

                Console.ReadKey();

            }



        }
    }


}
