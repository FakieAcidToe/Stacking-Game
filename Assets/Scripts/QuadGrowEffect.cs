using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadGrowEffect : MonoBehaviour
{
    [SerializeField] float growScale = 0.5f;
    [SerializeField] float growOpacity = 0.5f;

    Material mat;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        transform.localScale = new Vector3(
            transform.localScale.x + growScale * Time.deltaTime,
            transform.localScale.y + growScale * Time.deltaTime,
            1f);
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, mat.color.a - growOpacity * Time.deltaTime);
        if (mat.color.a <= 0) Destroy(gameObject);
    }
}
