using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace QnaBot.Dialogs
{
    /// <summary>
    /// QnAMakerService is a wrapper over the QnA Maker REST endpoint
    /// </summary>
    [Serializable]
    public class QnAMakerService
    {
        private string qnaServiceHostName;
        private string knowledgeBaseId;
        private string endpointKey;

        /// <summary>
        /// Initialize a particular endpoint with its details
        /// </summary>
        /// <param name="hostName">Hostname of the endpoint</param>
        /// <param name="kbId">Knowledge base ID</param>
        /// <param name="ek">Endpoint Key</param>
        public QnAMakerService(string hostName, string kbId, string ek)
        {
            qnaServiceHostName = hostName;
            knowledgeBaseId = kbId;
            endpointKey = ek;
        }

        /// <summary>
        /// Call the QnA Maker endpoint and get a response
        /// </summary>
        /// <param name="query">User question</param>
        /// <returns></returns>
        public string GetAnswer(string query)
        {
            var client = new RestClient(qnaServiceHostName + "/knowledgebases/" + knowledgeBaseId + "/generateAnswer");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "EndpointKey " + endpointKey);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"question\": \"" + query + "\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Deserialize the response JSON
                QnAAnswer answer = JsonConvert.DeserializeObject<QnAAnswer>(response.Content);

                // Return the answer if present
                if (answer.Answers.Count > 0)
                {
                    if (answer.Answers[0].answer != "No good match found in KB.")
                    {
                        return answer.Answers[0].answer;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                throw new Exception($"QnAMaker call failed with status code {response.StatusCode} {response.StatusDescription}");
            }
        }
    }

    /* START - QnA Maker Response Class */
    public class Metadata
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Answer
    {
        public IList<string> Questions { get; set; }
        public string answer { get; set; }
        public double Score { get; set; }
        public int Id { get; set; }
        public string Source { get; set; }
        public IList<object> Keywords { get; set; }
        public IList<Metadata> Metadata { get; set; }
    }

    public class QnAAnswer
    {
        public IList<Answer> Answers { get; set; }
    }
    /* END - QnA Maker Response Class */
}
