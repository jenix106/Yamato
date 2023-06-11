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
