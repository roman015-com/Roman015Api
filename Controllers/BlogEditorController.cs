using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Roman015API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Roman015API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BlogEditorController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private BlobContainerClient BlogContainer { get; }

        private BlogViewerController blogViewerController;

        private readonly int maxFileSizeBytes = 1024 * 1024 * 2; // 2MB

        public BlogEditorController(IConfiguration Configuration, BlogViewerController blogViewerController)
        {
            this.Configuration = Configuration;
            this.BlogContainer = new BlobContainerClient(Configuration["AzureBlobConnectionString"], "blog");
            this.blogViewerController = blogViewerController;
        }

        [HttpPost]
        [Route("UploadFile")]
        [Authorize(Roles = "BlogAdministrator")]
        public IActionResult UploadFile(UploadedFile uploadedFile)
        {
            if (uploadedFile.Bytes.LongLength > maxFileSizeBytes)
            {
                return BadRequest("File Size Must Be Below 2MB");
            }

            if(!uploadedFile.FileName.Contains(".") || uploadedFile.FileName.EndsWith(".md"))
            {
                return BadRequest("File must have extension that is not '.md'");
            }

            try
            {
                BlogContainer.DeleteBlobIfExists(uploadedFile.FileName);
                BlogContainer.UploadBlob(uploadedFile.FileName, BinaryData.FromBytes(uploadedFile.Bytes));                

                return Ok();
            }
            catch(Exception e)
            {
                return StatusCode(500, e.Message);
            }            
        }

        [HttpPost]
        [Route("UploadPost")]
        [Authorize(Roles = "BlogAdministrator")]
        public IActionResult UploadPost(UploadedPost uploadedPost)
        {
            int postId = int.Parse(uploadedPost.PostId.Replace("post_",""));

            if (postId < 0)
            {
                return BadRequest("Invalid postID");
            }

            if(string.IsNullOrWhiteSpace(uploadedPost.PostMarkDown))
            {
                return BadRequest("Invalid postMarkDown");
            }            

            try
            {
                string FileName = uploadedPost.PostId + ".md";
                BlogContainer.DeleteBlobIfExists(FileName);
                BlogContainer.UploadBlob(FileName, BinaryData.FromString(uploadedPost.PostMarkDown));

                //Reload Cache 
                blogViewerController.ReloadCache();

                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
