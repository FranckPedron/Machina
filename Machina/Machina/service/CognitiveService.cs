using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Machina.service
{

    public static class CognitiveService
    {
        private static readonly string API_KEY = "566824cab56946b183f27cf1e81da4e6";
        private static readonly string ENDPOINT_URL = "https://faceapifranck.cognitiveservices.azure.com/";

        public static void FaceDetect(Stream imageStream) 
        {
            if (imageStream == null)
            {
                return;
            }
            
            var url = ENDPOINT_URL + "detect" + "?returnFaceAttributes=age,gender";


            using (var webClient = new WebClient())
            {
                try
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/octet-stream";
                    webClient.Headers.Add("Ocp-Apim-Subscription-Key", API_KEY);

                    var data = ReadStream(imageStream);
                    var result = webClient.UploadData(url, data);
                   
                    if(result == null){
                        return;
                    }
                  
                    string json = Encoding.UTF8.GetString(result, 0, result.Length);
                    Console.WriteLine("Réponse OK" + json) ;
                    Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
       
        private static byte[] ReadStream(Stream input)
        {
            BinaryReader b = new BinaryReader(input);
            byte[] data = b.ReadBytes((int)input.Length);
            return data;
        }

    }
}
