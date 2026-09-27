using UnityEngine;

// Put this on the target prefab (needs a Collider, e.g. a Sphere).
public class Target : MonoBehaviour
{
    public int points = 1;
    [SerializeField] float spinSpeed = 90f;
    [SerializeField] float bobHeight = 0.03f;

    Vector3 startPos;
    Vector3 fullScale;
    float age;

    void Start()
    {
        startPos = transform.position;
        fullScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, Mathf.Clamp01(age * 5f)); // pop-in
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
        transform.position = startPos + Vector3.up * Mathf.Sin(age * 4f) * bobHeight;
    }

    public void Pop()
    {
        Destroy(gameObject);
    }
}
