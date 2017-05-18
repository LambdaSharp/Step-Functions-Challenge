# Step Functions -> Image Analyzer

[Presentation](https://docs.google.com/presentation/d/1go759T_2Nmved-L8EWBLS_bDQxFbj5l4T9Ry6eK5Who/edit?usp=sharing)

In this challenge we're going to explore [AWS Step Functions](https://aws.amazon.com/step-functions/) while building an image analyzer that can identify and cut out faces (and also hot dogs) from images that are uploaded to S3.


## Prerequisites

1. AWS Tools
    1. Sign-up for an [AWS account](https://aws.amazon.com)
    2. Install [AWS CLI](https://aws.amazon.com/cli/)
2. .NET Core
    1. Install [.NET Core 1.0](https://www.microsoft.com/net/core) **(NOTE: You MUST install 1.0. Do NOT install 1.1 or later!)**
    2. Install [Visual Studio Code](https://code.visualstudio.com/)
    3. Install [C# Extension for VS Code](https://code.visualstudio.com/Docs/languages/csharp)
3. AWS C# Lambda Tools
    1. [Install](https://aws.amazon.com/blogs/developer/creating-net-core-aws-lambda-projects-without-visual-studio/) the templates to the `dotnet` tool for AWS Lambda: `dotnet new -i Amazon.Lambda.Templates::*`

## Setup: Set up your AWS environment
Create an IAM Role in your aws account: `lambdasharp-stepfunctions` and give the role the following Permissions:

- `AmazonS3FullAccess`
- `AmazonRekognitionFullAccess`
- `AmazonSNSFullAccess`
- `AWSStepFunctionsFullAccess`

For simplicity we will use this role for all our lambda functions, but feel free to implement more granular permissions if this approach is against your philosophy.

Please visit all files called `aws-lambda-tools-defaults.json` and update all the relevant information to your AWS environment.

## Level 0: Deploy the provided lambda function called DetectFacesFunction

This function analyzes the provided image and performs face detection using [AWS Rekognition](https://aws.amazon.com/rekognition).

In this [repository](https://github.com/LambdaSharp/May2017-StepFunctionsChallenge), find the `DetectFacesFunction/src/DetectFacesFunction` directory.

Run `dotnet restore` and `dotnet build` and finally `dotnet lambda deploy-function`. This will deploy the function to your AWS account.

**ACCEPTANCE TEST:** Upload an image that contains human faces to S3, and run the lambda function with the [sample input](https://gist.github.com/yurigorokhov/cd6a6edb73c6bf42ae338ad85c9bd34e). Don't forget to replace the strings `IMAGE_BUCKET` and `IMAGE_KEY` with the appropriate values. The function should return URL's to the various faces that it detected in the input image. 

## Level 1: Create your first step function
Create a simple state machine using [AWS Step Functions](https://aws.amazon.com/step-functions/) that will run our DetectFacesFunction. 

```
Start -> DetectFaces -> End
```

**ACCEPTANCE TEST:** Run the state machine with the same input your used for your lambda function. Ensure your lambda function is run successfully.


## Level 2: Invoke the state machine when a file is uploaded to S3

So that we can focus on the fun stuff, we have provided you with a lambda function to serve as the glue in the folder `TriggerStateMachineFunction/src/TriggerStateMachineFunction`

Set up the lambda function to trigger when a file is uploaded to an S3 bucket. It is recommended that you create a folder called `upload` and use that as your directory where you will upload your images to. Only trigger the step function on that directory prefix.

**ACCEPTANCE TEST:** When a file is uploaded to S3 the state machine is automatically run.

## Level 3: Create a lambda function that determines if the image contains a #HotDogOrNot :)

Write a lambda function that will analyze an image located on S3, and return true if the image contains a hot dog, false otherwise.

**ACCEPTANCE TEST:** The lambda function should accept the following [sample input](https://gist.github.com/yurigorokhov/cd6a6edb73c6bf42ae338ad85c9bd34e). I recommend you use `DetectFacesFunction` as a starting point. The function should return `true` if a hotdot is present in the image, and `false` if it is not.

**OPTIONAL MATERIAL:** [HotDogOrNot](https://www.youtube.com/watch?v=ACmydtFDTGs)


## Level 4: Hook up the HotDog function to run in parallel with the face analyzer

The new state machine should look something like:

```
Start ->        DetectFaces       -> End
        \                        /
         \ -> HotDogDetector -> /
```

** ACCEPTANCE TEST:** Detect faces and HotDogDetector run in parallel. 

## Level 5: Error Handling

Introduce a random error somewhere in the `DetectFacesFunction` code such as:

```csharp
if(rand.Next(0, 3) == 2)) {
	throw new MyRandomException()
}
```

**ACCEPTANCE TEST:** Ensure your state machine knows that this is not a permanent error and retries the operation. It should however still fail if other (more permanent) exceptions are thrown.

## Boss Level: Notification

Write a final state that will merge the results from `DetectFaces` and `HotDogDetector` and notify you when a hot dog image is uploaded and how many faces were in the image.
