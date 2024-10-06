// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("K8SeAbgnN4dxhkrj0tRc9QabJtj6hSrmc5GbhIj8DeFW/rUb/Kc6TGQ6l3Kd8tZWBlxUu1597+UICHgY8x2vWcGUO+fEwOJBw/Fv3bRJZj57gyfXaMdG97QsP77I6rmIBBBJn7oIi6i6h4yDoAzCDH2Hi4uLj4qJfParD7sKyDMo34pufnUJNTHmmoe9Q9582tk7ZH5rLYeymCyP5v/ingiLhYq6CIuAiAiLi4oh7eEQaGcCEFQ+0lEYXniyV6aVVS1heqt9lc/bgL6/r2aYfeQJXsMXkhbGSked72dzYnX/vj73B8vOzAra0v0EBRVEGP7uLH0Mc3X+Cs2AEcIDhoXTeWPmuICguUYFo84w6OTbpP5/ypVoOmeRf9yp9xc3L4iJi4qL");
        private static int[] order = new int[] { 4,7,13,9,5,5,8,13,13,13,10,11,12,13,14 };
        private static int key = 138;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
