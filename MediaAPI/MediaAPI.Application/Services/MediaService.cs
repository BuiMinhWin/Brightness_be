using FFMpegCore;
using FFMpegCore.Pipes;
using MediaAPI.Application.DTOs;
using MediaAPI.Application.Interfaces;
using MediaAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using   YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace MediaAPI.Application.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly YoutubeClient _youtubeClient;

        public MediaService(IMediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
            _youtubeClient = new YoutubeClient();
        }

        public async Task<MediaItemDTO> CreateVideoAsync(CreateMediaItemRequest createRequest)
        {
            var mediaItem = new MediaItem
            {
                Title = createRequest.Title,
                Description = createRequest.Description,
                Url = createRequest.Url,
                Thumbnail = createRequest.Thumbnail,
                Category = createRequest.Category,
                Duration = createRequest.Duration,
                Platform = createRequest.Platform,
                Source = createRequest.Source,
                CreatedAt = DateTime.UtcNow
            };

            var newItem = await _mediaRepository.AddAsync(mediaItem);
            return new MediaItemDTO(newItem.Id, newItem.Title, newItem.Description, newItem.Url, newItem.Thumbnail, newItem.Views, newItem.Likes, newItem.Category, newItem.Duration, newItem.Platform, newItem.Source, newItem.Status, newItem.CreatedAt, newItem.UpdatedAt);
        }

        public async Task<bool> DeleteVideoAsync(Guid id)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                return false;

            mediaItem.Status = "inactive";
            mediaItem.UpdatedAt = DateTime.UtcNow;

            return await _mediaRepository.UpdateAsync(mediaItem);
        }

        public async Task<IEnumerable<MediaItemDTO>> GetAllVideosAsync(int pageNumber, int pageSize)
        {
            var mediaItems = await _mediaRepository.GetAllAsync(pageNumber, pageSize);
            return mediaItems.Select(item => new MediaItemDTO(item.Id, item.Title, item.Description, item.Url, item.Thumbnail, item.Views, item.Likes, item.Category, item.Duration, item.Platform, item.Source, item.Status, item.CreatedAt, item.UpdatedAt));
        }

        public async Task<MediaItemDTO?> GetVideoByIdAsync(Guid id)
        {
            var item = await _mediaRepository.GetByIdAsync(id);
            if (item == null) return null;

            return new MediaItemDTO(item.Id, item.Title, item.Description, item.Url, item.Thumbnail, item.Views, item.Likes, item.Category, item.Duration, item.Platform, item.Source, item.Status, item.CreatedAt, item.UpdatedAt);
        }

        public async Task<bool> LikeVideoAsync(Guid id, Guid userId)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                return false;

            var existingLike = await _mediaRepository.GetLikeAsync(userId, id);
            if (existingLike != null)
            {
                return true;
            }

            var like = new UserLike { UserId = userId, MediaItemId = id };
            await _mediaRepository.AddLikeAsync(like);

            mediaItem.Likes++;
            await _mediaRepository.UpdateAsync(mediaItem);

            return true;
        }

        public async Task<bool> UnlikeVideoAsync(Guid id, Guid userId)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                return false;

            var existingLike = await _mediaRepository.GetLikeAsync(userId, id);
            if (existingLike == null)
            {
                return true;
            }

            return await _mediaRepository.RemoveLikeAndDecreaseCountAsync(existingLike, mediaItem);
        }

        public async Task<bool> UpdateVideoAsync(Guid id, UpdateMediaItemRequest updateRequest)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                return false;

            mediaItem.Title = updateRequest.Title ?? mediaItem.Title;
            mediaItem.Description = updateRequest.Description ?? mediaItem.Description;
            mediaItem.Url = updateRequest.Url ?? mediaItem.Url;
            mediaItem.Thumbnail = updateRequest.Thumbnail ?? mediaItem.Thumbnail;
            mediaItem.Category = updateRequest.Category ?? mediaItem.Category;
            mediaItem.Duration = updateRequest.Duration ?? mediaItem.Duration;
            mediaItem.Platform = updateRequest.Platform ?? mediaItem.Platform;
            mediaItem.Source = updateRequest.Source ?? mediaItem.Source;
            mediaItem.Status = updateRequest.Status ?? mediaItem.Status;
            mediaItem.UpdatedAt = DateTime.UtcNow;

            return await _mediaRepository.UpdateAsync(mediaItem);
        }

        public async Task<bool> ViewVideoAsync(Guid id, Guid userId)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                return false;

            var existingView = await _mediaRepository.GetViewAsync(userId, id);
            if (existingView != null)
            {
                return true;
            }

            var view = new UserView { UserId = userId, MediaItemId = id };
            await _mediaRepository.AddViewAsync(view);

            mediaItem.Views++;
            await _mediaRepository.UpdateAsync(mediaItem);

            return true;
        }

        public async Task<Stream> GetVideoFileAsync(Guid id)
        {
            var mediaItem = await _mediaRepository.GetByIdAsync(id);
            if (mediaItem == null)
                throw new Exception("Video not found");

            // Tạo thư mục temp
            var tempDirectory = Path.Combine(Path.GetTempPath(), "VideoDownloads");
            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            var videoTempPath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}_video.mp4");
            var audioTempPath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}_audio.mp4");
            var outputTempPath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}_output.mp4");

            try
            {
                // Cấu hình FFmpeg
                var ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg");
                if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))
                {
                    throw new Exception($"ffmpeg.exe not found at: {ffmpegPath}");
                }
                GlobalFFOptions.Configure(options => options.BinaryFolder = ffmpegPath);

                // Lấy stream info
                var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(mediaItem.Url);
                var videoStreamInfo = streamManifest.GetVideoOnlyStreams().TryGetWithHighestVideoQuality();
                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().TryGetWithHighestBitrate();

                if (videoStreamInfo == null || audioStreamInfo == null)
                    throw new Exception("No suitable video/audio stream found for this video.");

                // Download video stream vào file tạm
                Console.WriteLine("Downloading video stream...");
                await _youtubeClient.Videos.Streams.DownloadAsync(videoStreamInfo, videoTempPath);

                // Download audio stream vào file tạm
                Console.WriteLine("Downloading audio stream...");
                await _youtubeClient.Videos.Streams.DownloadAsync(audioStreamInfo, audioTempPath);

                // Merge video và audio bằng FFmpeg
                Console.WriteLine("Merging video and audio...");
                await FFMpegArguments
                    .FromFileInput(videoTempPath)
                    .AddFileInput(audioTempPath)
                    .OutputToFile(outputTempPath, true, options => options
                        .WithVideoCodec("copy")
                        .WithAudioCodec("aac")
                        .ForceFormat("mp4"))
                    .ProcessAsynchronously();

                // Đọc file output vào memory stream
                var outputStream = new MemoryStream();
                using (var fileStream = File.OpenRead(outputTempPath))
                {
                    await fileStream.CopyToAsync(outputStream);
                }
                outputStream.Position = 0;

                // Xóa các file tạm
                CleanupTempFiles(videoTempPath, audioTempPath, outputTempPath);

                return outputStream;
            }
            catch (YoutubeExplode.Exceptions.VideoUnavailableException)
            {
                CleanupTempFiles(videoTempPath, audioTempPath, outputTempPath);
                throw new Exception("Video is unavailable on YouTube.");
            }
            catch (Exception ex)
            {
                CleanupTempFiles(videoTempPath, audioTempPath, outputTempPath);
                Console.WriteLine($"Error: {ex}");
                throw new Exception($"Failed to fetch or merge video: {ex.Message}");
            }
        }

        private void CleanupTempFiles(params string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Console.WriteLine($"Deleted temp file: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete temp file {filePath}: {ex.Message}");
                }
            }
        }


    }
}
