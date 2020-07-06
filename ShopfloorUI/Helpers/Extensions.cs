﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopfloorUI.Helpers
{
    public static class Extensions
    {
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

    }
}
