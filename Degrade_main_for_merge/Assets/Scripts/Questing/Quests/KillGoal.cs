using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGoal : Goal{
    public int EnemyID { get; set; }
    public KillGoal(int EnemyID, int RequiredAmount, string Description
    ,bool Completed = false,int CurrentAmount = 0)
    {
         this.EnemyID = EnemyID;
         this.Description = Description;
         this.Completed = Completed;
         this.RequiredAmount = RequiredAmount;
         this.CurrentAmount = CurrentAmount;
    }
    public override void Init(){
        base.Init();
        
    }

    void EnemyDied(int EnemyID){
        if(EnemyID == this.EnemyID){
            CurrentAmount++;
            Evaluate();
        }
    }
}
