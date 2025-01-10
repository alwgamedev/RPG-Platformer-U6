namespace RPGPlatformer.Core
{
    public static class InputTools
    {
        public const string keyboardBindingPrefix = "<Keyboard>/";

        public static string KeyName(string bindingPath)
        {
            if(bindingPath.Length < keyboardBindingPrefix.Length)
            {
                return "";
            }
            return bindingPath.Remove(0, keyboardBindingPrefix.Length);
        }

        public static string ToBindingPath(string text)
        {
            return keyboardBindingPrefix + text;
        }
    }
}