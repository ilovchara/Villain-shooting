using UnityEngine;

public static class Utility
{
    // 洗牌算法
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        for(int i = 0; i<array.Length -1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        // 返回被打乱的数组
        return array;
    }
}
