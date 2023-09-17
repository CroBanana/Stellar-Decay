using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Game
{
    public interface IDamageable
    {
        void TakeDmg(int dmgTaken);
    }
}