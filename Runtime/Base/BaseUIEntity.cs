using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.BaiZe.UIFramework
{
    public abstract class BaseUIEntity : BaseUIUnit, IUIEntity
    {
        public void Close()
        {

        }

        protected virtual void OnClosed() { }
    }
}