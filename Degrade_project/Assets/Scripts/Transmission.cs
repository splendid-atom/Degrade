using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transmission : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] bool isTransmission;
    [SerializeField] float time;

    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isTransmission)
        {
            if(time < 0.5f)
            {
                time += Time.deltaTime;
                time = Mathf.Clamp(time, 0f, 0.5f);
            }
            else
            {
                time = 0f;
                isTransmission = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // if(!isTransmission) return;
        // if(collision.gameObject.tag != "Player") return;
        // collision.transform.position = target.transform.position;
        // target.GetComponent<Transmission>().isTransmission = false;
    }


}

/*

Unity消息10个引用
private void UpdateO
iff (!isTransmission)
if (time < 0.5f)
time += Time.de1taTime;
time = Mathf.Clamp(time, Of, 0.5f) ;
else
time =Of;
isTransmission = true;
Unity消息10个引用
private void StartO
isTransmission = true;
Unity消息10个引用
private void OnTriggerEnter2D(Co11ider2D co1lision)
if (!isTransmission） return;
if （co1lision.game0bject.tag !="Player") return;
col1ision.transform.position =target.transform.position;
target.GetComponent<Transmission>O.isTransmission = false;

*/