using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class Video : MonoBehaviour
{
    VideoPlayer videoPlayer;
    public string videoFileName = "your_video_filename.mp4";

    void Start()
    {
        // Get the VideoPlayer component attached to the GameObject
        videoPlayer = GetComponent<VideoPlayer>();

        // Set the video URL using the path to the StreamingAssets folder
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoPath;

        // Play the video
        videoPlayer.Play();
    }

}