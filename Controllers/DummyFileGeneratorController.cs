using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Roman015API.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Roman015API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DummyFileGeneratorController : ControllerBase
    {
        private readonly long OneKiloByteInBytes = 1024;
        private readonly long OneMegaByteInBytes = 1024 * 1024;
        private readonly long OneGigaByteInBytes = 1024 * 1024 * 1024;

        // Max File size for safety reasons (100MB for now)
        private readonly long MaxFileSize = (1024 * 1024) * 100;

        private readonly ILogger<DummyFileGeneratorController> _logger;

        public DummyFileGeneratorController(ILogger<DummyFileGeneratorController> logger)
        {
            _logger = logger;
        }

        private long ConvertHumanReadableSizeToByteSize(string sizeHumanReadable)
        {
            long result = 0;

            if (string.IsNullOrWhiteSpace(sizeHumanReadable))
            {
                // Empty string - Invalid Number
                return -1;
            }

            // Remove all spaces in the input
            sizeHumanReadable = sizeHumanReadable
                                .Replace(" ", string.Empty)
                                .ToUpper();

            // If no specifier is mentioned, it is assumed to be bytes by default
            if (long.TryParse(sizeHumanReadable, out result))
            {
                return result;
            }

            // Last one or two characters must have the size specifier (K, KB, MB, GB, etc.)            
            if (sizeHumanReadable.EndsWith("K") || sizeHumanReadable.EndsWith("KB"))
            {
                // kilobyte
                if (decimal.TryParse(sizeHumanReadable.Substring(0, sizeHumanReadable.IndexOf("K")), out decimal sizeKB))
                {
                    return (long)(sizeKB * OneKiloByteInBytes);
                }
            }
            else if (sizeHumanReadable.EndsWith("MB"))
            {
                // megabyte
                if (decimal.TryParse(sizeHumanReadable.Substring(0, sizeHumanReadable.IndexOf("MB")), out decimal sizeMB))
                {
                    return (long)(sizeMB * OneMegaByteInBytes);
                }
            }
            else if (sizeHumanReadable.EndsWith("GB"))
            {
                // gigabyte
                if (decimal.TryParse(sizeHumanReadable.Substring(0, sizeHumanReadable.IndexOf("GB")), out decimal sizeGB))
                {
                    return (long)(sizeGB * OneGigaByteInBytes);
                }
            }
            else
            {
                // Invalid number
                return -1;
            }

            return result;
        }

        [HttpGet]
        public IActionResult GetDummyFile([FromQuery] string size, [FromQuery] string extension = "txt", [FromQuery] string name = "")
        {
            string filename;

            long byteSize = ConvertHumanReadableSizeToByteSize(size);
            if (byteSize <= 0)
            {
                return BadRequest("Bad File Size Specified '" + size + "'");
            }

            if (byteSize > MaxFileSize)
            {
                return BadRequest("Maximum File Size Allowed is " + MaxFileSize + " bytes, requested " + byteSize + " Bytes");
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                return BadRequest("Invalid Extension '." + extension + "'");
            }

            filename = (string.IsNullOrWhiteSpace(name) ? size.Replace(" ", string.Empty) : name);

            try
            {
                return File(
                    new DummyTextFileStream(byteSize),
                    MediaTypeNames.Text.Plain,
                    filename
                        + "."
                        + extension);
            }
            catch (Exception ex)
            {
                return BadRequest("File Size Too Large To Generate '" + size + "' \n" + ex.Message + "\n-----\n" + ex.StackTrace);
            }
        }

        [HttpPost]
        public IActionResult PostDummyFile([FromForm] string size, [FromForm] string extension = "txt", [FromForm] string name = "")
        {
            // This is the Post version of the same API
            return GetDummyFile(size, extension, name);
        }
    }
}
