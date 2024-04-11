// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using GrpcMessageClasses;
using System.ServiceModel;

namespace GrpcServices
{
    [ServiceContract]
    public interface IExternalNamesDatabaseService
    {
        [OperationContract] PlayerNameMessage GetExternalPlayerName(PlayerMessage request);
    }
}