using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Yamato
{
    public class YamatoSheathFramework : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<YamatoSheathFrameworkComponent>();
        }
    }
    public class YamatoSheathFrameworkComponent : MonoBehaviour { }
}
