using UnityEngine;

public class NumberRainEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem numberParticle;

    [Header("Auto Bounds")]
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float boundsPadding = 1.5f;

    [Header("Movement")]
    [SerializeField] private float topY = 7f;
    [SerializeField] private float bottomY = -7f;
    [SerializeField] private float fallSpeed = 2.5f;

    [Header("Horizontal Position")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Particle Spacing")]
    [SerializeField] private float particleSpacing = 0.6f;
    [SerializeField] private bool emitOnReset = true;

    private float _movedDistance;

    private void Awake()
    {
        ResetEmitter();
        UpdateBoundsFromCamera();
    }

    private void Update()
    {
        float movement = fallSpeed * Time.deltaTime;

        transform.position += new Vector3(0f, -1f * movement, 0f);
        _movedDistance += movement;

        while (_movedDistance >= particleSpacing)
        {
            EmitNumber();
            _movedDistance -= particleSpacing;
        }

        if (transform.position.y <= bottomY)
            ResetEmitter();
    }

    private void EmitNumber()
    {
        if (numberParticle == null)
            return;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = transform.position;

        numberParticle.Emit(emitParams, 1);
    }

    private void ResetEmitter()
    {
        UpdateBoundsFromCamera();

        float randomX = Random.Range(minX, maxX);

        transform.position = new Vector3(randomX, topY, transform.position.z);

        _movedDistance = 0f;

        if (emitOnReset)
            EmitNumber();
    }

    private void UpdateBoundsFromCamera()
    {
        if (!useCameraBounds)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null || !targetCamera.orthographic)
            return;

        float cameraHalfHeight = targetCamera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * targetCamera.aspect;

        Vector3 cameraPosition = targetCamera.transform.position;

        topY = cameraPosition.y + cameraHalfHeight + boundsPadding;
        bottomY = cameraPosition.y - cameraHalfHeight - boundsPadding;

        minX = cameraPosition.x - cameraHalfWidth - boundsPadding;
        maxX = cameraPosition.x + cameraHalfWidth + boundsPadding;
    }
}