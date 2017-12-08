using ArabicTextAnalyzer.Contracts;
using RestSharp;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class ApiTextConverter : ITextConverter
    {
        private const string ScriptFileAddress = "http://localhost:8012/ArabicTextConverter/convert.php";

        public string Convert(string source)
        {
            var client = new RestClient(ScriptFileAddress);
            var request = new RestRequest(Method.POST);

            request.AddParameter("text/xml", source, ParameterType.RequestBody);

            var result = client.Execute(request);
            return result.Content;
        }
    }
}
