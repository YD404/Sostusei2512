using UnityEngine;

public class ImageCutoff : MonoBehaviour
{




    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public enum LoopType
    {
        Restart,
        PingPong
    }

    [SerializeField]
    private EaseType easeType = EaseType.EaseOut;

    [SerializeField]
    private bool useCustomCurve = false;

    [SerializeField]
    private AnimationCurve customCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    private SpriteMask spriteMask;

    [SerializeField]
    private float duration = 1.5f;

    [SerializeField]
    private LoopType loopType = LoopType.Restart;

    [SerializeField]
    private bool invert = false;

    private float _timer;

    private void Start()
    {
        if (spriteMask == null)
        {
            spriteMask = GetComponent<SpriteMask>();
        }
    }

    private void Update()
    {
        // Update timer
        _timer += Time.deltaTime;

        float t = 0f;

        if (loopType == LoopType.Restart)
        {
            // Restart Logic
            if (_timer >= duration)
            {
                _timer = 0f;
            }
            t = _timer / duration;
        }
        else if (loopType == LoopType.PingPong)
        {
            t = Mathf.PingPong(_timer / duration, 1f);
        }

        float value = t;

        if (useCustomCurve)
        {
            value = customCurve.Evaluate(t);
        }
        else
        {
            switch (easeType)
            {
                case EaseType.Linear:
                    value = t;
                    break;
                case EaseType.EaseIn:
                    value = t * t * t; // Cubic Ease In
                    break;
                case EaseType.EaseOut:
                    value = 1f - Mathf.Pow(1f - t, 3f); // Cubic Ease Out
                    break;
                case EaseType.EaseInOut:
                    // Cubic Ease In Out
                    value = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                    break;
            }
        }

        // Invert if needed
        if (invert)
        {
            value = 1f - value;
        }

        // Apply
        if (spriteMask != null)
        {
            spriteMask.alphaCutoff = value;
        }
    }
}
