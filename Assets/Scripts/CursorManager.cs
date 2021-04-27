using System.Linq;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] public CursorAnimation[] cursorAnimations;
    private CursorType currentCursorType;
    private CursorAnimation currentCursor;
    private uint currentFrame;
    private float frameTime;
    private bool looping;

    private static CursorManager _instance;
    public static CursorManager Instance { get { return _instance; }}
    void Awake()
    {
        // Maintain if a new instance is created
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    void Start()
    {
        ResetCursor();
    }

    void Update()
    {
        if (frameTime <= 0) {
            currentFrame = currentFrame + 1;
            if (currentFrame >= currentCursor.frames.Length) {
                if (looping) {
                    currentFrame = 0;
                } else {
                    ResetCursor();
                }
            }
            var audioClips = currentCursor.frames[currentFrame].audioClips;
            if (audioClips.Length > 0) {
                AudioSource.PlayClipAtPoint(PickOne(audioClips), Camera.main.ScreenToViewportPoint(Input.mousePosition));
            }

            frameTime = currentCursor.frames[currentFrame].frameTime;
        }

        var currentAnimation = currentCursor.frames[currentFrame];
        Cursor.SetCursor(currentAnimation.texture, currentCursor.hotspot, CursorMode.ForceSoftware);
        frameTime -= Time.deltaTime;
    }

    public void ResetCursor() {
        SetCursor(CursorType.Base, force: true, looping: false);
    }

    public void SetCursor(CursorType type, bool force = false, bool looping = false) {
        if (force || type != currentCursorType) {
            var animation = FindCursorAnimation(type);

            if (animation == null) {
                Debug.LogFormat("Unknown cursor animation for type {0}", type);
            }
            currentCursor = new CursorAnimation(animation);
            currentFrame = 0;
            currentCursorType = type;
            this.looping = looping;
            frameTime = currentCursor.frames[0].frameTime;
        }
    }

    public enum CursorType {
        Base,
        Hover,
        Click,
        RightClick
    }

    private CursorAnimation FindCursorAnimation(CursorType type) {
        return cursorAnimations.First(animation => animation.type == type);
    }

    [System.Serializable]
    public struct AnimationFrame {
        public Texture2D texture;
        public float frameTime;
        public AudioClip[] audioClips;
    }

    [System.Serializable]
    public class CursorAnimation {
        public CursorAnimation(CursorAnimation animation) {
            this.type = animation.type;
            this.hotspot = animation.hotspot;
            this.frames = animation.frames.ToArray();
        }
        public CursorType type;
        public AnimationFrame[] frames;
        public Vector2 hotspot;
    }

    T PickOne<T>(T[] array) {
        return array[Random.Range(0, array.Length)];
    }
}
