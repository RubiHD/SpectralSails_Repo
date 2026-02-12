using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicController : MonoBehaviour
{
    [Header("Configuración de Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage displayImage;

    [Header("Configuración de Escenas")]
    [SerializeField] private string nextSceneName; // Nombre de la siguiente escena

    [Header("Opciones")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Configurar el video player
        ConfigurarVideoPlayer();

        // Suscribirse al evento cuando termina el video
        videoPlayer.loopPointReached += OnVideoFinished;

        // Reproducir el video
        videoPlayer.Play();
    }

    private void ConfigurarVideoPlayer()
    {
        // Configurar la textura de renderizado
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        videoPlayer.targetTexture = renderTexture;

        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }

        // Configurar audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        if (videoPlayer.GetComponent<AudioSource>() == null)
        {
            videoPlayer.gameObject.AddComponent<AudioSource>();
        }
        videoPlayer.SetTargetAudioSource(0, videoPlayer.GetComponent<AudioSource>());
    }

    private void Update()
    {
        // Permitir saltar la cinemática
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            SkipCinematic();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Cargar la siguiente escena cuando termine el video
        LoadNextScene();
    }

    private void SkipCinematic()
    {
        videoPlayer.Stop();
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No se ha especificado el nombre de la siguiente escena");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}