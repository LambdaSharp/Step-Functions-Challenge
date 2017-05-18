using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using ImageSharp;
using System.IO;
using System.Threading;
using Amazon.S3.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DetectFacesFunction {

    public class Input {
        
        //--- Fields ---
        public S3Event S3Event;
    }

    public class FaceDetectionResults {

        //--- Fields ---
        public string[] Faces;
    }

    public class Function {
        
        //--- Class Methods ---
        private static void CropImage(FileInfo original, FileInfo outputFile, BoundingBox box) {
            using(var imageStream = original.OpenRead()) {
                var image = new ImageSharp.Image(imageStream);
                var left = (int)((box.Left < 0.0 ? 0.0 : box.Left) * image.Width);
                var top = (int)((box.Top < 0.0 ? 0.0 : box.Top) * image.Height);
                var width = (int)(box.Width * image.Width);
                var height = (int)(box.Height * image.Height);
                using(var image2Stream = outputFile.OpenWrite()) {
                    var newImage = image.Crop(width, height, new Rectangle(
                        left, 
                        top,
                        width, 
                        height
                    )).Save(image2Stream);
                }
            }
        }

        //--- Fields ---
        private readonly AmazonRekognitionClient _rekognitionClient = new AmazonRekognitionClient();
        private readonly AmazonS3Client _s3Client = new AmazonS3Client();

        //--- Methods ---
        public async Task<FaceDetectionResults> FunctionHandler(Input e, ILambdaContext context) {
            
            // get first record
            var record = e.S3Event.Records.FirstOrDefault();
            if(record == null) {
                return null;
            }
            var key = record.S3.Object.Key;
            var bucket = record.S3.Bucket.Name;
            
            // detect faces
            var facesDetected = await _rekognitionClient.DetectFacesAsync(new DetectFacesRequest {
                Attributes = new List<string> { "ALL" },
                Image = new Amazon.Rekognition.Model.Image {
                    S3Object = new Amazon.Rekognition.Model.S3Object {
                        Bucket = bucket,
                        Name = key
                    }
                }
            });

            // cut out the faces
            var faces = new List<string>();
            using(var response = await _s3Client.GetObjectAsync(bucket, key))  {
                var tempFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(key));
                await response.WriteResponseStreamToFileAsync(tempFile, false, default(CancellationToken));

                // crop out each face
                using(var imageStream = File.OpenRead(tempFile)) {
                    var i = 1;
                    foreach(var faceDetails in facesDetected.FaceDetails) {
                        var faceFile = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(key));
                        CropImage(new FileInfo(tempFile), new FileInfo(faceFile), faceDetails.BoundingBox);
                        
                        // upload cropped image to S3
                        var faceKey = $"faces/{Path.GetFileNameWithoutExtension(key)}/face_{i}{Path.GetExtension(key)}";
                        using(var faceStream = File.OpenRead(faceFile)) {
                            await _s3Client.PutObjectAsync(new PutObjectRequest {
                                BucketName = bucket,
                                Key = faceKey,
                                InputStream = faceStream
                            });
                        }
                        try { File.Delete(faceFile); } catch { }

                        // generate a signed url for face
                        faces.Add(_s3Client.GetPreSignedURL(new GetPreSignedUrlRequest {
                            BucketName = bucket,
                            Key = faceKey,
                            Expires = DateTime.UtcNow.Add(TimeSpan.FromDays(1))
                        }));
                        i++;
                    }
                }
                try { File.Delete(tempFile); } catch {}
            }
            return new FaceDetectionResults { Faces = faces.ToArray() };
        }
    }
}
