// This is for unity to call api functions that require a return value
// Mostly copied from CallMethodRpcBuffer.cs in unityhawk - probably could share some code
using System;
using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers {
public class ApiCallRpc {
    /// the buffer name
    string _name;

    /// the rpc buffer
    RpcBuffer _rpcBuffer;

    /// the callback for this rpc
    public delegate bool CallApiMethod(string methodName, string argString, out string output);

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

                    var exists = callApiMethod(methodName, argString, out returnString);

                    // [messy hack] don't allow returning null because it seems to break things on the other side of the RPC
                    if (returnString == null) {
                        Console.WriteLine($"Warning: {methodName} returned null but null return values are not supported, converting to empty string");
                        returnString = "";
                    }
                } catch (Exception e) {
                    Console.WriteLine($"Error in ApiCallRpc: {e}");
                    returnString = ""; // return an empty string to avoid crashing bizhawk
                }

                byte[] returnData = System.Text.Encoding.ASCII.GetBytes(returnString);
                return returnData;
            }
        );
    }
}
}