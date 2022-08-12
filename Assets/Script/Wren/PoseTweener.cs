using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class PoseTweener : MonoBehaviour
{

    [Range(0,1)]
    public float lerpVal;

    public Pose pose1;
    public Pose pose2;
    
    public Pose mine;

    // Update is called once per frame
    void Update()
    {
        
   mine.wingRot1_L = Vector3.Lerp(pose1.wingRot1_L,pose2.wingRot1_L , lerpVal );
   mine.wingRot2_L = Vector3.Lerp(pose1.wingRot2_L,pose2.wingRot2_L , lerpVal );
   mine.wingRot3_L = Vector3.Lerp(pose1.wingRot3_L,pose2.wingRot3_L , lerpVal );
   mine.wingRot4_L = Vector3.Lerp(pose1.wingRot4_L,pose2.wingRot4_L , lerpVal );

   mine.wingRot1_R = Vector3.Lerp(pose1.wingRot1_R,pose2.wingRot1_R , lerpVal );
   mine.wingRot2_R = Vector3.Lerp(pose1.wingRot2_R,pose2.wingRot2_R , lerpVal );
   mine.wingRot3_R = Vector3.Lerp(pose1.wingRot3_R,pose2.wingRot3_R , lerpVal );
   mine.wingRot4_R = Vector3.Lerp(pose1.wingRot4_R,pose2.wingRot4_R , lerpVal );

   mine.tailRot = Vector3.Lerp(pose1.tailRot,pose2.tailRot , lerpVal );
   mine.hipRot = Vector3.Lerp(pose1.hipRot,pose2.hipRot , lerpVal );
   mine.spineRot = Vector3.Lerp(pose1.spineRot,pose2.spineRot , lerpVal );
   mine.shoulderRot = Vector3.Lerp(pose1.shoulderRot,pose2.shoulderRot , lerpVal );
   mine.neckRot = Vector3.Lerp(pose1.neckRot,pose2.neckRot , lerpVal );
   mine.headRot = Vector3.Lerp(pose1.headRot,pose2.headRot , lerpVal );

   mine.tailRot1 = Vector3.Lerp(pose1.tailRot1,pose2.tailRot1 , lerpVal );
   mine.tailRot2 = Vector3.Lerp(pose1.tailRot2,pose2.tailRot2 , lerpVal );
   mine.tailRot3 = Vector3.Lerp(pose1.tailRot3,pose2.tailRot3 , lerpVal );
    }
}
