using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;

namespace House.House.Extensions;

public static class DiscordImageHelper
{
    private static readonly HttpClient client = new();
    private const int MaxBytes = 1024 * 1024;

    public static async Task<byte[]?> GetSafeImageBytesAsync(DiscordEmoji emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji.Url))
        {
            return null;
        }

        return await DownloadAndSanitizeImageAsync(emoji.Url);
    }

    public static async Task<byte[]?> GetSafeImageBytesAsync(DiscordMessageSticker sticker)
    {
        if (sticker is null || string.IsNullOrWhiteSpace(sticker.StickerUrl))
        {
            return null;
        }

        return await DownloadAndSanitizeImageAsync(sticker.StickerUrl);
    }

    public static async Task<byte[]?> GetSafeImageBytesAsync(DiscordRole role)
    {
        if (string.IsNullOrEmpty(role.IconUrl))
        {
            return null;
        }

        return await DownloadAndSanitizeImageAsync(role.IconUrl);
    }

    public static async Task<byte[]?> GetSafeImageBytesAsync(DiscordGuild guild)
    {
        if (string.IsNullOrEmpty(guild.IconUrl))
        {
            return null;
        }

        return await DownloadAndSanitizeImageAsync(guild.IconUrl);
    }

    private static async Task<byte[]?> DownloadAndSanitizeImageAsync(string url)
    {
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var contentLength = response.Content.Headers.ContentLength;

        if (contentLength.HasValue && contentLength.Value > MaxBytes)
        {
            throw new InvalidOperationException($"Image size exceeded {MaxBytes} bytes");
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var memoryStream = new MemoryStream();

        int totalRead = 0;
        byte[] buffer = new byte[8192];
        int read;

        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            totalRead += read;
            if (totalRead > MaxBytes)
            {
                throw new InvalidOperationException($"Image size exceeds {MaxBytes} bytes");
            }

            await memoryStream.WriteAsync(buffer.AsMemory(0, read));
        }

        memoryStream.Position = 0;

        using var image = await Image.LoadAsync(memoryStream);
        await using var pngStream = new MemoryStream();
        await image.SaveAsPngAsync(pngStream);

        return pngStream.ToArray();
    }

    private static bool IsValidImageHeader(byte[] data)
    {
        // PNG
        if (data.Length >= 8 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E &&
            data[3] == 0x47 && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            return true;
        }

        // JPEG
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return true;
        }

        // GIF
        if (data.Length >= 6 && data[0] == 'G' && data[1] == 'I' && data[2] == 'F')
        {
            return true;
        }

        return false;
    }
}