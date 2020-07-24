namespace T.Common
{
    public delegate void CallBack();
    public delegate void CallBack<T>(T value);
    public delegate void CallBack<T, K>(T TValue, K KValue);
}
