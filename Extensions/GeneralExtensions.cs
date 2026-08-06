namespace BaseLib.Extensions;

internal static class GeneralExtensions
{
    internal static T LogChained<T>(this T obj, string logText)
    {
        BaseLibMain.Logger.Info(logText);
        return obj;
    }
    
    internal static T Chained<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
    
    internal static T ChainedGet<T, U>(this T obj, Func<T, U> getter, out U result)
    {
        result = getter(obj);
        return obj;
    }
}