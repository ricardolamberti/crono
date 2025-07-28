using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Sprite[] northSprites;
    public Sprite[] southSprites;
    public Sprite[] eastSprites;
    public Sprite[] westSprites;

    private Sprite[] currentSprites;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private float frameRate = 0.3f;

    private bool isWalking = false;

    void Update()
    {
        if (!isWalking || currentSprites == null || currentSprites.Length == 0)
            return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameRate)
        {
            currentFrame = (currentFrame + 1) % currentSprites.Length;
            spriteRenderer.sprite = currentSprites[currentFrame];
            frameTimer = 0f;
        }
    }

    public void SetDirection(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            if (dir.x > 0)
                currentSprites = eastSprites;
            else
                currentSprites = westSprites;
        }
        else
        {
            if (dir.z > 0)
                currentSprites = northSprites;
            else
                currentSprites = southSprites;
        }

        currentFrame = 0;
        frameTimer = 0f;

        if (currentSprites != null && currentSprites.Length > 0)
            spriteRenderer.sprite = currentSprites[0];
    }

    public void SetWalking(bool walking)
    {
        isWalking = walking;

        if (!walking && currentSprites != null && currentSprites.Length > 0)
        {
            // Mostrar siempre el primer frame cuando se detiene
            spriteRenderer.sprite = currentSprites[0];
            currentFrame = 0;
            frameTimer = 0;
        }
    }
}
