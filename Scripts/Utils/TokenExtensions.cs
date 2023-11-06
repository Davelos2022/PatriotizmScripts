using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public static class TokenExtensions
{
    public static CancellationTokenSource Refresh(this CancellationTokenSource source)
    {
        source?.Cancel();
        source?.Dispose();
        return new CancellationTokenSource();
    }
}
