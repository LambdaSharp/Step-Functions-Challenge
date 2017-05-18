using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Lambda.S3Events;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DetectHotDogFunction {

    //--- Types ---
    public class EventWrapper { 
        public S3Event S3Event { get; set; }
    }

    public class Function {

        //--- Fields ---
        private readonly AmazonRekognitionClient _rekog = new AmazonRekognitionClient();

        //--- Methods ---
        public async Task<bool> FunctionHandler(EventWrapper e, ILambdaContext context) {
            var record = e.S3Event.Records.FirstOrDefault();
            
            //TODO: detect if the image has a hot dog and return true if it does :)
            return false;
        }
    }
}