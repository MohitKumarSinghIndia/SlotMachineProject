using System.Collections;
using UnityEngine;

public class testevent : MonoBehaviour
{
    public GameObject testObject;
   public void activateIt()
    {
        testObject.SetActive(!testObject.activeInHierarchy);
    }

    public void runnn()
    {
        StartCoroutine(demoWait());
    }

    public IEnumerator demoWait()
    {
        testObject.SetActive(!testObject.activeInHierarchy);
        yield return new WaitForSeconds(5f);
    }
}
