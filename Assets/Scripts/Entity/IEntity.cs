using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EO
{
    public interface IEntity
    {
        public ulong EntityId { get; set; }
        public GameObject Obj { get; set; }
        public EntityType Type { get; set; }

        public EntityDef GetDef();
    }
}
