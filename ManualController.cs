using UnityEngine;

public class ManualController : MonoBehaviour {
    public DiamondSquare terrain;
    
    void Start() {
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            terrain.ExecuteDiamondSquare();
            // terrain.Colorize();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            terrain.Reset();
            // terrain.Colorize();
        }
    }
}
