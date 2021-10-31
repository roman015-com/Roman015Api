using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Roman015API.Services
{
    public class InstagramScraperService : IHostedService, IDisposable
    {
        private Timer timer;

        private readonly string InstagramApi = "https://graph.instagram.com/me/media" 
            + "?fields=id,caption,media_type,media_url,thumbnail_url,timestamp,permalink" 
            + "&access_token=";        

        private string LongLivedAccessToken { get; } // These last for 60 days
        private IConfiguration Configuration { get; }
        private BlobContainerClient BlobContainer { get; }

        public InstagramScraperService(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
            this.BlobContainer = new BlobContainerClient(Configuration["AzureBlobConnectionString"], "instagram-data");
            this.LongLivedAccessToken = System.Environment.GetEnvironmentVariable("LongLivedAccessToken");
        }        

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(async (state) =>
            {
                try
                {
                    string[] ids;

                    // Load Existing Data From Json
                    //var blobClient = BlobContainer.GetBlobClient(this.HomeFileName);
                    //var jsonData = new StreamReader(blobClient.OpenRead()).ReadToEnd();
                    //List<InstagramPost> instagramPosts = JsonSerializer.Deserialize<List<InstagramPost>>(jsonData);

                    // Load Data From Instagram
                    string url = InstagramApi + this.LongLivedAccessToken;
                    var response = await (new HttpClient()).GetStringAsync(url);
                    var responseInstagram = JsonSerializer.Deserialize<InstagramResponse>(response);
                    List<InstagramPost> postsFromApi = responseInstagram.data.ToList();

                    // Keep Loading the rest of the data, if any
                    while(!string.IsNullOrWhiteSpace(responseInstagram.paging.next))
                    {                        
                        url = responseInstagram.paging.next;
                        response = await (new HttpClient()).GetStringAsync(url);
                        responseInstagram = JsonSerializer.Deserialize<InstagramResponse>(response);
                        postsFromApi.AddRange(responseInstagram.data);
                    }

                    #region Sort & Store Data
                    #region Game
                    string GameFileName = "game.json";
                    List<InstagramPost> GamePosts = postsFromApi
                        .Where(item => item.caption.Contains("#GameA ") || item.caption.Contains("#GameN "))
                        .ToList();

                    ids = GamePosts.Select(item => item.id).ToArray();
                    postsFromApi = postsFromApi
                        .Where(item => !ids.Contains(item.id))
                        .ToList();

                    // Upload data to files
                    BlobContainer.DeleteBlobIfExists(GameFileName);
                    BlobContainer.UploadBlob(GameFileName, BinaryData.FromString(JsonSerializer.Serialize(GamePosts)));
                    #endregion

                    #region Home
                    string HomeFileName = "home.json";
                    List<InstagramPost> HomePosts = postsFromApi
                        .Where(item => item.caption.Contains("#Home "))
                        .Select(item =>
                        {
                            item.caption.Replace("#Home ", string.Empty);
                            return item;
                        })
                        .ToList();

                    ids = HomePosts.Select(item => item.id).ToArray();
                    postsFromApi = postsFromApi
                        .Where(item => !ids.Contains(item.id))
                        .ToList();

                    // Upload data to files
                    BlobContainer.DeleteBlobIfExists(HomeFileName);
                    BlobContainer.UploadBlob(HomeFileName, BinaryData.FromString(JsonSerializer.Serialize(HomePosts)));
                    #endregion

                    #region Announcement
                    string AnnouncementFileName = "announcement.json";
                    List<InstagramPost> AnnouncementPosts = postsFromApi
                        .Where(item => item.caption.Contains("#Announcement "))
                        .Select(item =>
                        {
                            item.caption.Replace("#Announcement ", string.Empty);
                            return item;
                        })
                        .OrderBy(item => item.caption)
                        .ToList();

                    ids = AnnouncementPosts.Select(item => item.id).ToArray();
                    postsFromApi = postsFromApi
                        .Where(item => !ids.Contains(item.id))
                        .ToList();

                    // Upload data to files
                    BlobContainer.DeleteBlobIfExists(AnnouncementFileName);
                    BlobContainer.UploadBlob(AnnouncementFileName, BinaryData.FromString(JsonSerializer.Serialize(AnnouncementPosts)));
                    #endregion

                    #region Betrothal
                    string BetrothalFileName = "betrothal.json";
                    List<InstagramPost> BetrothalPosts = postsFromApi
                        .Where(item => item.caption.Contains("#Betrothal "))
                        .Select(item =>
                        {
                            item.caption.Replace("#Betrothal ", string.Empty);
                            return item;
                        })
                        .OrderBy(item => item.caption)
                        .ToList();

                    ids = BetrothalPosts.Select(item => item.id).ToArray();
                    postsFromApi = postsFromApi
                        .Where(item => !ids.Contains(item.id))
                        .ToList();

                    // Upload data to files
                    BlobContainer.DeleteBlobIfExists(BetrothalFileName);
                    BlobContainer.UploadBlob(BetrothalFileName, BinaryData.FromString(JsonSerializer.Serialize(BetrothalPosts)));
                    #endregion

                    //#region LiveStream (i.e., Anything else not filtered out)
                    //string LiveStreamFileName = "live.json";
                    //List<InstagramPost> LivePosts = postsFromApi
                    //    .ToList();


                    //// Upload data to files
                    //BlobContainer.DeleteBlobIfExists(LiveStreamFileName);
                    //BlobContainer.UploadBlob(LiveStreamFileName, BinaryData.FromString(JsonSerializer.Serialize(LivePosts)));
                    //#endregion
                    #endregion
                }
                catch(Exception)
                {
                    // In Case Instagram goes down, do nothing, existing files should be okay
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }

    public class InstagramPost
    {
        public String id { get; set; }
        public String caption { get; set; }
        public String media_type{ get; set; }
        public String media_url{ get; set; }
        public String thumbnail_url{ get; set; }
        public String timestamp { get; set; }
        public String permalink{ get; set; }
        public String css{ get; set; }
    }

    public class InstagramCursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class InstagramPaging
    {
        public InstagramCursors cursors { get; set; }
        public string next { get; set; }        
    }

    public class InstagramResponse
    {
        public InstagramPost[] data { get; set; }
        public InstagramPaging paging { get; set; }
    }
}
