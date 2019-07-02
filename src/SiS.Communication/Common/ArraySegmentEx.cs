﻿using System;
using System.Collections.Generic;

namespace SiS.Communication
{
    /// <summary>
    /// Provides simple static methods to extend the array semgent class.
    /// </summary>
    public static class ArraySegmentExtension
    {
        /// <summary>
        /// Greate a new array to save the data of the array segment.
        /// </summary>
        /// <returns>The new array that has the data.</returns>
        public static T[] ToArray<T>(this ArraySegment<T> arraySegment)
        {
            T[] newArray = new T[arraySegment.Count];
            Array.Copy(arraySegment.Array, arraySegment.Offset, newArray, 0, arraySegment.Count);
            return newArray;

        }

        /// <summary>
        /// Create a new array list to save the data of array segment collection.
        /// </summary>
        /// <returns>The new array list that has the data.</returns>
        public static List<T[]> ToArrayList<T>(this IEnumerable<ArraySegment<T>> segmentArray)
        {
            List<T[]> list = new List<T[]>();
            foreach (ArraySegment<T> seg in segmentArray)
            {
                list.Add(seg.ToArray());
            }
            return list;
        }
    }
}