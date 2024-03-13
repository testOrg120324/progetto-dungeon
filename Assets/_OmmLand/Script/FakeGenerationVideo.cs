using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FakeGenerationVideo : MonoBehaviour
{

    List<Renderer> list = new List<Renderer>();

    public int velocity = 10;

    public GameObject cam2;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        cam2.gameObject.SetActive(true);

        list = GetComponentsInChildren<Renderer>().ToList() ;
        var ordered = list.OrderBy(a =>Vector3.Distance(a.transform.position, Camera.main.transform.position));

        foreach (var item in ordered) item.gameObject.SetActive(false);

        yield return 0;

        int count = 0;
        foreach (var item in ordered)
        {
            item.gameObject.SetActive(true);
            count++;
            if (count == velocity)
            {
                count = 0;
                yield return 0;
            }
        }
        Debug.LogFormat("Finish");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
