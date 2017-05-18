using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SendNotification {
    public class Function {
        
        public string FunctionHandler(string input, ILambdaContext context) {
            
            // Boss Level: Notification
            // Write a final state that will merge the results from `DetectFaces` and `HotDogDetector` and notify you if a hot dog image is uploaded and how links to the faces that were in the image.
            return string.Empty;
        }
    }
}
