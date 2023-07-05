# Autovie
Autovie short for Automatic movie, because it prepares the subtile files, for movies. This is created to save time! 
If you have a ton of movies and want to translate your subtitles to other langauges, as well as renaming them so jellyfin can pick them up automatically, then use this tool. Change the language that you want it to translate it to and from. In my example it English as source language and translating to Danish.
Youre also gonna want to edit the end of the file name, so it matches this format {movie name}.{language}.srt.
This is edited in the code, and youre gonna have to change the {language} part of it. 
As it currently is, it will only figure out danish and english, for the {language}, due to my needs. 

Set up a docker container for librtranslate OR use their API.
Docker installation guide is in here: https://github.com/LibreTranslate/LibreTranslate
Purchase API here: https://portal.libretranslate.com/
API has a price, however you can self host it for free, with docker, and its what i did.
Point your code to your server or use your api key.
Now build the project and place the .exe file in the directory that you want it to work in. This means, place it together with your movies.

Below is an example of how a directory should look:

Root directory: .exe file, movie 1 folder
movie 1 folder: video file, SRT file
movie 2 folder: video file, SRT file

You can have as many movie folders as you want. Place all movies and subtitle files inside its own movie folder, like in the example above.
Once the C# is done running, it will have:
* Found your SRT files and translated them to your target language
* Changed your file names to whatever your folder name is. This means you only have to update folder name manually.
* Given the SRT files the correct names, to be picked up by jellyfin

Now you can just make a computer run this script and you dont have to manually put files onto a website to make it translate your SRT files, which can take long, if you have tons of files to translate. Everything will be done automatically, and will be ready to be transfered to your jellyfin media server.

Translation speeds, using your own server, will vary depending on your hardware.
I heard that you could translate with your GPU, but i dont have a GPU in my server, so i havent looked much into it.
GPU translation would be much faster than CPU.
