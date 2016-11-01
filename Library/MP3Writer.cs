using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCorder
{
    // Takes a set of raw samples and writes them out as an MP3.
    public class MP3Writer
    {
        // First need to convert to wave format.
        private byte[] ConvertToWave(IList<Sample> samples, WaveFormat waveFormat)
        {
            byte[] result = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (WaveFileWriter writer = new WaveFileWriter(memoryStream, waveFormat))
                {
                    foreach (Sample sample in samples)
                    {
                        writer.Write(sample.Value, 0, sample.Value.Length);
                    }

                    // Writer won't finish on its own until it's disposed.
                    writer.Flush();

                    // Need to do this here because disposing the writer will close the memory stream.
                    result = memoryStream.ToArray();
                }
            }
            return result;
        }

        // Writes wave memorystream out to a file.
        private void WriteMP3(byte[] wave, string file)
        {
            string outputFile = file;

            if (File.Exists(outputFile))
            {
                outputFile = this.GetAlternatePath(outputFile);
            }

            using (MemoryStream waveMemoryStream = new MemoryStream(wave))
            {
                using (WaveFileReader reader = new WaveFileReader(waveMemoryStream))
                {
                    using (LameMP3FileWriter writer = new LameMP3FileWriter(outputFile, reader.WaveFormat, LAMEPreset.V4))
                    {
                        reader.CopyTo(writer);
                    }
                }
            }
        }

        //
        // For ...\File.mp3
        // returns ...\file.N.mp3
        //
        private string GetAlternatePath(string originalPath)
        {
            string outputFile = originalPath;
            int suffix = 0;

            string extension = Path.GetExtension(outputFile).Trim('.');
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(outputFile);
            string directoryName = Path.GetDirectoryName(outputFile);

            while (File.Exists(outputFile))
            {
                suffix++;
                outputFile = Path.Combine(directoryName, nameWithoutExtension + "." + suffix + "." + extension);
            }

            return outputFile;
        }

        public void WriteOutBuffer(RecordingBuffer buffer, string outputFile)
        {
            StatusManager.Status("Saving file: " + outputFile);
            byte[] waveFile = this.ConvertToWave(buffer.Samples, buffer.WaveFormat);
            this.WriteMP3(waveFile, outputFile);
        }
    }
}
