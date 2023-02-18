using System;

namespace Com.BaiZe.UIFramework
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UIUnitTag : Attribute
    {
        public bool isSingleton = false;
        public bool isOverMask = false;
    }
}