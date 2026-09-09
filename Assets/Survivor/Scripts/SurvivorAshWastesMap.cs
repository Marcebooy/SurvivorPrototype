using UnityEngine;
namespace BonkSurvivor
{
    public sealed partial class SurvivorGame
    {
        // Fixed environment candidate, deliberately independent of inventory/drop balancing.
        void BuildAshWastesMap()
        {
            var prefab=Resources.Load<GameObject>("AshWastes/Tuhkaeramaa_Environment");
            if(!prefab) throw new System.InvalidOperationException("Build the Tuhkaeramaa art package first.");
            var instance=Instantiate(prefab,world,false);fixedMapRoot=instance.transform;
            float scale=arenaRadius/60f;fixedMapRoot.localScale=Vector3.one*scale;
            foreach(var o in instance.GetComponent<AshWastesEnvironment>().obstacles)
                forestObstacles.Add(new Vector3(o.x*scale,o.y*scale,o.z*scale));
        }
    }
}
