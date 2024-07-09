using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* allows player to leave dungeon if they have the target item. Otherwise, they are teleported to a random space. */
[CreateAssetMenu(menuName = "Effects/Entity Effects/Exit Point", fileName = "entityEffect_exitPoint")]
public class ExitEffect : EntityEffect
{
    public override void Activate(Hunter hunter)
    {
        //if the player has the target item, game is over and player wins.
        //Otherwise, teleport player to a random space.
    }
}
