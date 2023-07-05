using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Automatically detect which directory the .exe file is run from
        string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Display a warning message and the root directory
        Console.WriteLine("WARNING: Long translations can take a while to finish. Do not close the program!");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(rootDirectory);

        // Run the ProcessSubDirectories and pass along the rootDirectory
        await ProcessSubDirectories(rootDirectory);

        // This code is only run after the previous code is done awaiting.
        // Changes text color to cyan and displays a completion message
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("All translations complete!");
        Console.WriteLine("Press enter to close the program!");

        // Waits for user input to close the program
        Console.ReadLine();
    }

    // Method to process subdirectories within the root directory
    static async Task ProcessSubDirectories(string rootDirectory)
    {
        // Retrieve an array of subdirectory paths within the root directory
        string[] subDirectories = Directory.GetDirectories(rootDirectory);

        // Process each subdirectory
        foreach (string subDirectory in subDirectories)
        {
            // Changes text color to green
            Console.ForegroundColor = ConsoleColor.Green;

            // Get folder name, movie file, and original SRT file within the subdirectory
            string folderName = Path.GetFileName(subDirectory);
            string movieFile = GetMovieFile(subDirectory);
            string originalSrtFile = GetOriginalSrtFile(subDirectory);

            // Check if the original SRT file is missing or empty
            if (string.IsNullOrEmpty(originalSrtFile))
            {
                // Red text indicating the subdirectory does not have an original SRT file
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Skipping directory: {subDirectory} (No original SRT file found)");

                // Continue to the next subdirectory
                continue;
            }

            // Rename movie file
            if (!string.IsNullOrEmpty(movieFile))
            {
                // Changes text color to yellow
                Console.ForegroundColor = ConsoleColor.Yellow;

                // Determine the new movie file name
                string newMovieFileName = Path.Combine(subDirectory, $"{folderName}{Path.GetExtension(movieFile)}");
                File.Move(movieFile, newMovieFileName);
                movieFile = newMovieFileName;

                // Writes to the console the new name and changes the text color back to green
                Console.WriteLine("Changed filename to " + newMovieFileName);
                Console.ForegroundColor = ConsoleColor.Green;
            }

            // Translate SRT file to Danish
            await TranslateAndRenameSrtFile(originalSrtFile, "Danish", subDirectory, folderName);

            // Rename original SRT file to {folder name}.English.srt
            string englishSrtFile = Path.Combine(subDirectory, $"{folderName}.English.srt");
            File.Move(originalSrtFile, englishSrtFile);
        }
    }

    // Method to get the movie file within a directory
    static string GetMovieFile(string directoryPath)
    {
        string[] videoFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => IsVideoFile(file)).ToArray();

        if (videoFiles.Length > 0)
        {
            return videoFiles[0];
        }

        return null;
    }

    // Method to check if a file is a video file
    static bool IsVideoFile(string filePath)
    {
        // Check if the extension matches those in the return to determine if it's a video file format
        string extension = Path.GetExtension(filePath).ToLower();
        return (extension == ".mp4" || extension == ".mkv" || extension == ".mov"); // Add other video file extensions if needed
    }

    // Method to get the original SRT file within a directory
    static string GetOriginalSrtFile(string directoryPath)
    {
        string[] srtFiles = Directory.GetFiles(directoryPath, "*.srt");

        foreach (string srtFile in srtFiles)
        {
            string fileName = Path.GetFileName(srtFile);

            // Exclude files ending with ".Danish.srt" or ".English.srt" as they are already translated
            if (!fileName.EndsWith(".Danish.srt") && !fileName.EndsWith(".English.srt"))
            {
                return srtFile;
            }
        }

        return null;
    }

    // Method to translate and rename an SRT file
    static async Task TranslateAndRenameSrtFile(string originalSrtFile, string language, string subDirectory, string folderName)
    {
        try
        {
            string translatedSrtFile = Path.Combine(subDirectory, $"{folderName}.{language}.srt");

            // Translate the contents of the original SRT file
            string translation = await TranslateSrtFile(originalSrtFile, "en", "da");

            // Write the translated content to the new SRT file
            File.WriteAllText(translatedSrtFile, translation, Encoding.UTF8);

            Console.WriteLine($"Translated and renamed '{originalSrtFile}' to '{translatedSrtFile}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error translating '{originalSrtFile}' to '{language}': {ex.Message}");
        }
    }

    // Method to translate an SRT file using an HTTP client
    static async Task<string> TranslateSrtFile(string filePath, string sourceLanguage, string targetLanguage)
    {
        try
        {
            // Read the contents of the SRT file
            string srtContent = await File.ReadAllTextAsync(filePath);

            // Translate the text content
            string translation = await TranslateText(srtContent, sourceLanguage, targetLanguage);

            return translation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error translating '{filePath}': {ex.Message}");
            return string.Empty;
        }
    }

    // Method to translate text using an HTTP client
    static async Task<string> TranslateText(string text, string sourceLanguage, string targetLanguage)
    {
        try
        {
            string translation;

            using (var client = new HttpClient())
            {
                // Prepare the translation request
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("q", text),
                    new KeyValuePair<string, string>("source", sourceLanguage),
                    new KeyValuePair<string, string>("target", targetLanguage)
                });

                // Send the translation request
                var response = await client.PostAsync("http://10.108.169.51:5000/translate", content);

                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Translation request failed with status code: {response.StatusCode}");
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response content: {responseContent}");
                    return string.Empty;
                }

                // Extract the translated text from the response
                string responseJson = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseJson);
                var translationText = jsonDocument.RootElement.GetProperty("translatedText").GetString();
                translation = translationText.Trim();
            }

            return translation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error translating text: {ex.Message}");
            return string.Empty;
        }
    }
}
