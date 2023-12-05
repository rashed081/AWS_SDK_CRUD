using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AWS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AWS.Controllers
{
    public class S3Controller : Controller
    {
        private IAmazonS3 _s3Client = new AmazonS3Client(new BasicAWSCredentials("____", "____"), RegionEndpoint.USEast1);

        public async Task<ActionResult> Index()
        {
            List<S3File> s3Files = await GetS3FilesAsync();

            return View(s3Files);
        }

        public async Task<List<S3File>> GetS3FilesAsync()
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = "rashedb8bucket1"
            };

            ListObjectsV2Response response = await _s3Client.ListObjectsV2Async(request);

            var s3Files = response.S3Objects.Select(obj => new S3File
            {
                FileName = obj.Key,
                FilePath = obj.Key
            }).ToList();

            return s3Files;
        }

        [HttpGet]
        public ActionResult DownloadFile(string objectKey)
        {
            try
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = "rashedb8bucket1",
                    Key = objectKey,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                };

                string url = _s3Client.GetPreSignedURL(request);

                return Redirect(url);
            }
            catch (AmazonS3Exception ex)
            {
                return Content($"S3 Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult> DeleteFile(string objectKey)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = "rashedb8bucket1",
                    Key = objectKey
                };

                await _s3Client.DeleteObjectAsync(request);

                return RedirectToAction("Index");
            }
            catch (AmazonS3Exception ex)
            {
                return Content($"S3 Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ViewBag.Message = "Please select a file to upload.";
                    return View("Upload");
                }

                string bucketName = "rashedb8bucket1";
                string objectKey = "uploads/" + Path.GetFileName(file.FileName);

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);

                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = objectKey,
                        InputStream = memoryStream
                    };

                    PutObjectResponse response = await _s3Client.PutObjectAsync(request);

                    ViewBag.Message = "File uploaded successfully!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error uploading file: {ex.Message}";
            }

            return View("Upload");
        }

    }
}
