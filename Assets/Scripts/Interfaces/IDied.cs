using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IDied
    {
        void Died()
        {
            //make object rigidbody
            //if player make camera activate another follow script
            // and remove this object from Gamemanager player list
        }
    }
}