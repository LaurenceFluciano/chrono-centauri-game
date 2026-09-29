using System;

[Flags]
public enum EraMask
{
    None           = 0,
    Past           = 1 << 0,
    Present        = 1 << 1,
    Future         = 1 << 2,
    
    PastPresent    = Past | Present,
    PastFuture     = Past | Future,
    PresentFuture  = Present | Future,
    AllEras        = Past | Present | Future
}

public enum Era
{
    Past,
    Present,
    Future
}