using UnityEngine;

public class BackgroundMovementController : MonoBehaviour {
    [Header("Layer Setting")]
    public float[] Layer_Speed = new float[5];
    public GameObject[] Layer_Objects = new GameObject[5];

    private Transform _camera;
    private float[] startPos = new float[5];

    public Transform parent;

    private float sizeX;
    private float boundSizeX;

    void Start() {
        _camera = Camera.main.transform;

        sizeX = Layer_Objects[0].transform.localScale.x * parent.localScale.x;
        boundSizeX = Layer_Objects[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        for (int i = 0; i < startPos.Length; i++) {
            startPos[i] = _camera.position.x;
        }
    }

    void Update() {
        for (int i = 0; i < Layer_Objects.Length; i++) {
            float temp = _camera.position.x * (1 - Layer_Speed[i]);
            float distance = _camera.position.x * Layer_Speed[i];

            Layer_Objects[i].transform.position = new Vector2(startPos[i] + distance, _camera.position.y);

            if (temp > startPos[i] + boundSizeX * sizeX) {
                startPos[i] += boundSizeX * sizeX;
            } else if (temp < startPos[i] - boundSizeX * sizeX) {
                startPos[i] -= boundSizeX * sizeX;
            }

        }
    }
}
