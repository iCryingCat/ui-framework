using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.BaiZe.UIFramework
{
    /// <summary>
    /// 视窗接口
    /// </summary>
    public interface IUIUnit
    {
        void Load();
        void Show();
        void Hide();
    }
}