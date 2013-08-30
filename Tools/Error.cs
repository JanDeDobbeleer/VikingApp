namespace Tools
{
    public static class Error
    {
        public static bool HandleError(object result, string message)
        {
            if (result != null)
                return false;
            Message.ShowToast(message);
            return true;
        }
    }
}