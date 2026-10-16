using System.Text;
using NLayer;

namespace Mp3ToWav;

public static class Mp3Converter
{
    /// <summary>Decodes an MP3 file and writes it as a 16-bit PCM WAV file.</summary>
    public static void Convert(string mp3Path, string wavPath)
    {
        using var mpeg = new MpegFile(mp3Path);
        using var output = File.Create(wavPath);
        Convert(mpeg, output);
    }

    /// <summary>Decodes an MP3 stream and writes 16-bit PCM WAV data to <paramref name="wavOutput"/> (must be seekable).</summary>
    public static void Convert(Stream mp3Input, Stream wavOutput)
    {
        using var mpeg = new MpegFile(mp3Input);
        Convert(mpeg, wavOutput);
    }

    private static void Convert(MpegFile mpeg, Stream output)
    {
        if (!output.CanSeek)
            throw new ArgumentException("Output stream must be seekable.", nameof(output));

        int channels = mpeg.Channels;
        int sampleRate = mpeg.SampleRate;
        const short bitsPerSample = 16;

        long headerStart = output.Position;
        using var writer = new BinaryWriter(output, Encoding.ASCII, leaveOpen: true);
        WriteHeader(writer, channels, sampleRate, bitsPerSample, dataLength: 0);

        var floats = new float[4096 * channels];
        var pcm = new byte[floats.Length * 2];
        long dataLength = 0;
        int read;
        while ((read = mpeg.ReadSamples(floats, 0, floats.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
            {
                short s = (short)Math.Round(Math.Clamp(floats[i], -1f, 1f) * short.MaxValue);
                pcm[i * 2] = (byte)s;
                pcm[i * 2 + 1] = (byte)(s >> 8);
            }
            writer.Write(pcm, 0, read * 2);
            dataLength += read * 2;
        }

        if (dataLength > uint.MaxValue - 36)
            throw new InvalidOperationException("Decoded audio is too large for a WAV file (>4 GB).");

        long end = output.Position;
        output.Position = headerStart;
        WriteHeader(writer, channels, sampleRate, bitsPerSample, (uint)dataLength);
        output.Position = end;
        writer.Flush();
    }

    private static void WriteHeader(BinaryWriter w, int channels, int sampleRate, short bitsPerSample, uint dataLength)
    {
        short blockAlign = (short)(channels * bitsPerSample / 8);
        w.Write("RIFF"u8);
        w.Write(36u + dataLength);
        w.Write("WAVE"u8);
        w.Write("fmt "u8);
        w.Write(16);                       // fmt chunk size
        w.Write((short)1);                 // PCM
        w.Write((short)channels);
        w.Write(sampleRate);
        w.Write(sampleRate * blockAlign);  // byte rate
        w.Write(blockAlign);
        w.Write(bitsPerSample);
        w.Write("data"u8);
        w.Write(dataLength);
    }
}
