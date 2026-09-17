using UnityEngine;

public static class NetworkEventManager
{
    public delegate void NoArgs();
    public delegate void OneArg<T1>(T1 t1);
    public delegate void TwoArgs<T1, T2>(T1 t1, T2 t2);
    public delegate void ThreeArgs<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void FourArgs<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    #region Unity Events
    #region Host
    public static NoArgs OnRequestStartUnityHost;
    public static NoArgs OnRequestStopUnityHost;

    public static NoArgs OnUnityHostStarted;
    public static NoArgs OnUnityHostStopped;

    #endregion

    #region Client
    public static NoArgs OnRequestStopUnityClient;
    public static NoArgs OnRequestStartUnityClient;

    public static NoArgs OnUnityClientStarted;
    public static NoArgs OnUnityClientStopped;

    #endregion

    #endregion

}