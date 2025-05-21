// This is for unity to call api functions that require a return value
// Mostly copied from CallMethodRpcBuffer.cs in unityhawk - probably could share some code
using System;
using SharedMemory;

using Plunderludics.UnityHawk.Shared;

namespace Plunderludics.UnityHawk.SharedBuffers {
public class ApiCallRpc {
    /// the rpc buffer
    RpcBuffer _rpcBuffer;

    /// the callback for this rpc
    /// (return null if and only if method call fails for some reason)
    public delegate string CallApiMethod(string methodName, string argString);

    public ApiCallRpc(string name, CallApiMethod callApiMethod) {
        _rpcBuffer = new (
            name: name,
            (msgId, payload) => {
                string returnString;
                try {
                    // Deserialize the payload to a MethodCall struct
                    MethodCall methodCall = Serialization.RawDeserialize<MethodCall>(payload);
                    string methodName = methodCall.MethodName;
                    string argString = methodCall.Argument;
                    // Console.WriteLine($"ApiCallRpc: {methodName}({argString})");
                    returnString = callApiMethod(methodName, argString);
                    
                    if (returnString == null) {
                        Console.WriteLine($"Error: ApiCallRpc {methodCall} returned null");
                        return null;
                    }
                } catch (Exception e) {
                    Console.WriteLine($"Error in ApiCallRpc: {e}");
                    return null;
                }

                byte[] returnData = System.Text.Encoding.ASCII.GetBytes(returnString);
                return returnData;
            }
        );
    }
}
}