using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;

using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TriggerStateMachineFunction {
    public class Function {

        //--- Fields ---
        private readonly AmazonStepFunctionsClient _stepFunctionsClient = new AmazonStepFunctionsClient();
        private readonly string _stateMachineArn;

        //--- Constructors ---
        public Function() : this(Environment.GetEnvironmentVariable("stateMachineArn")) { }

        public Function(string stateMachineArn) {
            _stateMachineArn = stateMachineArn ?? throw new NullReferenceException(nameof(stateMachineArn));
        }

        //--- Methods ---
        public async Task FunctionHandler(S3Event @event, ILambdaContext context) {
            var wrappedEvent = JsonConvert.SerializeObject(new { S3Event = @event });
            LambdaLogger.Log(wrappedEvent);
            await _stepFunctionsClient.StartExecutionAsync(new StartExecutionRequest {
                Input = wrappedEvent,
                Name = Guid.NewGuid().ToString(),
                StateMachineArn = _stateMachineArn,
            });
        }
    }
}
