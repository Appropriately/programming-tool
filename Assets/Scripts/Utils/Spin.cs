using UnityEngine;

public class Spin : MonoBehaviour
{
    void Update()
    {
        transform.Rotate (0, 0, 50 * Time.deltaTime);
    }
}
