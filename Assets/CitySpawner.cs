using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySpawner : MonoBehaviour
{
    public GameObject buildingPrefab;
    void Start() {
        Random.InitState(1337);
        int buildingSquare = 8;
        int jitterAmount = 15;
        for (int x = -buildingSquare; x < buildingSquare; x++) {
            for (int y = -buildingSquare; y < buildingSquare; y++) {
                if ((x * x + y * y) < 6 + Random.Range(-1, 2)) continue;
                var b = Instantiate(buildingPrefab, new Vector3(x * 45, 0, y * 45), Quaternion.identity);

                b.transform.position += new Vector3(Random.Range(-jitterAmount, jitterAmount), 0, Random.Range(-jitterAmount, jitterAmount));
                b.transform.localScale = new Vector3(Random.Range(15, 25), Random.Range(50, 100), Random.Range(15, 25));
                b.GetComponent<Renderer>().material.SetFloat("_Scale", Random.Range(0.75f, 1.25f) * b.GetComponent<Renderer>().material.GetFloat("_Scale"));
            }
        }
    }
}
