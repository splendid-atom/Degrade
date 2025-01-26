using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Gun_attack : MonoBehaviour
{

    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform muzzle;

    private Vector2 direction;
    
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject temp =Instantiate(projectile,muzzle.position,Quaternion.identity);

            temp.GetComponent<Bullet_projectile>().SetDirection(direction);
        }


        //将鼠标位置转化为世界坐标系下的方向向量，减去gun的位置，得到的向量即为射击方向
        direction = Camera.main.ScreenToWorldPoint(Input.mousePosition)-transform.position;
        
         System.Console.WriteLine("direction:");
        System.Console.WriteLine(Input.mousePosition);
        //将射击方向向量归一化，否则鼠标越远 速度越快 
        direction = direction.normalized;

        //将射击方向向量转化为角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //将角度转化为旋转角度
        transform.rotation = Quaternion.Euler(0,0,angle);

    }
}

