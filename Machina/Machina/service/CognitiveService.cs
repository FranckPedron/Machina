using System;
using System.Collections.Generic;
using System.Text;

namespace Machina.service
{
    private static readonly string API_KEY = "566824cab56946b183f27cf1e81da4e6";
    private static readonly string ENDPOINT_URL = "https://faceapifranck.cognitiveservices.azure.com/";

    public static class CognitiveService
    {
        public static void Facedetect() 
        {
            var url = ENDPOINT_URL + "detect" + "?returnFaceAttributes=age,gender";
        }
    }
}
